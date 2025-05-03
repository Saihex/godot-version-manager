// Copyright (c) 2025 Saihex Studios
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Threading.Tasks;
using Godot;

public partial class InstallationHandler : Node
{
    [Signal]
    public delegate void InstallationCompletedEventHandler();

    [Signal]
    public delegate void InstallationFailedEventHandler(string error_msg);
    public bool canCancel { get; private set; }
    private HttpRequest http;
    private string zipLocation;
    private bool cancelled = false;

    public void Cancel()
    {
        if (http == null || cancelled || !canCancel)
            return;

        canCancel = false;
        cancelled = true;
        http.CancelRequest();

        http.QueueFree();
        QueueFree();

        try
        {
            if (File.Exists(zipLocation))
                File.Delete(zipLocation);
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to clean up cancelled installation: {e.Message}");
        }
    }

    public async void beginDownload(GodotVersionData versionData, bool downloadMono)
    {
        canCancel = true;
        var installationPath = InstallationLocationSingleton.Instance.InstallationLocation;
        http = new HttpRequest();
        AddChild(http);

        var headers = new string[] { "User-Agent: GodotVersionManager" };

        var completion = new TaskCompletionSource();

        http.RequestCompleted += (result, responseCode, responseHeaders, body) =>
        {
            if (responseCode == 200 && !cancelled)
                completion.SetResult();
            else
                completion.SetException(
                    new Exception($"Request failed, Http code: {responseCode}")
                );

            http.QueueFree();
        };

        var zipURL = downloadMono ? versionData.monoAsset : versionData.normalAsset;
        var editor_name = zipURL.Split("/")[zipURL.Split("/").Length - 1];
        zipLocation = installationPath + "/" + editor_name;

        http.DownloadFile = zipLocation;
        http.Timeout = 30;
        http.UseThreads = true;
        Error err = http.Request(zipURL, headers);

        try
        {
            await completion.Task;

            if (err != Error.Ok)
                throw new Exception("File download failed!");

            canCancel = false;

            if (cancelled)
            {
                try
                {
                    if (File.Exists(zipLocation))
                        File.Delete(zipLocation);
                }
                catch (Exception e)
                {
                    GD.PrintErr($"Failed to clean up cancelled installation: {e.Message}");
                }

                return;
            }

            editor_name = editor_name.TrimSuffix(".zip");

            Directory.CreateDirectory(
                installationPath + "/" + versionData.Name + (downloadMono ? "_mono" : "")
            );

            await ZipExtractor.ExtractZipContents(
                zipLocation,
                installationPath + "/" + versionData.Name + (downloadMono ? "_mono" : "")
            );

            File.Delete(zipLocation);

            EmitSignal(nameof(InstallationCompleted));
        }
        catch (Exception ex)
        {
            try
            {
                if (File.Exists(zipLocation))
                    File.Delete(zipLocation);
            }
            catch (Exception e)
            {
                GD.PrintErr($"Failed to clean up failed installation: {e.Message}");
            }

            if (cancelled)
                return;

            EmitSignal(nameof(InstallationFailed), ex.Message);
        }
    }
}
