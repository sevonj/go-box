using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace GoBox.content;

public class ContentPak {
    [JsonPropertyName("stages")] public List<string> Stages { get; init; }
    [JsonPropertyName("props_static")] public List<string> PropsStatic { get; init; }
    [JsonPropertyName("props_static_brk")] public List<string> PropsStaticBrk { get; init; }
    [JsonPropertyName("props_phys")] public List<string> PropsPhys { get; init; }

    public static ContentPak FromJson(string path) {
        GD.Print($"Loading ContentPak '{path}'");
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);

        if (file == null) {
            GD.PrintErr($"Couldn't access '{path}'");
            return null;
        }

        var json = file.GetAsText();
        var pak = JsonSerializer.Deserialize<ContentPak>(json);

        return pak;
    }
}