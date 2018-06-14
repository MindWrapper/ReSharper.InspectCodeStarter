# ReSharper.InspectCodeStarter
Simplify inspectcode.exe usage with plugins. see [ReSharper Command Line Tools](https://www.jetbrains.com/resharper/features/command-line.html)

Example:
`.\inspect.bat <Path_To_Project_Or_Soltution_Under_Inspection.csproj> --format=html --output=res.html`

```
...
Analyzing ProjectUnderInspection.AssemblyInfo.cs
Analyzing .gitignore
Analyzing FooBar.cs
Analyzing ProjectUnderInspection.csproj
Analyzing README.md
Analyzing ProjectUnderInspection.csproj
Inspecting FooBar.cs
Inspection report was written to c:\unity\RCLTTest\res.html
```

