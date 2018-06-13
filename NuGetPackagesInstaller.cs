﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RCLTStarter
{
    class NuGetPackagesInstaller
    {
        readonly IEnumerable<string> m_Sources;
        readonly string m_InstallDir;

        public NuGetPackagesInstaller(IEnumerable<string> sources, string installDir)
        {
            m_Sources = sources;
            m_InstallDir = installDir;
        }

        public List<string> InstallPackages(IEnumerable<PackageInfo> packages)
        {
            return packages.Select(InstallPackage).ToList();
        }

        private string InstallPackage(PackageInfo packageInfo)
        {
            var args = new List<string>();
            args.Add("install");
            args.AddRange(m_Sources.Select(source => $"-source {source}"));
            args.Add($"{packageInfo.Id} -version {packageInfo.Version}");
            args.Add($"-OutputDirectory \"{m_InstallDir}\"");
            args.Add("-ExcludeVersion");
            Process.Start("nuget.exe", string.Join(" ", args))?.WaitForExit();
            return packageInfo.Id;
        }
    }
}
