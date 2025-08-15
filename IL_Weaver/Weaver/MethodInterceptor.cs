using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace IL_Weaver.Weaver
{
    /// <summary>
    /// netstandard2.0-friendly method interceptor using DynamicMethod.
    /// It does NOT modify MethodInfo in-place. Instead, it builds a wrapper delegate
    /// that calls optional OnEnter/OnExit/OnException callbacks around the original method call.
    ///
    /// Limitations (kept minimal by design):
    /// - Does not support methods on value-type instances (struct instance methods).
    /// - Does not support open generic methods or by-ref return types.
    /// - Supports by-ref parameters only for forwarding to the original call; argument snapshot array will box current values (no ref back-propagation in the snapshot).
    /// - The wrapper returned is an OPEN delegate: for instance methods, the first parameter is the instance.
    /// - Delegate shape construction uses Expression helper types limits (max parameters ~16-17). For many-arg methods this may fail.
    ///
    /// If you need a closed delegate for an instance, you can partially apply/bind the instance yourself (e.g., via closures) and forward arguments.
    /// </summary>
    public static class MethodInterceptor
    {
        /// <summary>
        /// Hooks invoked by the interceptor wrapper.
        /// Any of these can be null to skip that callback.
        /// </summary>
        public sealed class InterceptorHooks
        {
            public Action<InterceptorState, object, object[]> OnEnter { get; set; }
            public Action<InterceptorState, object, object, object[]> OnExit { get; set; }
            public Action<InterceptorState, object, Exception, object[]> OnException { get; set; }
        }

        /// <summary>
        /// State passed to callbacks; contains the original method being wrapped and an optional user tag.
        /// </summary>
        public sealed class InterceptorState
        {
            public InterceptorState(MethodInfo method, object tag = null)
            {
                Method = method ?? throw new ArgumentNullException(nameof(method));
                Tag = tag;
            }

            public MethodInfo Method { get; }
            public object Tag { get; }
            public InterceptorHooks Hooks { get; internal set; }
        }

        /// <summary>
        /// Static runtime that actually invokes the hooks. Keeping this separate simplifies IL emission.
        /// </summary>
        public static class InterceptorRuntime
        {
            public static void OnEnter(InterceptorState state, object instance, object[] args)
            {
                var h = state?.Hooks;
                h?.OnEnter?.Invoke(state, instance, args);
            }

            public static void OnExit(InterceptorState state, object instance, object returnValue, object[] args)
            {
                var h = state?.Hooks;
                h?.OnExit?.Invoke(state, instance, returnValue, args);
            }

            public static void OnException(InterceptorState state, object instance, Exception ex, object[] args)
            {
                var h = state?.Hooks;
                h?.OnException?.Invoke(state, instance, ex, args);
            }
        }

        /// <summary>
        /// Creates an OPEN delegate wrapper around the specified <paramref name="method"/>.
        /// The wrapper invokes hooks at start/end/exception and then calls the original.
        ///
        /// - For instance methods: the first parameter of the returned delegate is the instance (object of declaring type or subtype).
        /// - For static methods: the parameters match the method parameters.
        ///
        /// Use <paramref name="state"/> to carry MethodInfo and a custom tag; its Hooks will be used for callbacks.
        /// </summary>
        /// <param name="method">Closed MethodInfo (no generic parameters).</param>
        /// <param name="hooks">Callbacks to invoke (any may be null).</param>
        /// <param name="state">Optional state; if null, a new one is created.</param>
        /// <returns>A Delegate you can invoke as an open delegate (instance first if instance method).</returns>
        public static Delegate CreateOpenMethodWrapper(MethodInfo method, InterceptorHooks hooks, InterceptorState state = null)
        {
            if (method == null) throw new ArgumentNullException(nameof(method));
            if (hooks == null) throw new ArgumentNullException(nameof(hooks));

            if (method.ContainsGenericParameters)
                throw new NotSupportedException("Open generic methods are not supported.");

            var declType = method.DeclaringType;
            if (!method.IsStatic)
            {
                if (declType == null)
                    throw new InvalidOperationException("Method has no declaring type.");
                if (declType.IsValueType)
                    throw new NotSupportedException("Instance methods on value types are not supported in this minimal implementation.");
            }

            var parameters = method.GetParameters();
            var openParamTypes = (method.IsStatic
                ? parameters.Select(p => p.ParameterType)
                : new[] { declType }.Concat(parameters.Select(p => p.ParameterType)))
                .ToArray();

            // The DynamicMethod will have an extra first parameter for InterceptorState which we will CLOSE over.
            var dynParamTypes = new[] { typeof(InterceptorState) }.Concat(openParamTypes).ToArray();
            var returnType = method.ReturnType;

            // Owner module: use declaring module if accessible; otherwise, our own module.
            var ownerModule = typeof(MethodInterceptor).Module;
            var dm = new DynamicMethod(
                name: $"InterceptorWrapper_{method.Name}",
                returnType: returnType,
                parameterTypes: dynParamTypes,
                m: ownerModule,
                skipVisibility: true);

            var il = dm.GetILGenerator();

            // Locals
            LocalBuilder argsLocal = il.DeclareLocal(typeof(object[]));
            LocalBuilder retLocal = returnType == typeof(void) ? null : il.DeclareLocal(returnType);
            LocalBuilder exLocal = il.DeclareLocal(typeof(Exception));

            // Build object[] args snapshot
            int paramCount = parameters.Length;
            il.Emit(OpCodes.Ldc_I4, paramCount);
            il.Emit(OpCodes.Newarr, typeof(object));
            il.Emit(OpCodes.Stloc, argsLocal);

            // Fill array with boxed args
            for (int i = 0; i < paramCount; i++)
            {
                il.Emit(OpCodes.Ldloc, argsLocal);           // arr
                il.Emit(OpCodes.Ldc_I4, i);                  // index

                int dynIndex = 1 /*state*/ + (method.IsStatic ? 0 : 1) + i; // position of this arg in DynamicMethod params
                il.Emit(OpCodes.Ldarg, dynIndex);

                var pType = parameters[i].ParameterType;
                if (pType.IsByRef)
                {
                    var elemType = pType.GetElementType();
                    // Load ref value
                    OpCode ldind = GetLdindOp(elemType);
                    il.Emit(ldind);
                    if (elemType.IsValueType)
                        il.Emit(OpCodes.Box, elemType);
                }
                else
                {
                    if (pType.IsValueType)
                        il.Emit(OpCodes.Box, pType);
                }

                il.Emit(OpCodes.Stelem_Ref);
            }

            // Begin try block
            var tryBlock = il.BeginExceptionBlock();

            // OnEnter(state, instance, args)
            il.Emit(OpCodes.Ldarg_0); // state
            if (method.IsStatic)
            {
                il.Emit(OpCodes.Ldnull);
            }
            else
            {
                il.Emit(OpCodes.Ldarg_1); // instance
            }
            il.Emit(OpCodes.Ldloc, argsLocal);
            il.Emit(OpCodes.Call, typeof(InterceptorRuntime).GetMethod(nameof(InterceptorRuntime.OnEnter)));

            // Call original
            if (!method.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_1); // instance
            }
            // Load each argument (by-ref will pass the ref as-is)
            for (int i = 0; i < paramCount; i++)
            {
                int dynIndex = 1 /*state*/ + (method.IsStatic ? 0 : 1) + i;
                il.Emit(OpCodes.Ldarg, dynIndex);
            }

            il.Emit(method.IsStatic || !method.IsVirtual ? OpCodes.Call : OpCodes.Callvirt, method);

            if (returnType != typeof(void))
            {
                il.Emit(OpCodes.Stloc, retLocal);
            }

            // OnExit(state, instance, return, args)
            il.Emit(OpCodes.Ldarg_0); // state
            if (method.IsStatic) il.Emit(OpCodes.Ldnull); else il.Emit(OpCodes.Ldarg_1);
            if (returnType == typeof(void))
            {
                il.Emit(OpCodes.Ldnull);
            }
            else if (returnType.IsValueType)
            {
                il.Emit(OpCodes.Ldloc, retLocal);
                il.Emit(OpCodes.Box, returnType);
            }
            else
            {
                il.Emit(OpCodes.Ldloc, retLocal);
            }
            il.Emit(OpCodes.Ldloc, argsLocal);
            il.Emit(OpCodes.Call, typeof(InterceptorRuntime).GetMethod(nameof(InterceptorRuntime.OnExit)));

            // catch (Exception ex)
            il.BeginCatchBlock(typeof(Exception));
            il.Emit(OpCodes.Stloc, exLocal);
            il.Emit(OpCodes.Ldarg_0); // state
            if (method.IsStatic) il.Emit(OpCodes.Ldnull); else il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldloc, exLocal);
            il.Emit(OpCodes.Ldloc, argsLocal);
            il.Emit(OpCodes.Call, typeof(InterceptorRuntime).GetMethod(nameof(InterceptorRuntime.OnException)));
            il.Emit(OpCodes.Rethrow);

            il.EndExceptionBlock();

            // Return
            if (returnType == typeof(void))
            {
                il.Emit(OpCodes.Ret);
            }
            else
            {
                il.Emit(OpCodes.Ldloc, retLocal);
                il.Emit(OpCodes.Ret);
            }

            // Create state and bind hooks
            var st = state ?? new InterceptorState(method);
            st.Hooks = hooks;

            // Build delegate type matching the OPEN signature (instance first if instance method)
            var delegateType = DelegateTypeFactory.CreateOpenDelegateType(openParamTypes, returnType);

            // Close over InterceptorState (first dyn param). The resulting delegate is OPEN for the original method signature.
            return dm.CreateDelegate(delegateType, st);
        }

        private static OpCode GetLdindOp(Type t)
        {
            if (!t.IsValueType) return OpCodes.Ldind_Ref;
            if (t == typeof(sbyte)) return OpCodes.Ldind_I1;
            if (t == typeof(byte)) return OpCodes.Ldind_U1;
            if (t == typeof(short)) return OpCodes.Ldind_I2;
            if (t == typeof(ushort)) return OpCodes.Ldind_U2;
            if (t == typeof(int)) return OpCodes.Ldind_I4;
            if (t == typeof(uint)) return OpCodes.Ldind_U4;
            if (t == typeof(long)) return OpCodes.Ldind_I8;
            if (t == typeof(ulong)) return OpCodes.Ldind_I8; // no unsigned variant; loads as I8
            if (t == typeof(float)) return OpCodes.Ldind_R4;
            if (t == typeof(double)) return OpCodes.Ldind_R8;
            if (t.IsEnum) return GetLdindOp(Enum.GetUnderlyingType(t));
            return OpCodes.Ldobj; // struct
        }

        private static class DelegateTypeFactory
        {
            public static Type CreateOpenDelegateType(Type[] parameterTypes, Type returnType)
            {
                // Try to use System.Linq.Expressions factory helpers which support up to 16 parameters for Action/Func.
                if (returnType == typeof(void))
                {
                    switch (parameterTypes.Length)
                    {
                        case 0: return typeof(Action);
                        case 1: return typeof(Action<>).MakeGenericType(parameterTypes);
                        default:
                            var actionGeneric = Type.GetType($"System.Action`{parameterTypes.Length}");
                            if (actionGeneric == null)
                                throw new NotSupportedException("Too many parameters for Action delegate.");
                            return actionGeneric.MakeGenericType(parameterTypes);
                    }
                }
                else
                {
                    var all = parameterTypes.Concat(new[] { returnType }).ToArray();
                    switch (parameterTypes.Length)
                    {
                        case 0: return typeof(Func<>).MakeGenericType(all);
                        case 1: return typeof(Func<,>).MakeGenericType(all);
                        case 2: return typeof(Func<,,>).MakeGenericType(all);
                        case 3: return typeof(Func<,,,>).MakeGenericType(all);
                        case 4: return typeof(Func<,,,,>).MakeGenericType(all);
                        case 5: return typeof(Func<,,,,,>).MakeGenericType(all);
                        case 6: return typeof(Func<,,,,,,>).MakeGenericType(all);
                        case 7: return typeof(Func<,,,,,,,>).MakeGenericType(all);
                        case 8: return typeof(Func<,,,,,,,,>).MakeGenericType(all);
                        case 9: return typeof(Func<,,,,,,,,,>).MakeGenericType(all);
                        case 10: return typeof(Func<,,,,,,,,,,>).MakeGenericType(all);
                        case 11: return typeof(Func<,,,,,,,,,,,>).MakeGenericType(all);
                        case 12: return typeof(Func<,,,,,,,,,,,,>).MakeGenericType(all);
                        case 13: return typeof(Func<,,,,,,,,,,,,,>).MakeGenericType(all);
                        case 14: return typeof(Func<,,,,,,,,,,,,,,>).MakeGenericType(all);
                        case 15: return typeof(Func<,,,,,,,,,,,,,,,>).MakeGenericType(all);
                        default:
                            throw new NotSupportedException("Too many parameters for Func delegate.");
                    }
                }
            }
        }
    }
}
