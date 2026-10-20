using System;
using GoBox.gamemodes;
using GoBox.types;
using Godot;

namespace GoBox;

[GlobalClass]
public partial class UiStageSelect : PanelContainer {
    [Export] private TabBar _tabBar;
    [Export] private VBoxContainer _contentVbox;
    [Export] private Label _previewNameLabel;
    [Export] private Label _previewIdLabel;
    [Export] private Label _previewDescLabel;
    [Export] private UiPictureFrame _previewPicture;
    [Export] private Button _loadButton;
    [Export] private OptionButton _gamemodeSelect;
    [Export] private CheckBox _collDebugCheckBox;

    [Export] private Control _savesToolbar;
    [Export] private Button _saveButton;
    [Export] private Button _saveFileBrowseButton;

    private content.StageDef _selectedStage;
    private SaveStateHead _selectedSave;
    private String _selectedSaveName;

    private enum GamemodeEnum {
        Free,
        TimeTrial,
    }

    private enum Tabs {
        Stages = 0,
        Saves,
        Max,
    }

    private Tabs _currentTab = Tabs.Stages;


    // --- Node

    public override void _Ready() {
        for (var i = 0; i < (int)Tabs.Max; i++) {
            _tabBar.AddTab(((Tabs)i).ToString());
        }

        _tabBar.TabChanged += OnTabSelected;
        _loadButton.Pressed += Commit;
        _saveButton.Pressed += CreateSaveState;
        _saveFileBrowseButton.Pressed += () => { OS.ShellOpen(ProjectSettings.GlobalizePath(SaveStateHead.SaveDir())); };

        _gamemodeSelect.AddItem("Free Sandbox", (int)GamemodeEnum.Free);
        _gamemodeSelect.AddItem("Time Trial", (int)GamemodeEnum.TimeTrial);

        _saveButton.Disabled = Game.World == null;

        OnTabSelected(0);
    }

    // --- Private

    private void ReloadPage() {
        foreach (var child in _contentVbox.GetChildren()) child.QueueFree();

        // TODO: Maybe design better sections
        switch (_currentTab) {
            case Tabs.Stages: {
                var cont = new FlowContainer();
                _contentVbox.AddChild(cont);

                foreach (var (id, stage) in content.Content.StageDefs) {
                    var button = UiPictureButton.Instantiate();
                    button.Text = stage.Name;
                    button.Pressed += () => SelectStage(id);
                    button.Texture = LoadThumbnail(stage);
                    cont.AddChild(button);
                }

                break;
            }
            case Tabs.Saves: {
                var cont = new FlowContainer();
                _contentVbox.AddChild(cont);

                using var d = DirAccess.Open(SaveState.SaveDir());
                var files = d.GetFiles();
                if (files.IsEmpty()) {
                    cont.AddChild(new Label { Text = "No save states." });
                    break;
                }

                foreach (var filename in files) {
                    var button = UiPictureButton.Instantiate();
                    button.Text = filename;
                    button.Pressed += () => SelectSave(filename);
                    // button.Texture = LoadThumbnail(stage);
                    cont.AddChild(button);
                }

                break;
            }
        }
    }

    private void SelectStage(string id) {
        _selectedStage = null;

        if (string.IsNullOrEmpty(id))
            return;

        if (!content.Content.StageDefs.TryGetValue(id, out var stage)) {
            GD.PushError($"Attempted to select nonexistent stage '{id}'");
            return;
        }

        _selectedStage = stage;
        RefreshPreview();
    }

    private void SelectSave(string filename) {
        _selectedStage = null;
        _selectedSave = null;
        _selectedSaveName = null;
        var stub = SaveStateHead.FromJson(SaveStateHead.SaveDir().PathJoin(filename));
        if (stub == null) return;
        SelectStage(stub.StageId);
        if (_selectedStage == null) {
            GD.PushError("SaveState stage not found");
            return;
        }

        _selectedSave = stub;
        _selectedSaveName = filename;
        RefreshPreview();
    }


    private void RefreshPreview() {
        _loadButton.Disabled = _selectedStage == null;
        if (_selectedStage == null) {
            _previewNameLabel.Text = "Nothing selected.";
            _previewIdLabel.Text = "";
            _previewDescLabel.Text = "";
            return;
        }

        if (_selectedSave == null) {
            _previewNameLabel.Text = _selectedStage.Name;
            _previewIdLabel.Text = _selectedStage.Id;
            _previewDescLabel.Text = _selectedStage.Description;
            _previewPicture.Texture = LoadThumbnail(_selectedStage);
        }
        else {
            _previewNameLabel.Text = "(save)" + _selectedStage.Name;
            _previewIdLabel.Text = _selectedStage.Id;
            _previewDescLabel.Text = _selectedSaveName;
            _previewPicture.Texture = LoadThumbnail(_selectedStage);
        }
    }


    private Texture2D LoadThumbnail(content.StageDef stageDef) {
        if (stageDef?.ThumbnailPath == null) return null;

        if (!stageDef.External) {
            var tex = GD.Load<Texture2D>(stageDef.ThumbnailPath);
            if (!IsInstanceValid(tex)) GD.PrintErr(GetPath(), $" Couldn't load thumbnail for stage '{stageDef.Id}'");
            return tex;
        }

        var path = content.Content.ContentPath.PathJoin(stageDef.ThumbnailPath);
        var img = Image.LoadFromFile(path);
        return ImageTexture.CreateFromImage(img);
    }

    private void OnTabSelected(long index) {
        _currentTab = (Tabs)index;
        ReloadPage();
        RefreshPreview();
        _gamemodeSelect.Disabled = _currentTab == Tabs.Saves;
        _collDebugCheckBox.Disabled = _currentTab == Tabs.Saves;
        _savesToolbar.Visible = _currentTab == Tabs.Saves;
    }

    private void Commit() {
        if (_selectedSave != null) {
            var savestate = SaveState.FromJson(SaveStateHead.SaveDir().PathJoin(_selectedSaveName));
            Game.LoadSaveState(savestate);
            return;
        }

        if (_selectedStage == null) return;
        Gamemode gamemode;
        switch ((GamemodeEnum)_gamemodeSelect.Selected) {
            case GamemodeEnum.Free: {
                gamemode = new Gamemode();
                break;
            }
            case GamemodeEnum.TimeTrial: {
                gamemode = new GmTimeTrial();
                break;
            }
            default: throw new ArgumentOutOfRangeException();
        }

        GD.Print(gamemode);
        Game.LoadStage(_selectedStage, gamemode, debugColls: _collDebugCheckBox.ButtonPressed);
    }

    private void CreateSaveState() {
        var save = Game.CreateSaveState();
        if (save == null) {
            GD.PushError("save is null");
            return;
        }

        DirAccess.MakeDirAbsolute(SaveState.SaveDir());

        var filename = DateTime.UtcNow.ToString("yyyy_MM_dd_HH_mm_ss_ffff", System.Globalization.CultureInfo.InvariantCulture) + ".json";
        var filepath = SaveState.SaveDir().PathJoin(filename);
        FileAccess.Open(filepath, FileAccess.ModeFlags.Write).StoreString(save);

        ReloadPage();
    }
}