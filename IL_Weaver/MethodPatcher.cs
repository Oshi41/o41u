using System;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

namespace IL_Weaver;

/// <summary>
/// Minimal in-place IL method body patcher.
/// - Locates a method by Type full name and Method name via SRM
/// - Replaces the body with a tiny header method that returns default
/// - Supports return types: void, reference-like, primitives (bool/char/i1/u1/i2/u2/i4/u4/i8/u8/r4/r8, native int)
/// - Skips methods that have EH sections or unsupported return types
/// - Does not alter metadata; body must fit into original body span (header+code)
/// </summary>
internal static class MethodPatcher
{
    private const byte Cee_Ret = 0x2A;
    private const byte Cee_Ldc_I4_0 = 0x16;
    private const byte Cee_Conv_I8 = 0x69;
    private const byte Cee_Conv_I = 0xD3; // conv.i
    private const byte Cee_Ldc_R4 = 0x22; // followed by 4 bytes
    private const byte Cee_Ldc_R8 = 0x23; // followed by 8 bytes
    private const byte Cee_Ldnull = 0x14;

    // ECMA-335 element type constants (subset)
    private const byte ET_VOID = 0x01;
    private const byte ET_BOOLEAN = 0x02;
    private const byte ET_CHAR = 0x03;
    private const byte ET_I1 = 0x04;
    private const byte ET_U1 = 0x05;
    private const byte ET_I2 = 0x06;
    private const byte ET_U2 = 0x07;
    private const byte ET_I4 = 0x08;
    private const byte ET_U4 = 0x09;
    private const byte ET_I8 = 0x0A;
    private const byte ET_U8 = 0x0B;
    private const byte ET_R4 = 0x0C;
    private const byte ET_R8 = 0x0D;
    private const byte ET_STRING = 0x0E;
    private const byte ET_PTR = 0x0F;
    private const byte ET_BYREF = 0x10;
    private const byte ET_VALUETYPE = 0x11;
    private const byte ET_CLASS = 0x12;
    private const byte ET_VAR = 0x13;
    private const byte ET_ARRAY = 0x14;
    private const byte ET_GENERICINST = 0x15;
    private const byte ET_TYPEDBYREF = 0x16;
    private const byte ET_I = 0x18;
    private const byte ET_U = 0x19;
    private const byte ET_OBJECT = 0x1C;
    private const byte ET_SZARRAY = 0x1D;
    private const byte ET_MVAR = 0x1E;

    /// <summary>
    /// Replace method body with a default-return tiny method.
    /// </summary>
    internal static bool TryReplaceWithDefault(
        string inputPath,
        string outputPath,
        string typeFullName,
        string methodName,
        out string? reason)
    {
        reason = null;
        using var fs = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var peReader = new PEReader(fs, PEStreamOptions.LeaveOpen);

        var md = peReader.GetMetadataReader();

        // Find type by full name (Namespace.TypeName)
        var typeHandle = md.TypeDefinitions
            .Select(h => (h, md.GetTypeDefinition(h)))
            .FirstOrDefault(t => string.Equals(
                GetFullName(md, t.Item2), typeFullName, StringComparison.Ordinal));

        if (typeHandle.h.IsNil)
        {
            reason = $"Type not found: {typeFullName}";
            return false;
        }

        // Find method by simple name (first matching)
        var typeDef = typeHandle.Item2;
        var methodHandle = typeDef.GetMethods()
            .Select(h => (h, md.GetMethodDefinition(h)))
            .FirstOrDefault(m => string.Equals(md.GetString(m.Item2.Name), methodName, StringComparison.Ordinal)).h;

        if (methodHandle.IsNil)
        {
            reason = $"Method not found: {typeFullName}.{methodName}";
            return false;
        }

        var methodDef = md.GetMethodDefinition(methodHandle);
        var sigReader = md.GetBlobReader(methodDef.Signature);

        // Parse signature header and parameter count
        var sigHeader = sigReader.ReadSignatureHeader();
        _ = sigReader.ReadCompressedInteger(); // paramCount (unused)

        // Parse return type element kind (skip simple wrappers like byref)
        var retKind = ReadReturnElementType(ref sigReader);

        // Resolve RVA to file offset
        var rva = methodDef.RelativeVirtualAddress;
        if (rva == 0)
        {
            reason = "Method has no RVA (abstract/external).";
            return false;
        }

        var bodyOffset = RvaToOffset(peReader.PEHeaders, rva);
        if (bodyOffset < 0)
        {
            reason = "Unable to map method RVA to file offset.";
            return false;
        }

        // Copy file bytes into memory to patch
        var bytes = File.ReadAllBytes(inputPath);
        var span = bytes.AsSpan();

        // Parse original header to determine available span and ensure no EH sections
        if (!TryReadMethodBodyHeader(span.Slice(bodyOffset), out var origIsFat, out var origHeaderSize, out var origCodeSize, out var hasSections))
        {
            reason = "Unsupported or malformed method body header.";
            return false;
        }

        if (hasSections)
        {
            reason = "Method has extra sections (e.g., exception handlers); in-place patch is not supported.";
            return false;
        }

        var originalSpanSize = checked(origHeaderSize + origCodeSize);

        // Compose new method body (header + code) using SRM encoders
        var newBody = ComposeDefaultBodyEncoded(retKind, out var supported);
        if (!supported || newBody.Length == 0)
        {
            reason = "Unsupported return type for default return patch.";
            return false;
        }

        // Trim any trailing alignment padding produced by the encoder
        if (!TryReadMethodBodyHeader(newBody, out _, out var newHeaderSize, out var newCodeSize, out _))
        {
            reason = "Failed to read generated method body header.";
            return false;
        }
        var effectiveNewSize = newHeaderSize + newCodeSize;
        if (effectiveNewSize > originalSpanSize)
        {
            reason = $"New body ({effectiveNewSize} bytes) exceeds original span ({originalSpanSize} bytes).";
            return false;
        }

        // Write header+code
        var dst = span.Slice(bodyOffset, originalSpanSize);
        new ReadOnlySpan<byte>(newBody, 0, effectiveNewSize).CopyTo(dst);

        // Pad remaining with zeros for readability and determinism
        for (var i = effectiveNewSize; i < originalSpanSize; i++) dst[i] = 0x00;

        // Save to output path
        var outDir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(outDir) && !Directory.Exists(outDir)) Directory.CreateDirectory(outDir);
        File.WriteAllBytes(outputPath, bytes);
        return true;
    }

    private static string GetFullName(MetadataReader md, TypeDefinition td)
    {
        var ns = md.GetString(td.Namespace);
        var name = md.GetString(td.Name);
        return string.IsNullOrEmpty(ns) ? name : ns + "." + name;
    }

    private static int RvaToOffset(PEHeaders headers, int rva)
    {
        foreach (var section in headers.SectionHeaders)
        {
            var start = section.VirtualAddress;
            var size = Math.Max(section.SizeOfRawData, section.VirtualSize);
            if (rva >= start && rva < start + size)
            {
                return (rva - start) + section.PointerToRawData;
            }
        }
        return -1;
    }

    private static bool TryReadMethodBodyHeader(ReadOnlySpan<byte> src, out bool isFat, out int headerSize, out int codeSize, out bool hasSections)
    {
        if (src.Length < 1) { isFat = false; headerSize = 0; codeSize = 0; hasSections = false; return false; }
        var b0 = src[0];
        var kind = b0 & 0x3;
        if (kind == 0x2) // Tiny
        {
            isFat = false;
            headerSize = 1;
            codeSize = b0 >> 2;
            hasSections = false;
            return true;
        }

        if (src.Length < 12) { isFat = true; headerSize = 0; codeSize = 0; hasSections = false; return false; }
        // Fat: 12-byte header
        // 0..1: Flags (low 12 bits), Size in DWords (high 4 bits)
        ushort flagsAndSize = (ushort)(src[0] | (src[1] << 8));
        var flags = flagsAndSize & 0x0FFF;
        var sizeDWords = (flagsAndSize >> 12) & 0xF;
        isFat = true;
        headerSize = sizeDWords * 4;
        codeSize = (int)(src[4] | (src[5] << 8) | (src[6] << 16) | (src[7] << 24));
        hasSections = (flags & 0x08) != 0; // CorILMethod_MoreSects
        return headerSize >= 12;
    }

    private static byte ReadReturnElementType(ref BlobReader sigReader)
    {
        // Return type can be prefixed by ByRef, CustomMod, etc. We skip BYREF and PTR once.
        byte et = sigReader.ReadByte();
        if (et == ET_BYREF || et == ET_PTR)
        {
            et = sigReader.ReadByte();
        }
        return et;
    }

    private static byte[] ComposeDefaultBody(byte retKind, out bool supported)
    {
        supported = true;
        switch (retKind)
        {
            case ET_VOID:
                return new[] { Cee_Ret };
            case ET_BOOLEAN:
            case ET_CHAR:
            case ET_I1:
            case ET_U1:
            case ET_I2:
            case ET_U2:
            case ET_I4:
            case ET_U4:
                return new[] { Cee_Ldc_I4_0, Cee_Ret };
            case ET_I8:
            case ET_U8:
                return new[] { Cee_Ldc_I4_0, Cee_Conv_I8, Cee_Ret };
            case ET_R4:
                {
                    var buf = new byte[1 + 4 + 1];
                    buf[0] = Cee_Ldc_R4;
                    // 0f 00 00 00 little-endian for 0.0f
                    var fbytes = BitConverter.GetBytes(0.0f);
                    fbytes.CopyTo(buf, 1);
                    buf[5] = Cee_Ret;
                    return buf;
                }
            case ET_R8:
                {
                    var buf = new byte[1 + 8 + 1];
                    buf[0] = Cee_Ldc_R8;
                    var dbytes = BitConverter.GetBytes(0.0);
                    dbytes.CopyTo(buf, 1);
                    buf[9] = Cee_Ret;
                    return buf;
                }
            case ET_I: // native int
            case ET_U:
                return new[] { Cee_Ldc_I4_0, Cee_Conv_I, Cee_Ret };

            // Treat reference-like as null return
            case ET_STRING:
            case ET_CLASS:
            case ET_OBJECT:
            case ET_ARRAY:
            case ET_SZARRAY:
            case ET_VAR:
            case ET_MVAR:
            case ET_PTR: // if not ByRef to value type, treat as managed pointer; ldnull
            case ET_GENERICINST:
                return new[] { Cee_Ldnull, Cee_Ret };

            default:
                supported = false;
                return Array.Empty<byte>();
        }
    }

    private static byte[] ComposeDefaultBodyEncoded(byte retKind, out bool supported)
    {
        supported = true;
        var code = new BlobBuilder();
        var il = new InstructionEncoder(code);
        switch (retKind)
        {
            case ET_VOID:
                il.OpCode(ILOpCode.Ret);
                break;
            case ET_BOOLEAN:
            case ET_CHAR:
            case ET_I1:
            case ET_U1:
            case ET_I2:
            case ET_U2:
            case ET_I4:
            case ET_U4:
                il.LoadConstantI4(0);
                il.OpCode(ILOpCode.Ret);
                break;
            case ET_I8:
            case ET_U8:
                il.LoadConstantI4(0);
                il.OpCode(ILOpCode.Conv_i8);
                il.OpCode(ILOpCode.Ret);
                break;
            case ET_R4:
                il.LoadConstantI4(0);
                il.OpCode(ILOpCode.Conv_r4);
                il.OpCode(ILOpCode.Ret);
                break;
            case ET_R8:
                il.LoadConstantI4(0);
                il.OpCode(ILOpCode.Conv_r8);
                il.OpCode(ILOpCode.Ret);
                break;
            case ET_I: // native int
            case ET_U:
                il.LoadConstantI4(0);
                il.OpCode(ILOpCode.Conv_i);
                il.OpCode(ILOpCode.Ret);
                break;

            // Treat reference-like as null return
            case ET_STRING:
            case ET_CLASS:
            case ET_OBJECT:
            case ET_ARRAY:
            case ET_SZARRAY:
            case ET_VAR:
            case ET_MVAR:
            case ET_PTR: // if not ByRef to value type, treat as managed pointer; ldnull
            case ET_GENERICINST:
                il.OpCode(ILOpCode.Ldnull);
                il.OpCode(ILOpCode.Ret);
                break;

            default:
                supported = false;
                return Array.Empty<byte>();
        }

        var codeBytes = code.ToArray();
        if (codeBytes.Length > 63)
        {
            supported = false;
            return Array.Empty<byte>();
        }

        var result = new byte[1 + codeBytes.Length];
        result[0] = (byte)((codeBytes.Length << 2) | 0x2); // tiny header
        Buffer.BlockCopy(codeBytes, 0, result, 1, codeBytes.Length);
        return result;
    }
}
