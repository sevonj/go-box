using System.Collections.Generic;
using GoBox.content;
using Godot;


namespace GoBox;

public static class StageLoader {
    public static World Load(StageDef stageDef, bool debugColls = false) {
        if (!stageDef.External) {
            if (debugColls) GD.PrintErr("debugColls is not supported on internal stages.");

            var packed = ResourceLoader.Load<PackedScene>(stageDef.ResourcePath);
            return packed.Instantiate<World>();
        }

        if (stageDef.CollPath == null) {
            GD.PrintErr($"Stage's CollPath is null");
            return null;
        }

        if (stageDef.ModelPath == null) {
            GD.PrintErr($"Stage's ModelPath is null");
            return null;
        }

        var collShape = GoboxColl.Load(Content.FullPath(stageDef.CollPath));
        if (collShape == null) return null;
        var coll = new StaticBody3D();
        coll.AddChild(new CollisionShape3D { Shape = collShape });

        var model = debugColls
            ? GenerateCollDebugModel(collShape)
            : GoboxGltf.Load(Content.FullPath(stageDef.ModelPath));
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
        playerStart.Position = stageDef.PlayerStartPosition;
        playerStart.Rotation = stageDef.PlayerStartRotation * Mathf.Pi / 180f;

        foreach (var propSpawn in stageDef.PropsStatic) {
            var propInst = Content.InstantiateProp(propSpawn.Id);
            if (propInst == null) continue;
            actorCont.AddChild(propInst);
            propInst.Position = propSpawn.Position;
            propInst.Rotation = propSpawn.Rotation * Mathf.Pi / 180f;
        }

        return world;
    }


    private static MeshInstance3D GenerateCollDebugModel(Shape3D shape) {
        if (shape is not ConcavePolygonShape3D conk) return null;

        var st = new SurfaceTool();
        st.Begin(Mesh.PrimitiveType.Triangles);

        foreach (var v in conk.Data) {
            st.AddVertex(v);
        }

        st.GenerateNormals();

        st.SetMaterial(ResourceLoader.Load<Material>("res://assets/materials/mat_debug_collision.tres"));

        return new MeshInstance3D { Mesh = st.Commit() };
    }
}