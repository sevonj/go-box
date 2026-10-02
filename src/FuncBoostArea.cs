using GoBox.interfaces;
using Godot;

namespace GoBox;

[GlobalClass]
public partial class FuncBoostArea : Area3D {
    private const string MatPath = "res://assets/materials/mat_fx_trackbarrier.tres";

    [Export] public float Force = 1f;
    [Export] public bool LimitMaxSpeed;
    [Export] public float MaxSpeed = 50f;

    [Export] private CollisionShape3D _shape;
    private MeshInstance3D _meshInst = new();

    public override void _Ready() {
        AddChild(_meshInst);

        RebuildMesh();
    }

    public override void _PhysicsProcess(double delta) {
        foreach (var body in GetOverlappingBodies()) {
            if (body is not IPhysActor physActor) continue;

            if (physActor.GetLinearVelocity().Length() < MaxSpeed || !LimitMaxSpeed) {
                physActor.ApplyForce(GlobalBasis.Column2.Normalized() * Force * (float)delta);
            }
        }
    }

    private void RebuildMesh() {
        if (_shape.Shape is not BoxShape3D box) return;
        var size = box.Size / 2f;
        var len = size.Z;

        // Start cap
        var a = new Vector3(-size.X, -size.Y, -size.Z);
        var b = new Vector3(size.X, -size.Y, -size.Z);
        var c = new Vector3(size.X, size.Y, -size.Z);
        var d = new Vector3(-size.X, size.Y, -size.Z);

        // End cap
        var e = new Vector3(-size.X, -size.Y, size.Z);
        var f = new Vector3(size.X, -size.Y, size.Z);
        var g = new Vector3(size.X, size.Y, size.Z);
        var h = new Vector3(-size.X, size.Y, size.Z);

        // Cap center vertices
        var i = Vector3.Forward * size.Z;
        var j = -i;

        var mat = ResourceLoader.Load<Material>(MatPath);

        var sf = new SurfaceTool();

        sf.Begin(Mesh.PrimitiveType.Triangles);
        sf.SetMaterial(mat);

        // Start cap
        sf.SetUV(new(0f, 0.5f));
        sf.AddVertex(i);
        sf.SetUV(new(1f, 0f));
        sf.AddVertex(a);
        sf.SetUV(new(1f, 1f));
        sf.AddVertex(b);

        sf.SetUV(new(0f, 0.5f));
        sf.AddVertex(i);
        sf.SetUV(new(1f, 0f));
        sf.AddVertex(b);
        sf.SetUV(new(1f, 1f));
        sf.AddVertex(c);

        sf.SetUV(new(0f, 0.5f));
        sf.AddVertex(i);
        sf.SetUV(new(1f, 0f));
        sf.AddVertex(c);
        sf.SetUV(new(1f, 1f));
        sf.AddVertex(d);

        sf.SetUV(new(0f, 0.5f));
        sf.AddVertex(i);
        sf.SetUV(new(1f, 0f));
        sf.AddVertex(d);
        sf.SetUV(new(1f, 1f));
        sf.AddVertex(a);

        // End cap
        sf.SetUV(new(1f, 0.5f));
        sf.AddVertex(j);
        sf.SetUV(new(0f, 1f));
        sf.AddVertex(f);
        sf.SetUV(new(0f, 0f));
        sf.AddVertex(e);

        sf.SetUV(new(1f, 0.5f));
        sf.AddVertex(j);
        sf.SetUV(new(0f, 1f));
        sf.AddVertex(g);
        sf.SetUV(new(0f, 0f));
        sf.AddVertex(f);

        sf.SetUV(new(1f, 0.5f));
        sf.AddVertex(j);
        sf.SetUV(new(0f, 1f));
        sf.AddVertex(h);
        sf.SetUV(new(0f, 0f));
        sf.AddVertex(g);

        sf.SetUV(new(1f, 0.5f));
        sf.AddVertex(j);
        sf.SetUV(new(0f, 1f));
        sf.AddVertex(e);
        sf.SetUV(new(0f, 0f));
        sf.AddVertex(h);

        // Walls
        sf.SetUV(new(0f, 0f));
        sf.AddVertex(a);
        sf.SetUV(new(len, 0f));
        sf.AddVertex(e);
        sf.SetUV(new(len, 1f));
        sf.AddVertex(f);
        sf.SetUV(new(len, 1f));
        sf.AddVertex(f);
        sf.SetUV(new(0f, 1f));
        sf.AddVertex(b);
        sf.SetUV(new(0f, 0f));
        sf.AddVertex(a);

        sf.SetUV(new(0f, 0f));
        sf.AddVertex(b);
        sf.SetUV(new(len, 0f));
        sf.AddVertex(f);
        sf.SetUV(new(len, 1f));
        sf.AddVertex(g);
        sf.SetUV(new(len, 1f));
        sf.AddVertex(g);
        sf.SetUV(new(0f, 1f));
        sf.AddVertex(c);
        sf.SetUV(new(0f, 0f));
        sf.AddVertex(b);

        sf.SetUV(new(0f, 0f));
        sf.AddVertex(c);
        sf.SetUV(new(len, 0f));
        sf.AddVertex(g);
        sf.SetUV(new(len, 1f));
        sf.AddVertex(h);
        sf.SetUV(new(len, 1f));
        sf.AddVertex(h);
        sf.SetUV(new(0f, 1f));
        sf.AddVertex(d);
        sf.SetUV(new(0f, 0f));
        sf.AddVertex(c);

        sf.SetUV(new(0f, 0f));
        sf.AddVertex(d);
        sf.SetUV(new(len, 0f));
        sf.AddVertex(h);
        sf.SetUV(new(len, 1f));
        sf.AddVertex(e);
        sf.SetUV(new(len, 1f));
        sf.AddVertex(e);
        sf.SetUV(new(0f, 1f));
        sf.AddVertex(a);
        sf.SetUV(new(0f, 0f));
        sf.AddVertex(d);


        // sf.AddIndex(0);
        // sf.AddIndex(1);
        // sf.AddIndex(2);
        // sf.AddIndex(0);
        // sf.AddIndex(2);
        // sf.AddIndex(3);
        // sf.AddIndex(0);
        // sf.AddIndex(3);
        // sf.AddIndex(4);
        // sf.AddIndex(0);
        // sf.AddIndex(4);
        // sf.AddIndex(1);

        _meshInst.Mesh = sf.Commit();
    }
}