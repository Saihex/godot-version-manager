// Copyright (c) 2025 Saihex Studios
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Godot;

public partial class InstallationLocationSingleton : Node
{
    public static InstallationLocationSingleton Instance { get; private set; }
    public string InstallationLocation { get; set; }

    public override void _Ready()
    {
        Instance = this;
    }
}
