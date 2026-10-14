using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace GoBox.content;

public class PropDef {
    [JsonPropertyName("id")] public string Id { get; init; }
    [JsonPropertyName("name")] public string Name { get; init; }
    [JsonPropertyName("description")] public string Description { get; init; }
    [JsonPropertyName("thumbnail_path")] public string ThumbnailPath { get; init; }
}

public class PropStaticDef : PropDef {
    [JsonPropertyName("model_path")] public string ModelPath { get; init; }
    [JsonPropertyName("coll_path")] public string CollPath { get; init; }

    public static PropStaticDef FromJson(string path) {
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);

        if (file == null) {
            GD.PrintErr($"Couldn't access '{path}'");
            return null;
        }

        var json = file.GetAsText();
        var def = JsonSerializer.Deserialize<PropStaticDef>(json);

        return def;
    }
}

public class PropStaticBrkDef : PropDef {
    [JsonPropertyName("model_path")] public string ModelPath { get; init; }
    [JsonPropertyName("coll_path")] public string CollPath { get; init; }

    [JsonPropertyName("model_destroy_path")]
    public string ModelDestroyPath { get; init; }

    [JsonPropertyName("coll_destroy_path")]
    public string CollDestroyPath { get; init; }

    public static PropStaticBrkDef FromJson(string path) {
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);

        if (file == null) {
            GD.PrintErr($"Couldn't access '{path}'");
            return null;
        }

        var json = file.GetAsText();
        var def = JsonSerializer.Deserialize<PropStaticBrkDef>(json);

        return def;
    }
}

public class PropPhysDef : PropDef {
    [JsonPropertyName("model_path")] public string ModelPath { get; init; }
    [JsonPropertyName("coll_path")] public string CollPath { get; init; }
    [JsonPropertyName("mass")] public float Mass { get; init; }

    public static PropPhysDef FromJson(string path) {
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);

        if (file == null) {
            GD.PrintErr($"Couldn't access '{path}'");
            return null;
        }

        var json = file.GetAsText();
        var def = JsonSerializer.Deserialize<PropPhysDef>(json);

        return def;
    }
}