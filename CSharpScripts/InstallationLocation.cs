using System;
using System.IO;
using Godot;

public partial class InstallationLocation : Button
{
    const string _configFileName = "install_path.txt";
    public bool UseRelative = true;
    private string _selectedPath = "";
    private string _rawSelectedAbsolutePath = "";
    private ConfirmationDialog _relativePrompt;

    [Signal]
    public delegate void DirectorySelectedEventHandler(string path);

    public override void _Ready()
    {
        Pressed += OnPressed;

        string existingPath = LoadPathFromFile();
        if (!string.IsNullOrEmpty(existingPath))
        {
            _selectedPath = existingPath;
            InstallationLocationSingleton.Instance.InstallationLocation = _selectedPath;
        }
    }

    private void OnPressed()
    {
        var dialog = new FileDialog
        {
            FileMode = FileDialog.FileModeEnum.OpenDir,
            Access = FileDialog.AccessEnum.Filesystem,
            Title = "Select Installation Directory",
        };

        dialog.DirSelected += OnDirSelected;
        GetTree().Root.AddChild(dialog);
        dialog.PopupCentered();
    }

    private async void OnDirSelected(string absolutePath)
    {
        _rawSelectedAbsolutePath = absolutePath;

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        ShowRelativePrompt();
    }

    private void ShowRelativePrompt()
    {
        _relativePrompt = new ConfirmationDialog
        {
            Title = "Use Relative Path?",
            DialogText = "Do you want to save this path as relative to the app?",
        };

        _relativePrompt.GetOkButton().Text = "Yes";
        _relativePrompt.GetCancelButton().Text = "No";

        _relativePrompt.Confirmed += OnRelativeConfirmed;
        _relativePrompt.Canceled += OnRelativeCanceled;

        GetTree().Root.AddChild(_relativePrompt);
        _relativePrompt.PopupCentered();
    }

    private void OnRelativeConfirmed()
    {
        string basePath = GetAppBaseDir();
        try
        {
            Uri baseUri = new Uri(basePath + Path.DirectorySeparatorChar);
            Uri targetUri = new Uri(_rawSelectedAbsolutePath);

            _selectedPath = Uri.UnescapeDataString(baseUri.MakeRelativeUri(targetUri).ToString())
                .Replace('/', Path.DirectorySeparatorChar);
        }
        catch (Exception e)
        {
            GetTree()
                .CallGroup(
                    "notify",
                    "warning_notify",
                    $"Failed to convert to relative path, using absolute instead.\n {e.Message}"
                );
            _selectedPath = _rawSelectedAbsolutePath;
            UseRelative = false;
        }

        FinalizeSelection(true);
    }

    private void OnRelativeCanceled()
    {
        _selectedPath = _rawSelectedAbsolutePath;
        FinalizeSelection(false);
    }

    private void FinalizeSelection(bool relative)
    {
        UseRelative = relative;
        InstallationLocationSingleton.Instance.InstallationLocation = _selectedPath;
        SavePathToFile(_selectedPath);
        EmitSignal(SignalName.DirectorySelected, _selectedPath);
    }

    public string GetFinalPath()
    {
        if (UseRelative)
            return Path.GetFullPath(Path.Combine(GetAppBaseDir(), _selectedPath));
        return _selectedPath;
    }

    private string GetAppBaseDir()
    {
        return Path.GetDirectoryName(OS.GetExecutablePath());
    }

    private void SavePathToFile(string path)
    {
        string filePath = Path.Combine(GetAppBaseDir(), _configFileName);
        try
        {
            File.WriteAllText(filePath, path);
        }
        catch (Exception e)
        {
            GetTree()
                .CallGroup(
                    "notify",
                    "warning_notify",
                    $"Failed to save installation location!\n {e.Message}"
                );
        }
    }

    private string LoadPathFromFile()
    {
        string filePath = Path.Combine(GetAppBaseDir(), _configFileName);
        try
        {
            if (File.Exists(filePath))
            {
                return File.ReadAllText(filePath).Trim();
            }
        }
        catch (Exception e)
        {
            GD.Print($"Failed to load install path: {e.Message}\nFirst time launch? maybe :3");
        }

        return "";
    }
}
