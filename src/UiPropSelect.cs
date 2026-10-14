using System.Linq;
using Godot;

namespace GoBox;

[GlobalClass]
public partial class UiPropSelect : PanelContainer {
    [Export] private FlowContainer _grid;
    [Export] private Label _previewNameLabel;
    [Export] private UiPictureFrame _previewPic;


    public override void _Ready() {
        ReloadGrid();
        if (content.Content.PropDefs.Count > 0) {
            LocalVars.SelectPropDef(content.Content.PropDefs.Keys.First());
        }
        else {
            LocalVars.SelectPropDef(null);
        }

        RefreshPreview();
    }

    private void ReloadGrid() {
        foreach (var child in _grid.GetChildren()) child.QueueFree();

        foreach (var (id, def) in content.Content.PropDefs) {
            var button = UiPictureButton.Instantiate();
            button.Text = def.Name;
            button.Pressed += () => SelectPropDef(id);
            button.Texture = LoadPropThumbnail(def);
            _grid.AddChild(button);
        }
    }

    private void SelectPropDef(string id) {
        LocalVars.SelectPropDef(id);
        RefreshPreview();
    }


    private void RefreshPreview() {
        if (LocalVars.SelectedPropDef == null) {
            _previewNameLabel.Text = "Nothing selected.";
            return;
        }

        _previewNameLabel.Text = LocalVars.SelectedPropDef.Name;
        _previewPic.Texture = LoadPropThumbnail(LocalVars.SelectedPropDef);
    }


    private Texture2D LoadPropThumbnail(content.PropDef propDef) {
        if (propDef?.ThumbnailPath == null) return null;
        var tex = GD.Load<Texture2D>(propDef.ThumbnailPath);
        if (!IsInstanceValid(tex)) GD.PrintErr(GetPath(), $" Couldn't load thumbnail for prop id '{propDef.Id}'");
        return tex;
    }
}