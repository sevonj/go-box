using Godot;

namespace GoBox;

public static class LocalVars {
    public static bool DisablePlayerControl;
    public static content.StageDef CurrentStageDef;
    public static string CurrentGamemode;
    public static content.PropDef SelectedPropDef { get; private set; }

    public static void SelectPropDef(string id) {
        SelectedPropDef = null;

        if (string.IsNullOrEmpty(id))
            return;

        if (!content.Content.PropDefs.TryGetValue(id, out var def)) {
            GD.PushError($"Attempted to select nonexistent prop id '{id}'");
            return;
        }

        SelectedPropDef = def;
    }
}