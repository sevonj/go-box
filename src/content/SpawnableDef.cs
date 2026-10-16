using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace GoBox.content;

public class SpawnableDef {
    [JsonPropertyName("id")] public string Id { get; init; }
    [JsonPropertyName("name")] public string Name { get; init; }
    [JsonPropertyName("description")] public string Description { get; init; }
    [JsonPropertyName("thumbnail_path")] public string ThumbnailPath { get; init; }

    public virtual string GetSpawnTypeName() => "PLACEHOLDER";
}

public class SpecialDef : SpawnableDef {
    public string ResPath { get; init; }
    public override string GetSpawnTypeName() => "Special";
}

public class PropDef : SpawnableDef {
}

public class PropStaticDef : PropDef {
    [JsonPropertyName("model_path")] public string ModelPath { get; init; }
    [JsonPropertyName("coll_path")] public string CollPath { get; init; }

    public override string GetSpawnTypeName() => "Static Prop";

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

    public override string GetSpawnTypeName() => "Breakable Static Prop";

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

    public override string GetSpawnTypeName() => "Physics Prop";

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

public class PropWheelDef : PropDef {
    [JsonPropertyName("model_path")] public string ModelPath { get; init; }
    [JsonPropertyName("coll_path")] public string CollPath { get; init; }
    [JsonPropertyName("mass")] public float Mass { get; init; }
    [JsonPropertyName("offset")] public Vector3 Offset { get; init; }

    public override string GetSpawnTypeName() => "Prop Wheel";

    public static PropWheelDef FromJson(string path) {
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);

        if (file == null) {
            GD.PrintErr($"Couldn't access '{path}'");
            return null;
        }

        var options = new JsonSerializerOptions();
        options.Converters.Add(new Vector3JsonConv());
        
        var json = file.GetAsText();
        var def = JsonSerializer.Deserialize<PropWheelDef>(json, options);

        return def;
    }
}