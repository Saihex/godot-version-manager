using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using Godot;

public static class ZipExtractor
{
    // Default permissions: owner read/write, others read-only
    private const UnixFileMode DefaultFileMode =
        UnixFileMode.UserRead
        | UnixFileMode.UserWrite
        | UnixFileMode.GroupRead
        | UnixFileMode.OtherRead
        | UnixFileMode.UserExecute;
    private const UnixFileMode DefaultDirectoryMode =
        DefaultFileMode
        | UnixFileMode.UserExecute
        | UnixFileMode.GroupExecute
        | UnixFileMode.OtherExecute;

    public static void ExtractZipContents(string zipPath, string extractTo)
    {
        using ZipArchive archive = ZipFile.OpenRead(zipPath);
        string commonRoot = GetRootDirectory(archive);

        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            // Strip the root directory if present
            string relativePath = entry.FullName;

            if (!string.IsNullOrEmpty(commonRoot) && relativePath.StartsWith(commonRoot))
                relativePath = relativePath.Substring(commonRoot.Length);

            if (string.IsNullOrEmpty(relativePath))
                continue; // Skip root folder

            string fullPath = Path.Combine(extractTo, relativePath);

            if (entry.FullName.EndsWith("/"))
            {
                Directory.CreateDirectory(fullPath);
                SetPermissions(fullPath, isDirectory: true);
            }
            else
            {
                string directory = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    SetPermissions(directory, isDirectory: true);
                }

                using (var entryStream = entry.Open())
                using (var fileStream = File.Create(fullPath))
                {
                    entryStream.CopyTo(fileStream);
                }
                SetPermissions(fullPath, isDirectory: false);
            }
        }
    }

    private static void SetPermissions(string path, bool isDirectory)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return;

        try
        {
            var mode = isDirectory ? DefaultDirectoryMode : DefaultFileMode;
            File.SetUnixFileMode(path, mode);
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to set permissions for {path}: {e.Message}");
        }
    }

    private static string GetRootDirectory(ZipArchive archive)
    {
        foreach (var entry in archive.Entries)
        {
            if (entry.FullName.Contains("/"))
                return entry.FullName.Substring(0, entry.FullName.IndexOf("/") + 1);
        }
        return "";
    }
}
