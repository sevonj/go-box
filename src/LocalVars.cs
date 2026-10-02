using Godot;

namespace GoBox;

public static class LocalVars {
    public static content.PhysProp SelectedPhysProp { get; private set; }
    public static PackedScene SelectedPhysPropPacked { get; private set; }

    public static void SelectPhysProp(string id) {
        SelectedPhysProp = null;
        SelectedPhysPropPacked = null;

        if (string.IsNullOrEmpty(id))
            return;


        if (!content.Content.PhysProps.TryGetValue(id, out var prop)) {
            GD.PushError($"Attempted to select nonexistent prop id '{id}'");
            return;
        }

        var packed = GD.Load<PackedScene>(prop.ResourcePath);

        if (packed == null) {
            GD.PushError($"Failed to load resource for prop id '{id}'. Respath:  '{prop.ResourcePath}'");
            return;
        }

        SelectedPhysProp = prop;
        SelectedPhysPropPacked = packed;
    }
}