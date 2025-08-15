# IO helpers — Directories, files, and globbing

Small helpers to work with files and directories quickly.

- Directory.Ensure(path?) — create directory if missing and return DirectoryInfo
- Directory.Current — DirectoryInfo of current working directory
- File.Ensure(path, defaultContent?) — create file if missing and return FileInfo
- Extensions.ListFiles(globs...) — glob search inside a DirectoryInfo
- Extensions.ReadAllText(file) — safe read with logging

## Description

These helpers wrap common I/O tasks and standardize behavior with safe defaults and logging.
They reduce boilerplate when ensuring folders/files exist and when enumerating files using glob patterns.

## Usage

```csharp
using lib.Helpers;
using lib.Extensions;

// Ensure directory exists
var di = Directory.Ensure("data/output");

// Ensure file exists with default content
var fi = File.Ensure("data/output/config.json", "{\n  \"enabled\": true\n}\n");

// List files via glob patterns (from a DirectoryInfo)
var dlls = Directory.Current.ListFiles("**/*.dll", "**/*.exe");

// Safe read file content (returns null on errors)
var text = fi?.ReadAllText();
```

## Reasoning why it's useful

- Avoid repetitive checks and creation code for directories and files.
- Glob patterns make file discovery concise and powerful.
- Safer reads with logging improve reliability in tooling and scripts.

## Examples

- Find AOT app binary in tests:
```csharp
var dlls = Directory.Current.ListFiles("**/aot.dll");
Assert.That(dlls, Is.Not.Empty);
```

- Create a folder and write default config:
```csharp
var di = Directory.Ensure("settings");
var fi = File.Ensure(Path.Combine(di!.FullName, "app.json"), "{\n}\n");
```

Notes
- Namespaces: lib.Helpers (Directory, File), lib.Extensions (ListFiles, ReadAllText)
- See also: tests\WeavingTests.cs for real usage