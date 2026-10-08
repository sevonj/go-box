using Godot;

namespace GoBox;

[GlobalClass]
public partial class UiStageSelect : PanelContainer {
    [Export] private GridContainer _grid;
    [Export] private Label _previewNameLabel;
    [Export] private Label _previewIdLabel;
    [Export] private Label _previewDescLabel;
    [Export] private UiPictureFrame _previewPicture;
    [Export] private Button _loadButton;
    [Export] private CheckBox _collDebugCheckBox;

    private content.StageDef _selected;


    public override void _Ready() {
        _loadButton.Pressed += Commit;

        ReloadGrid();
        RefreshPreview();
    }

    private void ReloadGrid() {
        foreach (var child in _grid.GetChildren()) child.QueueFree();

        foreach (var (id, stage) in content.Content.Stages) {
            var button = UiPictureButton.Instantiate();
            button.Text = stage.Name;
            button.Pressed += () => SelectStage(id);
            button.Texture = LoadThumbnail(stage);
            _grid.AddChild(button);
        }
    }

    private void SelectStage(string id) {
        _selected = null;

        if (string.IsNullOrEmpty(id))
            return;

        if (!content.Content.Stages.TryGetValue(id, out var stage)) {
            GD.PushError($"Attempted to select nonexistent stage '{id}'");
            return;
        }

        _selected = stage;
        RefreshPreview();
    }


    private void RefreshPreview() {
        _loadButton.Disabled = _selected == null;
        if (_selected == null) {
            _previewNameLabel.Text = "Nothing selected.";
            _previewIdLabel.Text = "";
            _previewDescLabel.Text = "";
            return;
        }

        _previewNameLabel.Text = _selected.Name;
        _previewIdLabel.Text = _selected.Id;
        _previewDescLabel.Text = _selected.Description;
        _previewPicture.Texture = LoadThumbnail(_selected);
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

    private void Commit() {
        if (_selected == null) return;
        var debugColls = _collDebugCheckBox.ButtonPressed;
        Game.LoadStage(_selected, debugColls);
    }
}