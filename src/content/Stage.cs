using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace GoBox.content;

public class PropSpawn {
    public string Res { get; init; }
    public Vector3 Pos { get; init; }
    public Vector3 Rot { get; init; }
}

public class Stage {
    public string Id { get; init; }
    public int Version { get; init; }

    [Obsolete("Internal content is only a temporary workaround")]
    public bool External { get; init; }

    public string Name { get; init; }
    public string Description { get; init; }
    public string ThumbnailPath { get; init; }

    [Obsolete("Internal content is only a temporary workaround")]
    public string ResourcePath { get; init; }

    public string ModelPath { get; init; }
    public string CollPath { get; init; }
    public Vector3 PlayerStartPosition { get; init; }
    public Vector3 PlayerStartRotation { get; init; }
    public List<PropSpawn> Props { get; init; }

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

        if (Props == null) {
            GD.PrintErr("Stage props is null");
            ok = false;
        }

        return ok;
    }

    public static Stage FromJson(string path) {
        GD.Print($"Opening stage manifest at '{path}'");
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);

        if (file == null) {
            GD.PrintErr($"Couldn't access '{path}'");
            return null;
        }

        var options = new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };
        options.Converters.Add(new Vector3JsonConv());

        var json = file.GetAsText();
        var stage = JsonSerializer.Deserialize<Stage>(json, options);

        if (!stage.Validate()) {
            GD.PrintErr($"stage failed validation: {path}");
            return null;
        }

        return stage;
    }
}

public class Vector3JsonConv : JsonConverter<Vector3> {
    public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.StartArray) {
            throw new JsonException("Invalid Vector3 format.");
        }

        reader.Read();
        var x = reader.GetSingle();
        reader.Read();
        var y = reader.GetSingle();
        reader.Read();
        var z = reader.GetSingle();
        reader.Read();

        if (reader.TokenType != JsonTokenType.EndArray) {
            throw new JsonException("Vector3 has too many elements");
        }

        return new Vector3(x, y, z);
    }

    public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options) {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.X);
        writer.WriteNumberValue(value.Y);
        writer.WriteNumberValue(value.Z);
        writer.WriteEndArray();
    }
}