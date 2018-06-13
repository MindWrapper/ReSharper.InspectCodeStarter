# ReSharper.InspectCodeStarter
Simplify inspectcode.exe usage with plugins. see [ReSharper Command Line Tools](https://www.jetbrains.com/resharper/features/command-line.html)

Example:
`.\inspect c:\temp\classlib\classlib.sln --base-dir=c:\RCLTTest`

```
Package "JetBrains.ReSharper.CommandLineTools.2018.1.0" is already installed.
Feeds used:
  C:\Users\yan\.nuget\packages\
  https://resharper-plugins.jetbrains.com/api/v2/
  https://nuget.org/api/v2/

Package "ReSharper.FooDetector.2018.1.0" is already installed.
Feeds used:
  C:\Users\yan\.nuget\packages\
  https://resharper-plugins.jetbrains.com/api/v2/
  https://nuget.org/api/v2/

Package "PowerToys.CyclomaticComplexity.2018.1.0" is already installed.
JetBrains Inspect Code 2018.1
Running in 64-bit mode, .NET runtime 4.0.30319.42000 under Microsoft Windows NT 6.2.9200.0
Analyzing files

Analyzing classlib.AssemblyInfo.cs
Analyzing Class1.cs
Analyzing classlib.csproj
Inspecting Class1.cs
Inspection report was written to c:\RCLTTest\result.xml
```
