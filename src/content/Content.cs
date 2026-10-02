using System.Collections.Generic;
using System.IO;
using Godot;

namespace GoBox.content;

public static class Content {
    public static readonly string ContentPath = "content"; //OS.GetExecutablePath().GetBaseDir().PathJoin("content");

    private static bool _contentLoaded;

    public static Dictionary<string, PhysProp> PhysProps { get; private set; } = new() {
        {
            "gobox.hogball", new PhysProp {
                Id = "gobox.hogball",
                Name = "Hogball",
                ResourcePath = "res://scenes/prop_hogball.tscn",
                ThumbnailPath = "res://assets/models/prop_hog/thumbnail.png"
            }
        }, {
            "gobox.tv", new PhysProp {
                Id = "gobox.tv",
                Name = "TV",
                ResourcePath = "res://scenes/prop_tv.tscn",
                ThumbnailPath = "res://assets/models/prop_tv/thumbnail.png"
            }
        },
    };

    public static Dictionary<string, Stage> Stages { get; private set; } = new() {
        {
            "gobox.bliss", new Stage {
                Id = "gobox.bliss",
                External = false,
                Name = "Bliss",
                Description = "A green, hilly zone",
                ResourcePath = "res://stages/stage_bliss.tscn",
                ThumbnailPath = "res://stages/gobox.bliss/preview.png"
            }
        }, {
            "gobox.city", new Stage {
                Id = "gobox.city",
                External = false,
                Name = "City",
                Description = "A city of white concrete",
                ResourcePath = "res://stages/stage_city.tscn",
                ThumbnailPath = "res://stages/gobox.city/preview.png"
            }
        },
    };

    public static string FullPath(string relative) => ContentPath.PathJoin(relative);

    public static void LoadContent() {
        if (_contentLoaded) return;
        _contentLoaded = true;

        var dirInfo = new DirectoryInfo(ContentPath);
        var dirs = dirInfo.EnumerateDirectories("*");

        foreach (var pakDirInfo in dirs) {
            GD.Print($"Found content dir '{pakDirInfo}'");
            LoadContentPak(pakDirInfo.ToString());
        }
    }

    private static void LoadContentPak(string pakPath) {
        var pakManifestPath = pakPath.PathJoin("manifest.json");
        if (!File.Exists(pakManifestPath)) {
            GD.Print($"No manifest in {pakPath}");
            return;
        }

        var pak = ContentPak.FromJson(pakManifestPath);
        GD.Print($"Pak contents:");
        if (pak.Stages != null) {
            foreach (var stageName in pak.Stages) {
                GD.Print($" - Stage '{stageName}'");
            }
        }

        if (pak.Stages != null) {
            foreach (var stageName in pak.Stages) {
                var stagePath = pakPath.PathJoin("stages").PathJoin(stageName).PathJoin("manifest.json");
                var stage = Stage.FromJson(stagePath);
                Stages.Add(stage.Id, stage);
            }
        }
    }
}