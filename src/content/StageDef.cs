using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using GoBox.types;
using Godot;

namespace GoBox.content;

public class PropSpawn {
    [JsonPropertyName("id")] public string Id { get; init; }
    [JsonPropertyName("position")] public Vector3 Position { get; init; }
    [JsonPropertyName("rotation")] public Vector3 Rotation { get; init; }
}

public class StageDef {
    [JsonPropertyName("id")] public string Id { get; init; }
    [JsonPropertyName("version")] public int Version { get; init; }

    [Obsolete("Internal content is only a temporary workaround")]
    [JsonPropertyName("external")]
    public bool External { get; init; }

    [JsonPropertyName("name")] public string Name { get; init; }
    [JsonPropertyName("description")] public string Description { get; init; }
    [JsonPropertyName("thumbnail_path")] public string ThumbnailPath { get; init; }

    [Obsolete("Internal content is only a temporary workaround")]
    [JsonPropertyName("resource_path")]
    public string ResourcePath { get; init; }

    [JsonPropertyName("model_path")] public string ModelPath { get; init; }
    [JsonPropertyName("coll_path")] public string CollPath { get; init; }

    [JsonPropertyName("player_start_position")]
    public Vector3 PlayerStartPosition { get; init; }

    [JsonPropertyName("player_start_rotation")]
    public Vector3 PlayerStartRotation { get; init; }

    [JsonPropertyName("props_static")] public List<PropSpawn> PropsStatic { get; init; }


    public static StageDef FromJson(string path) {
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);

        if (file == null) {
            GD.PrintErr($"Couldn't access '{path}'");
            return null;
        }

        var json = file.GetAsText();
        var stage = JsonSerializer.Deserialize<StageDef>(json, GoboxJson.JsonOptions);

        if (!stage.Validate()) {
            GD.PrintErr($"stage failed validation: {path}");
            return null;
        }

        return stage;
    }

    public bool Validate() {
        var ok = true;
        if (string.IsNullOrEmpty(Id)) {
            GD.PrintErr("Stage id is null or empty");
            ok = false;
        }

        if (string.IsNullOrEmpty(Name)) {
            GD.PrintErr("Stage name is null or empty");
            ok = false;
        }

        if (string.IsNullOrEmpty(Description)) GD.PushWarning("Stage description is null or empty");
        if (string.IsNullOrEmpty(ThumbnailPath)) GD.PushWarning("Stage thumbnail path is null or empty");

        if (External) {
            if (string.IsNullOrEmpty(ModelPath)) {
                GD.PrintErr("Stage model path is null or empty");
                ok = false;
            }

            if (string.IsNullOrEmpty(CollPath)) {
                GD.PrintErr("Stage collision path is null or empty");
                ok = false;
            }
        }
        else if (string.IsNullOrEmpty(ResourcePath)) {
            GD.PrintErr("Stage res path is null or empty");
            ok = false;
        }

        if (PropsStatic == null) {
            GD.PrintErr("Stage props is null");
            ok = false;
        }

        return ok;
    }
}