o41u – Project-specific Development Guidelines

Audience: Experienced .NET developers working on this repository.

1. Build and Configuration
- Toolchain
  - .NET SDK: 9.0 (LangVersion 12 is used across projects).
  - OS: Repository and CI assume Windows; paths and example commands use PowerShell with backslashes.
- Solution layout and TFMs
  - lib (o41u.lib): netstandard2.0. Nullable enabled, implicit usings generally disabled unless specified. Produces library consumed by other projects.
  - IL_Weaver (o41u.IL_Weaver): netstandard2.0. Uses System.Reflection.Metadata and System.Reflection.Emit.* to inspect/emit IL.
  - app (o41u-app): net9.0 console app; Release uses single-file publish options.
  - aot (o41u.aot): net9.0 console app with PublishAot=true, SelfContained=true (win-x64 by default via SDK resolution). Debug/Release OutputPath is overridden to ..\bin\<Configuration>\.
  - tests (o41u.tests): net9.0 using NUnit 4, NUnit3TestAdapter, Microsoft.NET.Test.Sdk. References lib and IL_Weaver.
- Output paths
  - Several projects override OutputPath to project_root\bin\<Configuration>\.
  - Expect binaries in: bin\Debug\ and bin\Release\ plus project-specific subdirectories (e.g., aot also has aot\bin\...).
- Build
  - dotnet build .\o41u.sln -c Debug
  - For Release (single-file app, AOT constraints): dotnet build .\o41u.sln -c Release
  - Note: IL_Weaver defines MSBuild target RunIlWeaver that is conditioned to run AfterTargets=CoreCompile for consumer projects targeting net8.0. Current apps/tests target net9.0, so this target will not fire automatically. If you want automatic weaving for consumers, either retarget to net8.0 or adjust the target Condition to include net9.0.

2. Testing
- Frameworks and runners
  - Framework: NUnit 4.x; Runner: Microsoft.NET.Test.Sdk with NUnit3TestAdapter.
  - Target framework: net9.0.
- Running tests
  - All tests in solution: dotnet test .\o41u.sln -c Debug
  - Project-level: dotnet test .\tests\tests.csproj -c Debug
  - Filter by fully-qualified name (FQN):
    - dotnet test .\tests\tests.csproj -c Debug --filter FullyQualifiedName=tests.HelpersTests.CommonComparer_CastAdapter_Works
- Known behaviors and prerequisites
  - WeavingTests.LoadAotManagedDll expects to find a file named aot.dll somewhere under the repo. The aot project builds o41u.aot.dll by default. To satisfy this test one of the following must be done before running tests:
    - Build aot, then copy or rename the output DLL to aot.dll within the repository tree. Example (PowerShell):
      - dotnet build .\aot\aot.csproj -c Debug
      - Copy-Item .\aot\bin\Debug\net9.0\o41u.aot.dll .\bin\Debug\aot.dll
    - Or adjust the test to look for o41u.aot.dll instead (not recommended unless you intend to change the test contract).
  - Some PromiseTests currently fail (as of 2025-08-15):
    - PromiseContext_Current_WorksCorrectly (expects "Started" but returns "Should not reach here").
    - Promise_AddChild_WaitsForChildren (expects false, observed true).
    - Investigate lib\Promises implementation for context handling and completion semantics if you work on this area.
- Adding a new test (example)
  - Create a test file under tests with a [TestFixture] and [Test] method. Example used locally:
    - File: tests\DocExampleTests.cs
      using NUnit.Framework;
      namespace tests;
      [TestFixture]
      public class DocExampleTests
      {
          [Test]
          public void Addition_Works()
          {
              Assert.That(2 + 2, Is.EqualTo(4));
          }
      }
  - Run a single test for a quick check:
    - dotnet test .\tests\tests.csproj -c Debug --filter FullyQualifiedName=tests.DocExampleTests.Addition_Works
  - Remove temporary/example tests before committing unless they add value.

3. Development Notes and Conventions
- Language and nullability
  - LangVersion 12; Nullable enabled across projects; prefer explicit nullability annotations and standard patterns for optional values.
- Logging and diagnostics
  - lib depends on Serilog and enrichers (Sensitive, Thread, WithCaller). When adding logs, use structured logging with minimal allocations in hot paths.
- File system utilities
  - lib.Helpers.Directory and lib.Helpers.File provide Ensure(...) helpers and globbing via Microsoft.Extensions.FileSystemGlobbing. Prefer these utilities over ad-hoc IO when working inside the repo to align with existing tests (see HelpersTests for expected behaviors).
- Comparisons and guards
  - lib.Extensions.CommonComparer implements non-trivial semantics for IDictionary/IEnumerable comparisons (e.g., equality vs set containment, special handling of nulls). Consult tests\HelpersTests.cs when changing it to avoid regressions.
  - Guard and GuardResult encapsulate common validation patterns. GuardResult has implicit conversions to/from bool/string and supports CheckAndThrow(). See tests for usage expectations.
- IL weaving internals
  - IL_Weaver emits default method bodies for various return types using System.Reflection.Metadata.
  - The MSBuild target RunIlWeaver is currently conditioned for net8.0 consumers only:
    <Target Name="RunIlWeaver" AfterTargets="CoreCompile" BeforeTargets="PrepareForILLink;PublishAot" Condition="'$(MSBuildProjectName)' != 'IL_Weaver' and '$(TargetFramework)' == 'net8.0'">
    If you need it for net9.0, expand the Condition accordingly.
- AOT app specifics
  - aot.csproj sets PublishAot=true and SelfContained=true. For AOT publish, prefer dotnet publish .\aot\aot.csproj -c Release -r win-x64 /p:PublishAot=true.
  - Unit tests do not require full native AOT publish, but WeavingTests assumes the presence of a managed DLL named aot.dll for metadata loading.

4. Quickstart
- Build: dotnet build .\o41u.sln -c Debug
- Run tests: dotnet test .\tests\tests.csproj -c Debug
- Make WeavingTests pass (optional):
  - dotnet build .\aot\aot.csproj -c Debug
  - Copy-Item .\aot\bin\Debug\net9.0\o41u.aot.dll .\bin\Debug\aot.dll
- Add a new test: create a [TestFixture] in tests, run it via FQN filter as shown above.

5. Housekeeping
- Keep OutputPath overrides in csproj files in mind when writing scripts. Many artifacts land in repo_root\bin\<Configuration> in addition to per-project bin folders.
- Avoid leaving temporary files or tests in the repo. Remove any throwaway diagnostics/tests after use.
