using System.Collections.Generic;
using System.IO;
using System.Linq;
using GoBox.types;
using Godot;

namespace GoBox.content;

public static class Content {
    public static readonly string ContentPath = "content"; //OS.GetExecutablePath().GetBaseDir().PathJoin("content");

    private static bool _contentLoaded;


    public static Dictionary<string, StageDef> StageDefs { get; private set; } = [];
    public static Dictionary<string, SpawnableDef> SpawnableDefs { get; private set; } = [];
    public static Dictionary<string, ArrayMesh> Meshes { get; private set; } = [];
    public static Dictionary<string, Shape3D> CollMeshes { get; private set; } = [];

    public static ArrayMesh GetMesh(string path) {
        Meshes.TryGetValue(path, out var cachced);
        if (cachced != null) {
            return cachced;
        }

        var scene = Gltf.Load(FullPath(path));
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

        var shape = Coll.Load(FullPath(path));
        CollMeshes.Add(path, shape);
        return shape;
    }

    public static string FullPath(string relative) => ContentPath.PathJoin(relative);

    public static Node3D InstantiateProp(string id) {
        SpawnableDefs.TryGetValue(id, out var def);

        switch (def) {
            case SpecialDef specialDef:
                return (Node3D)GD.Load<PackedScene>(specialDef.ResPath).Instantiate();
            case PropStaticDef sPropDef:
                return PropStatic.FromDef(sPropDef);
            case PropStaticBrkDef sBrkDef:
                return PropStaticBrk.FromDef(sBrkDef);
            case PropPhysDef pPropDef:
                return PropPhys.FromDef(pPropDef);
            case PropWheelDef wheelDef:
                return PropWheel.FromDef(wheelDef);
            case null:
                GD.PushError($"Prop '{id}' doesn't exist"); break;
            default:
                GD.PushError("UNREACHABLE"); break;
        }

        return null;
    }

    public static void LoadContent() {
        if (_contentLoaded) return;
        _contentLoaded = true;

        LoadContentPak(ContentPath.PathJoin("gobox.base"));

        SpawnableDefs.Add("gobox.base.func_boost_area", new SpecialDef {
            Id = "gobox.base.func_boost_area",
            Name = "Boost Area",
            Description = "Pushes physics objects in a direction",
            ThumbnailPath = "gobox.base/thumbnails/func_boost_area.png",
            ResPath = "res://scenes/func_boost_area.tscn"
        });
        SpawnableDefs.Add("gobox.base.func_finish_line", new SpecialDef {
            Id = "gobox.base.func_finish_line",
            Name = "Finish Line",
            Description = "Finish line with a trigger",
            ThumbnailPath = "gobox.base/thumbnails/func_finish_line.png",
            ResPath = "res://scenes/func_finish_line.tscn"
        });
        SpawnableDefs.Add("gobox.base.launcher", new SpecialDef {
            Id = "gobox.base.launcher",
            Name = "Launcher",
            Description = "Plastic lawnchair",
            ThumbnailPath = "gobox.base/thumbnails/launcher.png",
            ResPath = "res://scenes/launcher.tscn"
        });

        var dirInfo = new DirectoryInfo(ContentPath);
        var dirs = dirInfo.EnumerateDirectories("*");

        foreach (var pakDirInfo in dirs) {
            GD.Print($"Found content dir '{pakDirInfo}'");
            if (pakDirInfo.Name == "gobox.base") continue;
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
        GD.Print($" - {pak.PropsPhys?.Count ?? 0} phys prop(s)");
        GD.Print($" - {pak.Wheels?.Count ?? 0} wheel(s)");

        if (pak.Stages != null) {
            foreach (var stageName in pak.Stages) {
                var path = pakPath.PathJoin("stages").PathJoin(stageName).PathJoin("stage.json");
                var def = StageDef.FromJson(path);
                StageDefs.Add(def.Id, def);
            }
        }

        if (pak.PropsStatic != null) {
            foreach (var id in pak.PropsStatic) {
                var path = pakPath.PathJoin("props").PathJoin($"{id}.json");
                var def = PropStaticDef.FromJson(path);
                SpawnableDefs.Add(def.Id, def);
            }
        }

        if (pak.PropsStaticBrk != null) {
            foreach (var id in pak.PropsStaticBrk) {
                var path = pakPath.PathJoin("props").PathJoin($"{id}.json");
                var def = PropStaticBrkDef.FromJson(path);
                SpawnableDefs.Add(def.Id, def);
            }
        }

        if (pak.PropsPhys != null) {
            foreach (var id in pak.PropsPhys) {
                var path = pakPath.PathJoin("props").PathJoin($"{id}.json");
                var def = PropPhysDef.FromJson(path);
                SpawnableDefs.Add(def.Id, def);
            }
        }

        if (pak.Wheels != null) {
            foreach (var id in pak.Wheels) {
                var path = pakPath.PathJoin("props").PathJoin($"{id}.json");
                var def = PropWheelDef.FromJson(path);
                SpawnableDefs.Add(def.Id, def);
            }
        }
    }
}