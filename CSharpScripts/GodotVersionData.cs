using Godot;

public partial class GodotVersionData : GodotObject
{
    public string Name { get; set; }
    public string PublishedAt { get; set; }
    public bool Draft { get; set; }
    public bool Prerelease { get; set; }
    public string monoAsset { get; set; }
    public string normalAsset { get; set; }
}
