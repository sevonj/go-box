using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace GoBox.types;

public class SaveStateHead {
    [JsonPropertyName("stage_id")] public string StageId { get; init; }

    public static string SaveDir() => "user://saves";

    public static SaveStateHead FromJson(string path) {
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);

        if (file == null) {
            GD.PrintErr($"Couldn't access '{path}'");
            return null;
        }

        var json = file.GetAsText();
        var def = JsonSerializer.Deserialize<SaveStateHead>(json, GoboxJson.JsonOptions);

        return def;
    }
}

public class SaveState : SaveStateHead {
    [JsonPropertyName("actor_ids")] public List<string> ActorIds { get; set; } = [];
    [JsonPropertyName("actor_states")] public List<string> ActorStates { get; set; } = [];


    public static SaveState FromJson(string path) {
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);

        if (file == null) {
            GD.PrintErr($"Couldn't access '{path}'");
            return null;
        }

        var json = file.GetAsText();
        var def = JsonSerializer.Deserialize<SaveState>(json, GoboxJson.JsonOptions);

        return def;
    }

    public string ToJson() => JsonSerializer.Serialize(this, GoboxJson.JsonOptions);
}