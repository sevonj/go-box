using System.Collections.Generic;
using System.Text.Json;
using Godot;

namespace GoBox.content;

public class ContentPak {
    public List<string> Stages { get; init; }

    public static ContentPak FromJson(string path) {
        GD.Print($"Loading ContentPak '{path}'");
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

        var json = file.GetAsText();
        var pak = JsonSerializer.Deserialize<ContentPak>(json, options);

        return pak;
    }
}