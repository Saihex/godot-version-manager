using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;

public partial class GodotVersionFetcher : Node
{
    [Signal]
    public delegate void VersionsFetchedEventHandler(Godot.Collections.Array versions);

    [Signal]
    public delegate void FetchFailedEventHandler(string msg);

    public async void Fetch()
    {
        HttpRequest http = new HttpRequest();
        AddChild(http);

        var headers = new string[] { "User-Agent: GodotVersionManager" };

        var completion = new TaskCompletionSource<byte[]>();

        http.RequestCompleted += (result, responseCode, responseHeaders, body) =>
        {
            if (responseCode == 200)
                completion.SetResult(body);
            else
                completion.SetException(new System.Exception("Request failed."));
        };

        http.Request("https://api.github.com/repos/godotengine/godot/releases", headers);

        try
        {
            byte[] body = await completion.Task;
            string json = System.Text.Encoding.UTF8.GetString(body);

            var parsed = JsonDocument.Parse(json);
            var list = new List<GodotVersionData>();

            foreach (var release in parsed.RootElement.EnumerateArray())
            {
                var assets = release.GetProperty("assets").EnumerateArray();

                var version = new GodotVersionData
                {
                    Name = release.GetProperty("tag_name").GetString(),
                    PublishedAt = release.GetProperty("published_at").GetString(),
                    Draft = release.GetProperty("draft").GetBoolean(),
                    Prerelease = release.GetProperty("prerelease").GetBoolean(),
                    monoAsset = FilterAssetsArray(assets, true),
                    normalAsset = FilterAssetsArray(assets, false),
                };

                list.Add(version);
            }

            list.Sort((a, b) => b.Name.CompareTo(a.Name));

            var final_list = new Godot.Collections.Array();
            foreach (var version in list)
            {
                final_list.Add(version);
            }

            EmitSignal(nameof(VersionsFetched), final_list);
        }
        catch (System.Exception ex)
        {
            // notification already managed on high level UI!
            EmitSignal(nameof(FetchFailed), ex.Message);
        }
    }

    public static string FilterAssetsArray(JsonElement.ArrayEnumerator array, bool getMono)
    {
        bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        bool is64bit = System.Environment.Is64BitOperatingSystem;

        foreach (var thisAssetData in array)
        {
            var name = thisAssetData.GetProperty("name").GetString();

            var isMono = name.Find("mono") != -1;
            if (getMono && !isMono)
                continue;
            if (!getMono && isMono)
                continue;

            if (
                isWindows && is64bit && name.Find("win64") != -1
                || (!is64bit && name.Find("win32") != -1)
            )
            {
                return thisAssetData.GetProperty("browser_download_url").GetString();
            }

            // Assume Linux, my beloved <3
            if (
                !isWindows && is64bit && name.Find("linux") != -1 && name.Find("x86_64") != -1
                || (!is64bit && name.Find("linux") != -1 && name.Find("x86_32") != -1)
            )
            {
                return thisAssetData.GetProperty("browser_download_url").GetString();
            }
        }

        return "";
    }
}
