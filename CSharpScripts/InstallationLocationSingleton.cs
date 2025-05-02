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
