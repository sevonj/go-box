using Godot;

namespace GoBox;

public static class LocalVars {
    public static bool DisablePlayerControl;
    public static content.StageDef CurrentStageDef;
    public static string CurrentGamemode;
    public static content.SpawnableDef SelectedSpawnableDef { get; private set; }

    public static void SelectSpawnable(string id) {
        SelectedSpawnableDef = null;

        if (string.IsNullOrEmpty(id))
            return;

        if (!content.Content.SpawnableDefs.TryGetValue(id, out var def)) {
            GD.PushError($"Attempted to select nonexistent prop id '{id}'");
            return;
        }

        SelectedSpawnableDef = def;
    }
}