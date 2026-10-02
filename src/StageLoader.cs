using System.Collections.Generic;
using GoBox.content;
using Godot;
using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;


namespace GoBox;

public static class StageLoader {
    public static World Load(Stage stage, bool debugColls = false) {
        if (!stage.External) {
            if (debugColls) GD.PrintErr("debugColls is not supported on internal stages.");

            var packed = ResourceLoader.Load<PackedScene>(stage.ResourcePath);
            return packed.Instantiate<World>();
        }


        var coll = LoadColl(stage);
        if (coll == null) return null;

        var model = debugColls ? GenerateCollDebugModel(coll) : LoadGltf(stage);
        if (model == null) return null;
        var modelStack = new Stack<Node>();
        modelStack.Push(model);
        while (modelStack.Count > 0) {
            var node = modelStack.Pop();
            var name = node.Name.ToString();
            if (name.EndsWith("-colonly")) {
                node.Free();
                continue;
            }

            foreach (var child in node.GetChildren()) modelStack.Push(child);
        }

        var envCont = new Node();
        envCont.Name = "Env";
        var staticCont = new Node();
        staticCont.Name = "Static";
        var actorCont = new Node();
        actorCont.Name = "Actors";

        var world = new World();
        world.AddChild(envCont);
        world.AddChild(staticCont);
        world.AddChild(actorCont);

        envCont.AddChild(ResourceLoader.Load<PackedScene>("res://assets/env/env_default.tscn").Instantiate());

        staticCont.AddChild(model);
        staticCont.AddChild(coll);

        var playerStart = new InfoPlayerStart();
        actorCont.AddChild(playerStart);
        playerStart.Position = stage.PlayerStartPosition;
        playerStart.Rotation = stage.PlayerStartRotation * Mathf.Pi / 180f;

        foreach (var spawn in stage.Props) {
            var prop = (PhysProp)ResourceLoader.Load<PackedScene>(spawn.Res).Instantiate();
            actorCont.AddChild(prop);
            prop.Position = spawn.Pos;
            prop.Rotation = spawn.Rot * Mathf.Pi / 180f;
        }

        return world;
    }

    private static Node LoadGltf(Stage stage) {
        if (stage.ModelPath == null) {
            GD.PrintErr($"Stage's ModelPath is null");
            return null;
        }

        var gltfDoc = new GltfDocument();
        var gltfSt = new GltfState();
        var path = Content.FullPath(stage.ModelPath);
        var result = gltfDoc.AppendFromFile(path, gltfSt);

        if (result != Error.Ok) {
            GD.PrintErr($"Failed to load GLTF from '{path}': code {result}");
            return null;
        }

        return gltfDoc.GenerateScene(gltfSt);
    }


    private static StaticBody3D LoadColl(Stage stage) {
        if (stage.CollPath == null) {
            GD.PrintErr($"Stage's CollPath is null");
            return null;
        }

        var path = Content.FullPath(stage.CollPath);

        const int headerSize = 16 + 4 + 4;
        const int minSize = headerSize + 3 * 3 * 4; // + size of tri
        byte[] collSignature = "GoBox Collision\n"u8.ToArray();
        const uint gameCollVersion = 2;

        var bytes = FileAccess.GetFileAsBytes(path);
        if (bytes.Length == 0) {
            GD.PushError($"Failed to access file '{path}'");
            return null;
        }

        if (bytes.Length < minSize) {
            GD.PushError($"'{path}' is too small to be a coll file");
            return null;
        }

        var data = new ReadOnlySpan<byte>(bytes);

        if (!data.Slice(0, 16).SequenceEqual(collSignature)) {
            GD.PushError($"'{path}' Coll file signature mismatch.");
            return null;
        }

        var version = BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(16, 4));
        if (version != gameCollVersion) {
            GD.PushError($"'{path}' Collision has unsupported version. file={version}, game={gameCollVersion}");
            return null;
        }

        var numTris = BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(20, 4));
        var expected = headerSize + numTris * 9 * sizeof(float);
        if (bytes.Length != expected) {
            GD.PushError(
                $"'{path}' Coll file length mismatch. Your file is probably bad. expected={expected}, got={bytes.Length}");
            return null;
        }

        var verts = MemoryMarshal.Cast<byte, Vector3>(data.Slice(headerSize)).ToArray();

        var shape = new ConcavePolygonShape3D { Data = verts };
        var body = new StaticBody3D();
        body.AddChild(new CollisionShape3D { Shape = shape });

        return body;
    }

    private static MeshInstance3D GenerateCollDebugModel(StaticBody3D body) {
        var st = new SurfaceTool();
        st.Begin(Mesh.PrimitiveType.Triangles);

        foreach (var child in body.GetChildren()) {
            if (child is not CollisionShape3D shape) continue;
            if (shape.Shape is not ConcavePolygonShape3D conk) {
                GD.PushWarning($"Collision debug model: skipping unsupported collider of type {shape.Shape.GetType()}");
                continue;
            }

            foreach (var v in conk.Data) {
                st.AddVertex(v);
            }
        }

        st.GenerateNormals();

        st.SetMaterial(ResourceLoader.Load<Material>("res://assets/materials/mat_debug_collision.tres"));

        return new MeshInstance3D { Mesh = st.Commit() };
    }
}