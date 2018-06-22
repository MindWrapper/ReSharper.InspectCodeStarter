# ReSharper.InspectCodeStarter

Simplify inspectcode.exe usage with plugins. see [ReSharper Command Line Tools](https://www.jetbrains.com/resharper/features/command-line.html). See also this [blog](http://drugalya.com/r-command-line-tools-and-compiled-extensions/)

Example:
`.\inspect.bat <Path_To_Project_Or_Soltution_Under_Inspection.csproj> --plugin=ReSharper.FooDetector.2018.1.0 --plugin=PowerToys.CyclomaticComplexity.2018.1.0 --format=html --output=res.html`

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

