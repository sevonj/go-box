using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;

namespace GoBox.content;

public static class Content {
    public static readonly string ContentPath = "content"; //OS.GetExecutablePath().GetBaseDir().PathJoin("content");

    private static bool _contentLoaded;


    public static Dictionary<string, StageDef> Stages { get; private set; } = new() {
        {
            "gobox.base.bliss", new StageDef {
                Id = "gobox.bliss",
                External = false,
                Name = "Bliss",
                Description = "A green, hilly zone",
                ResourcePath = "res://stages/stage_bliss.tscn",
                ThumbnailPath = "res://stages/gobox.bliss/preview.png"
            }
        }, {
            "gobox.base.city", new StageDef {
                Id = "gobox.city",
                External = false,
                Name = "City",
                Description = "A city of white concrete",
                ResourcePath = "res://stages/stage_city.tscn",
                ThumbnailPath = "res://stages/gobox.city/preview.png"
            }
        }, {
            "gobox.base.downhill", new StageDef {
                Id = "gobox.base.downhill",
                External = false,
                Name = "Downhill",
                Description = "It's all downhill from here",
                ResourcePath = "res://stages/stage_downhill.tscn",
                ThumbnailPath = "res://content/gobox.base/stages/downhill/preview.png"
            }
        },
    };

    public static Dictionary<string, PropDef> PropDefs { get; private set; } = [];

    public static Dictionary<string, PhysProp> PropsPhys { get; private set; } = new() {
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

    public static Dictionary<string, ArrayMesh> Meshes { get; private set; } = [];
    public static Dictionary<string, ConcavePolygonShape3D> CollMeshes { get; private set; } = [];

    public static ArrayMesh GetMesh(string path) {
        Meshes.TryGetValue(path, out var cachced);
        if (cachced != null) {
            return cachced;
        }

        var scene = GoboxGltf.Load(FullPath(path));
        var mesh = (ArrayMesh)FindMeshInst(scene)?.Mesh;
        Meshes.Add(path, mesh);
        scene.Free();
        return mesh;

        MeshInstance3D FindMeshInst(Node node) {
            if (node is MeshInstance3D meshInst) return meshInst;
            return node.GetChildren().Select(FindMeshInst).FirstOrDefault(found => found != null);
        }
    }

    public static Shape3D GetCollMesh(string path) {
        CollMeshes.TryGetValue(path, out var cachced);
        if (cachced != null) {
            return cachced;
        }

        var shape = GoboxColl.Load(FullPath(path));
        CollMeshes.Add(path, shape);
        return shape;
    }

    public static string FullPath(string relative) => ContentPath.PathJoin(relative);

    public static Node3D InstantiateProp(string id) {
        PropDefs.TryGetValue(id, out var propDef);
        if (propDef == null) {
            GD.PrintErr($"Prop '{id}' doesn't exist");
            return null;
        }

        if (propDef is PropStaticDef propStaticDef)
            return PropStatic.FromDef(propStaticDef);
        if (propDef is PropStaticBrkDef brkDef)
            return PropStaticBrk.FromDef(brkDef);

        GD.PrintErr("UNREACHABLE");
        return null;
    }

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
        var pakManifestPath = pakPath.PathJoin("pak.json");
        if (!File.Exists(pakManifestPath)) {
            GD.Print($"No manifest in {pakPath}");
            return;
        }

        var pak = ContentPak.FromJson(pakManifestPath);
        GD.Print($"Pak contents:");
        GD.Print($" - {pak.Stages?.Count ?? 0} stage(s)");
        GD.Print($" - {pak.PropsStatic?.Count ?? 0} static prop(s)");
        GD.Print($" - {pak.PropsStaticBrk?.Count ?? 0} static brk prop(s)");

        if (pak.Stages != null) {
            foreach (var stageName in pak.Stages) {
                var path = pakPath.PathJoin("stages").PathJoin(stageName).PathJoin("stage.json");
                var def = StageDef.FromJson(path);
                Stages.Add(def.Id, def);
            }
        }

        if (pak.PropsStatic != null) {
            foreach (var id in pak.PropsStatic) {
                var path = pakPath.PathJoin("props").PathJoin($"{id}.json");
                var def = PropStaticDef.FromJson(path);
                PropDefs.Add(def.Id, def);
            }
        }

        if (pak.PropsStaticBrk != null) {
            foreach (var id in pak.PropsStaticBrk) {
                var path = pakPath.PathJoin("props").PathJoin($"{id}.json");
                var def = PropStaticBrkDef.FromJson(path);
                PropDefs.Add(def.Id, def);
            }
        }
    }
}