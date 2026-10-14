using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;

namespace GoBox.content;

public static class Content {
    public static readonly string ContentPath = "content"; //OS.GetExecutablePath().GetBaseDir().PathJoin("content");

    private static bool _contentLoaded;


    public static Dictionary<string, StageDef> StageDefs { get; private set; } = [];
    public static Dictionary<string, PropDef> PropDefs { get; private set; } = [];

    public static Dictionary<string, ArrayMesh> Meshes { get; private set; } = [];
    public static Dictionary<string, Shape3D> CollMeshes { get; private set; } = [];

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

        if (propDef is PropStaticDef sPropDef)
            return PropStatic.FromDef(sPropDef);
        if (propDef is PropStaticBrkDef sBrkDef)
            return PropStaticBrk.FromDef(sBrkDef);
        if (propDef is PropPhysDef pPropDef)
            return PropPhys.FromDef(pPropDef);

        GD.PrintErr("UNREACHABLE");
        return null;
    }

    public static void LoadContent() {
        if (_contentLoaded) return;
        _contentLoaded = true;

        LoadContentPak(ContentPath.PathJoin("gobox.base"));

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

        if (pak.PropsPhys != null) {
            foreach (var id in pak.PropsPhys) {
                var path = pakPath.PathJoin("props").PathJoin($"{id}.json");
                var def = PropPhysDef.FromJson(path);
                PropDefs.Add(def.Id, def);
            }
        }
    }
}