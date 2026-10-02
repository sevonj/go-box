using System.Linq;
using Godot;

namespace GoBox;

[GlobalClass]
public partial class UiPropSelect : PanelContainer {
    [Export] private GridContainer _grid;
    [Export] private Label _previewNameLabel;
    [Export] private UiPictureFrame _previewPic;


    public override void _Ready() {
        ReloadGrid();
        LocalVars.SelectPhysProp(content.Content.PhysProps.Keys.First());
        RefreshPreview();
    }

    private void ReloadGrid() {
        foreach (var child in _grid.GetChildren()) child.QueueFree();

        foreach (var (id, prop) in content.Content.PhysProps) {
            var button = UiPictureButton.Instantiate();
            button.Text = prop.Name;
            button.Pressed += () => SelectPhysProp(id);
            button.Texture = LoadPropThumbnail(prop);
            _grid.AddChild(button);
        }
    }

    private void SelectPhysProp(string id) {
        LocalVars.SelectPhysProp(id);
        RefreshPreview();
    }


    private void RefreshPreview() {
        if (LocalVars.SelectedPhysProp == null) {
            _previewNameLabel.Text = "Nothing selected.";
            return;
        }

        _previewNameLabel.Text = LocalVars.SelectedPhysProp.Name;
        _previewPic.Texture = LoadPropThumbnail(LocalVars.SelectedPhysProp);
    }


    private Texture2D LoadPropThumbnail(content.PhysProp prop) {
        if (prop?.ThumbnailPath == null) return null;
        var tex = GD.Load<Texture2D>(prop.ThumbnailPath);
        if (!IsInstanceValid(tex)) GD.PrintErr(GetPath(), $" Couldn't load thumbnail for prop id '{prop.Id}'");
        return tex;
    }
}