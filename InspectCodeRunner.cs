using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
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

        public InspectCodeRunner(string[] args)
        {
            var options = new OptionSet {
                { "base-dir=", "Consist of two folders: 1. JetBrains.ReSharper.CommandLineTools with tools executables. 'packages' contains code analysis packages", v => m_BaseDir = v },
                { "o=|output=", "Files contains inspectcode's output. Default: $base-dir\\result.xml", v => m_OutPutFilePath = v }
            };
            m_InspectCodeOptions = options.Parse(args);
            m_InspectCodeOptions.Add($"--caches-home={GetTemporaryDirectory()}");
            if (string.IsNullOrEmpty(m_OutPutFilePath))
            {
                m_OutPutFilePath = Path.Combine(m_BaseDir, "result.xml");
            }
            m_InspectCodeOptions.Add($"--output={m_OutPutFilePath}");
            m_PluginsInstallDir = Path.Combine(m_BaseDir, "Plugins");
        }

        public void Run()
        {   
            InstallReSharperCommandLineTools();
            var codeAnalysisPlugins = InstallCodeAnalysisPlugins();
            RunInspectCode(codeAnalysisPlugins);
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
            var installer = new NuGetPackagesInstaller(sources, m_PluginsInstallDir);
            var packages = new[]
            {
                new PackageInfo { Id = "ReSharper.FooDetector", Version = k_SdkVersion},
                new PackageInfo { Id = "PowerToys.CyclomaticComplexity", Version = k_SdkVersion},
            };
            return installer.InstallPackages(packages);
        }


        private void SetupAdditionalDeployerPackagesEnvVar(List<string> installedPackages)
        {
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
