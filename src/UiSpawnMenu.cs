using System.Collections.Generic;
using System.Linq;
using GoBox.content;
using Godot;

namespace GoBox;

[GlobalClass]
public partial class UiSpawnMenu : PanelContainer {
    [Export] private TabBar _tabBar;
    [Export] private VBoxContainer _contentVbox;
    [Export] private Label _previewNameLabel;
    [Export] private Label _previewTypeLabel;
    [Export] private Label _previewIdLabel;
    [Export] private Label _previewDescLabel;
    [Export] private UiPictureFrame _previewPic;

    private enum Tabs {
        Props = 0,
        Special,
        Max,
    }

    private Tabs _currentTab = Tabs.Props;

    // --- Node

    public override void _Ready() {
        for (var i = 0; i < (int)Tabs.Max; i++) {
            _tabBar.AddTab(((Tabs)i).ToString());
        }

        _tabBar.TabChanged += OnTabSelected;

        ReloadPage();
        LocalVars.SelectSpawnable(Content.SpawnableDefs.Count > 0 ? Content.SpawnableDefs.Keys.First() : null);
        RefreshPreview();
    }

    // --- Private

    private void ReloadPage() {
        foreach (var child in _contentVbox.GetChildren()) child.QueueFree();
        
        // TODO: Maybe design better sections
        switch (_currentTab) {
            case Tabs.Props: {
                AddSection("Physics Props", Content.SpawnableDefs.Where(p => p.Value is PropPhysDef));
                AddSection("Breakables", Content.SpawnableDefs.Where(p => p.Value is PropStaticBrkDef));
                AddSection("Static Models", Content.SpawnableDefs.Where(p => p.Value is PropStaticDef));
                break;
            }
            case Tabs.Special: {
                AddSection("Special", Content.SpawnableDefs.Where(p => p.Value is SpecialDef));
                break;
            }
        }
    }

    private void AddSection(string title, IEnumerable<KeyValuePair<string, SpawnableDef>> propDefs) {
        var filtered = propDefs.ToList();
        if (filtered.Count == 0) return;

        var separator = UiContentSourceSeparator.Instantiate();
        separator.Label.Text = title;
        _contentVbox.AddChild(separator);

        var cont = new FlowContainer();
        _contentVbox.AddChild(cont);

        foreach (var (id, def) in filtered) {
            var button = UiPictureButton.Instantiate();
            button.Text = def.Name;
            button.Pressed += () => SelectSpawnable(id);
            button.Texture = LoadThumbnail(def);
            cont.AddChild(button);
        }
    }


    private void SelectSpawnable(string id) {
        LocalVars.SelectSpawnable(id);
        RefreshPreview();
    }


    private void RefreshPreview() {
        if (LocalVars.SelectedSpawnableDef == null) {
            _previewNameLabel.Text = "Nothing selected.";
            _previewTypeLabel.Text = "";
            _previewIdLabel.Text = "";
            _previewDescLabel.Text = "";
            return;
        }

        _previewNameLabel.Text = LocalVars.SelectedSpawnableDef.Name;
        _previewIdLabel.Text = LocalVars.SelectedSpawnableDef.Id;
        _previewTypeLabel.Text = LocalVars.SelectedSpawnableDef.GetSpawnTypeName();
        _previewDescLabel.Text = LocalVars.SelectedSpawnableDef.Description;
        _previewPic.Texture = LoadThumbnail(LocalVars.SelectedSpawnableDef);
    }


    private Texture2D LoadThumbnail(SpawnableDef spawnableDef) {
        if (string.IsNullOrEmpty(spawnableDef?.ThumbnailPath)) return null;
        var tex = GD.Load<Texture2D>(spawnableDef.ThumbnailPath);
        if (!IsInstanceValid(tex)) GD.PrintErr(GetPath(), $" Couldn't load thumbnail for prop id '{spawnableDef.Id}'");
        return tex;
    }

    private void OnTabSelected(long index) {
        _currentTab = (Tabs)index;
        ReloadPage();
        RefreshPreview();
    }
}