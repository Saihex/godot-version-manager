using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Godot;

public partial class GodotLauncher : Node
{
    public GodotInstallationData[] getInstallationList()
    {
        var installationPath = InstallationLocationSingleton.Instance.InstallationLocation;

        DirAccess dir = DirAccess.Open(installationPath);

        if (dir == null)
        {
            return null;
        }

        List<GodotInstallationData> godotInstallationDatas = new List<GodotInstallationData>();

        foreach (var installationName in dir.GetDirectories())
        {
            DirAccess installedDirectory = DirAccess.Open(
                installationPath + "/" + installationName
            );

            if (installedDirectory == null)
                continue;

            foreach (var executable in installedDirectory.GetFiles())
            {
                if (
                    (
                        executable.EndsWith(".exe")
                        || executable.EndsWith(".x86_64")
                        || executable.EndsWith(".x86_32")
                    )
                    && executable.Find("console", 0, false) == -1
                )
                {
                    var isMono = installationName.EndsWith("_mono");

                    var _data = new GodotInstallationData()
                    {
                        Executable = installationPath + "/" + installationName + "/" + executable,
                        Name =
                            installationName.TrimSuffix("_mono") + (isMono ? " (Mono)" : " (Zulu)"),
                    };

                    godotInstallationDatas.Add(_data);
                    break;
                }
            }
        }

        godotInstallationDatas.Sort((a, b) => b.Name.CompareTo(a.Name));

        for (int i = 0; i < godotInstallationDatas.Count; i++)
        {
            godotInstallationDatas[i].Name = godotInstallationDatas[i].Name.TrimSuffix(" (Zulu)");
        }

        return [.. godotInstallationDatas];
    }

    public void launchProcess(string pathToExecutable, string arguments = "")
    {
        try
        {
            string fullPath = ProjectSettings.GlobalizePath(pathToExecutable);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                fullPath = fullPath.Replace("/", "\\"); // I hate you windows.

            if (!File.Exists(fullPath))
                throw new Exception("Installation doesn't exists!");

            _ = Process.Start(
                new ProcessStartInfo
                {
                    FileName = fullPath,
                    Arguments = arguments,
                    UseShellExecute = true,
                    CreateNoWindow = false,
                }
            );

            fullPath = fullPath.Replace("\\", "/"); // fml
            var filename = fullPath.Split("/")[fullPath.Split("/").Length - 2];
            var isMono = filename.EndsWith("_mono") ? " (Mono)" : "";

            GetTree()
                .CallGroup(
                    "notify",
                    "info_notify",
                    $"{filename.TrimSuffix("_mono")}{isMono} launched successfully!"
                );
        }
        catch (Exception e)
        {
            GetTree()
                .CallGroup(
                    "notify",
                    "warning_notify",
                    $"Failed launching installation!\n {e.Message}"
                );
        }
    }
}

public partial class GodotInstallationData : GodotObject
{
    public string Name { get; set; }
    public string Executable { get; set; }
}
