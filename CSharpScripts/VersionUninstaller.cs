// Copyright (c) 2025 Saihex Studios
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.IO; // For directory operations
using Godot;

public partial class VersionUninstaller : Button
{
    private ConfirmationDialog _relativePrompt;
    private string selectedLocation = "";
    private string selectedPath = "";

    public override void _Ready()
    {
        Pressed += OnPressed;
    }

    private void _on_confirmed()
    {
        if (Directory.Exists(selectedLocation + "/" + selectedPath))
        {
            try
            {
                Directory.Delete(selectedLocation + "/" + selectedPath, true);
                GetTree()
                    .CallGroup("notify", "info_notify", $"Successfully uninstalled {selectedPath}");
            }
            catch (Exception e)
            {
                GetTree()
                    .CallGroup(
                        "notify",
                        "warning_notify",
                        $"Failed to uninstall {selectedLocation}/{selectedPath}\n{e.Message}"
                    );
            }
        }
        else
        {
            GetTree()
                .CallGroup(
                    "notify",
                    "warning_notify",
                    $"Failed to uninstall {selectedPath}; Directory not found!"
                );
        }

        GetTree().CallGroup("reactive_elements", "force_refresh_without_fetch");
        QueueFree();
    }

    private void OnPressed()
    {
        selectedLocation = InstallationLocationSingleton.Instance.InstallationLocation;
        selectedPath = GetMeta("VersionPath").As<string>();

        _relativePrompt = new ConfirmationDialog
        {
            Title = $"Uninstall {selectedPath}",
            DialogText = "Are you sure you want to uninstall this installation?",
        };

        _relativePrompt.GetOkButton().Text = "Yes";
        _relativePrompt.GetCancelButton().Text = "Cancel";

        _relativePrompt.Confirmed += _on_confirmed;

        GetTree().Root.AddChild(_relativePrompt);
        _relativePrompt.PopupCentered();
    }
}
