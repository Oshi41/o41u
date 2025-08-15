# Project Guidelines

## This file rules
* Keep this file up-to-date.
* Keep projects documentation up-to-date (see o41u/lib/docs/).
* Use the [Markdown](https://guides.github.com/features/mastering-markdown/) syntax.
* Use more rich formatting (headers, emoji, bold text, etc.)
* Use simple, short sentences, separate them with a blank lines.

## Code style rules
* Use default JetBrains Rider code style for c#. Use approximately:
* **120** sym / line, **4** spaces for indentation, ~**200-500** lines per file
* Always prefer readability, easy to understand \ maintain code over performance.
* **Do not repeat** code from standard libraries, **reuse existing code**.
* Always prefer existing tools instead of reinventing the wheel.
* NEVER add dependencies on external Nugget libraries not from Microsoft.

## Documents rules
* For any notable features, create a separate document in o41u/lib/docs/ in .md format. Use rules above for formatting.
* Follow the easy read principle: 20 seconds of quick look should be enough to obtain the main idea.
* Use more pretty formatting for best readability
* Include those topics for every feature: 
  * Description | Usage | Reasoning why it's useful | Examples

## Build and Configuration
.NET SDK: 9.0, LangVersion 12, Windows OS.

- Solution
  - lib (netstandard2.0): Library project, provide set of useful methods.
  - IL_Weaver (netstandard2.0): Planning IL weaving tool during compile time.
  - aot (net9.0): "HelloWorld" console app, PublishAot=true, SelfContained=true.
  - tests (net9.0): Unit tests, NUnit 4
  
- Output paths
  - All projects are building the same directory: o41u/bin/{Configuration}/{Platform}
  
- Build
  - IL_Weaver defines MSBuild target RunIlWeaver that is conditioned to run
    AfterTargets=CoreCompile for consumer projects targeting net8.0.


- Tests
    - Target on 50% of test code coverage.
    - Do not be dogma strict about the goals, use your resources wisely.
    - Tests must confirm main functionality of the feature works
    - Prefer robust tests over code coverage goal. If you starting spending time
        on tests too much, drop or exclude them to prevent diving into "the rabbit hole" of
        support time spending.
    - Run tests after reasonable code changes.


## Roadmap
- Add docs for implemented features.
- Adding more extensions for repeating coding tasks (see o41u/lib/Extensions)

- Find the way to modify **System.Reflection.Assembly** loaded from file:
  - Intercept existing method
  - Insert method before entering the method
  - Insert method after executing the method

- Investigate how to populate **System.Reflection.Assembly** to **System.Reflection.Emit.AssemblyBuilder**
  - Find robust way to copy metadata with precision
  - Test copying for large real-world managed assemblies

- Investigate how to handle signed .dll files which need to be weaved
  - Will the changed .dll be corrupted?
  - Will the weaver fail to weave the assembly?
  - How to maintain the original signature if the library was changed by weaver?

- Implement IL weaving for AOT build
  - Proof of concept
  - Test with real-world AOT apps
  - Signing and security explore

- Implement Aspect-oriented lib
  - Intercept methods \ properties \ constructors \ fields using attributes
  - Data Validation \ Logging using attributes (see o41u/lib/Helpers/Guard)

- Implement Promise.cs
  - See [original idea reference](https://github.com/luminati-io/luminati-proxy/blob/master/util/etask.js)
  - Expose API context for async methods:
    - **return(res)** - cancel the execution and override the result
    - **then(fn)** - execute the callback when the promise is resolved
    - **finally(fn)** - execute the callback when the promise is resolved or rejected
    - **cancel()** - cancel the execution of the promise
    - **catch(fn)** - execute the callback when the promise is rejected.
           Allows to handle errors smoothly
    - **wait()** - wait for the promise to be resolved or rejected
    - **sleep()** - pause current promise execution
    - **add(child)** - interrupt this promise execution, add child promise,
           waits for it's completion
    - timeout(ms) - throws timeout error after ms
    - log - expose Logger
    - expose performance API as metrics \ timeline
    - option for debouncing \ throttling
    - option for retrying
    - option for only once run executing
    - option for avoid method execution when promise is working (.single-time-run)
  
  - !Investigate!
    - invent easy way for integration. JS source use simple method wrapper which may be
         annoying in c#. IL Weaving?
    - Expose the context to the user during the **promise execution only**. JS source
         exposed it by inserting it as method argument, but better to use static
         variables

- Implement some helper code \ find the NuGet for fast build of web API usage:
  - Google, OpenAI, Claude, etc
