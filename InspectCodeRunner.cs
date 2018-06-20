using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Mono.Options;

namespace RCLTStarter
{
    class InspectCodeRunner
    {
        string m_BaseDir;
        string m_OutPutFilePath;
        string m_PluginsInstallDir;
        List<string> m_InspectCodeOptions;
        const string k_SdkVersion = "2018.1.0";
        List<string> m_PluginsSpecifiedInCommandLine;
        string m_CachesHome;

        public InspectCodeRunner(string[] args)
        {
            m_PluginsSpecifiedInCommandLine = new List<string>();

            var options = new OptionSet {
                { "base-dir=", "Consist of two folders: 1. JetBrains.ReSharper.CommandLineTools with tools executable. 'packages' contains code analysis packages", v => m_BaseDir = v },
                { "o=|output=", "Files contains 'inspectcode' output. Default: $base-dir\\result.xml", v => m_OutPutFilePath = v },
                { "p=|plugin=", $"Plugin id with version. E.g 'ReSharper.FooDetector.2018.0.1'. If version part is not specified, {k_SdkVersion} is used.", v => m_PluginsSpecifiedInCommandLine.Add(v)}
            };
            m_InspectCodeOptions = options.Parse(args);

            if (string.IsNullOrEmpty(m_BaseDir))
            {
                m_BaseDir = Environment.CurrentDirectory;
            }

            m_BaseDir = Path.GetFullPath(m_BaseDir);
            if (string.IsNullOrEmpty(m_OutPutFilePath))
            {
                m_OutPutFilePath = Path.Combine(m_BaseDir, "result.xml");
            }
            m_InspectCodeOptions.Add($"--output={m_OutPutFilePath}");

            // Use unique location for cache, otherwise 'inspectcode' might not pickup newly installed plugins
            m_CachesHome = GetTemporaryDirectory();
            m_InspectCodeOptions.Add($"--caches-home={m_CachesHome}");
            m_PluginsInstallDir = Path.Combine(m_BaseDir, "Plugins");
        }

        public void Run()
        {
            // After installation is complete inspectcode.exe located in base_dir\JetBrains.ReSharper.CommandLineTools\tools\inspectcode.exe
            InstallReSharperCommandLineTools();

            // Uses NuGet to install packages to base_dir\Plugins
            var codeAnalysisPlugins = InstallCodeAnalysisPlugins();

            // Runs 'inspectcode.exe' on specified project or solution.
            // Look for comments inside 'SetupAdditionalDeployerPackagesEnvVar' for extra details
            RunInspectCode(codeAnalysisPlugins);

            DeleteTemporaryCacheFolder();
        }

        void DeleteTemporaryCacheFolder()
        {
            Console.WriteLine($"Deleting temporary cache folder {m_CachesHome}");
            Directory.Delete(m_CachesHome, true);
        }

        private void InstallReSharperCommandLineTools()
        {
            var sources = new[] { "https://nuget.org/api/v2/" };
            var installer = new NuGetPackagesInstaller(sources, m_BaseDir);

            var packages = new[]
            {
                new PackageInfo { Id = "JetBrains.ReSharper.CommandLineTools", Version = k_SdkVersion},
            };
            installer.InstallPackages(packages);
        }

        private List<string> InstallCodeAnalysisPlugins()
        {
            var sources = new[] { "https://resharper-plugins.jetbrains.com/api/v2/", "https://nuget.org/api/v2/" };
            var packages = new List<PackageInfo>();
            foreach (var p in m_PluginsSpecifiedInCommandLine)
            {
                var match = Regex.Match(p, @"\d+(\s*\.\s*\d+){0,3}$");
                if (match.Success)
                {
                    var version = match.Groups[0].Value;
                    packages.Add(new PackageInfo { Id = p, Version = version });
                }
                else
                {
                    packages.Add(new PackageInfo { Id = p, Version = k_SdkVersion});
                }
            }
            var installer = new NuGetPackagesInstaller(sources, m_PluginsInstallDir);
            return installer.InstallPackages(packages);
        }

        private void SetupAdditionalDeployerPackagesEnvVar(List<string> installedPackages)
        {
            /*
                Forms XML and writes it JET_ADDITIONAL_DEPLOYED_PACKAGES environment variable.
                Each 'Folder' contains a path to an expanded nuget package
                Example:

                <Packages>
                    <Folder Path="c:\RCLTTest\ReSharper.FooDetector" />
                    <Folder Path="c:\RCLTTest\PowerToys.CyclomaticComplexity" />
                </Packages>

                See documention JetBrains.Application.Environment.AdditionalDeployedPackages.Schema
                in 'JetBrains.ReSharper.CommandLineTools\tools\JetBrains.Platform.Shell.xml' 

                Using 'Folder' was the only way I could make it work. It would have been possible to 
                use 'File' element, but it would require to write code for downloading nupukg instead
                of utilizing the full power of nuget. Other options, described in `JetBrains.Platform.Shell.xml` like DownloadId,

                Each 'Folder' contains and an expanded package content and a nupkg file. 
                `inspectcode` ignores expanded package content, instead it picks up a nupkg  
                and  expand it somewhere into  %LocalAppData%\JetBrains\Shared\vAny\DeployedPackagesExpand\

                I would expect 'inspectcode' to use a package contents which aldready expanded to 'Folder', expanding nupkg again.
                As it brings some small overhead. Left a request to fix it here https://github.com/JetBrains/resharper-unity/issues/536
                When/if it is fixed, the overhead will disappear.
             */
            var doc = new XmlDocument();
            var packages = doc.CreateElement("Packages");
            doc.AppendChild(packages);
            foreach (var packageRelativePath in installedPackages)
            {
                var folder = doc.CreateElement("Folder");
                folder.SetAttribute("Path", Path.Combine(m_PluginsInstallDir, packageRelativePath));
                packages.AppendChild(folder);
            }

            var sb = new StringBuilder();
            var tw = new StringWriter(sb);
            doc.Save(tw);
            Environment.SetEnvironmentVariable("JET_ADDITIONAL_DEPLOYED_PACKAGES", sb.ToString());
        }

        private void RunInspectCode(List<string> installedPlugins)
        {
            SetupAdditionalDeployerPackagesEnvVar(installedPlugins);
            var inspectCode = Path.Combine(m_BaseDir, "JetBrains.ReSharper.CommandLineTools", "tools", "inspectcode.exe");
            var p = Process.Start(inspectCode, string.Join(" ", m_InspectCodeOptions));
            if (p == null)
            {
                throw new Exception("failed to start inspectcode.exe");
            }
            p.WaitForExit();
        }

        private static string GetTemporaryDirectory()
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }
    }
}
