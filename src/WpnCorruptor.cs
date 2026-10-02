using System;
using Godot;

namespace GoBox;

[GlobalClass]
public partial class WpnCorruptor : Weapon {
    [Export] public uint Iterations = 100;
    [Export] public bool DontTouchPositionExponent = true;

    private RandomNumberGenerator _rng = new();

    public static WpnCorruptor Instantiate() =>
        (WpnCorruptor)GD.Load<PackedScene>("res://scenes/wpn_corruptor.tscn").Instantiate();

    private WpnCorruptor() {
    }

    public override void _Ready() {
        _rng.Randomize();
    }

    // --- Weapon

    public override string WpnName() => "Corruptor";

    public override string WpnDescription() => "Shoot props for controlled bit flips. Probably safe.\n" +
                                               "Restart the game to undo.";

    public override void Fire() {
        var cam = GetViewport().GetCamera3D();
        if (!IsInstanceValid(cam)) {
            GD.PrintErr($"{GetPath()}: Couldn't access camera.");
            return;
        }


        var spaceState = GetWorld3D().DirectSpaceState;
        var mousePos = GetViewport().GetMousePosition();

        var origin = cam.ProjectRayOrigin(mousePos);
        var end = origin + cam.ProjectRayNormal(mousePos) * 100f;
        var query = PhysicsRayQueryParameters3D.Create(origin, end);
        var result = spaceState.IntersectRay(query);

        if (result.Count == 0) return;
        if ((GodotObject)result["collider"] is not PhysProp prop) return;

        if (prop.Model?.Mesh is ArrayMesh mesh) {
            Corrupt(mesh);
        }
    }

    public override void FireAlt() {
    }

    // --- Private

    private unsafe void Corrupt(ArrayMesh mesh) {
        var arrays = mesh.SurfaceGetArrays(0);
        var verts = arrays[(int)Mesh.ArrayType.Vertex].AsVector3Array();
        if (verts.Length == 0) return;

        var primitive = mesh.SurfaceGetPrimitiveType(0);
        var material = mesh.SurfaceGetMaterial(0);

        var numFloats = verts.Length * 3;
        fixed (Vector3* p = verts) {
            var buffer = (int*)p;
            for (var i = 0; i < Iterations; i++) {
                var idx = (int)(_rng.Randi() % (uint)numFloats);
                buffer[idx] ^= 1 << (
                    DontTouchPositionExponent
                        ? (int)(_rng.Randi() % 24 - 1) % 32 // -1 => 31 for sign
                        : (int)(_rng.Randi() % 32)
                );
            }
        }

        arrays[(int)Mesh.ArrayType.Vertex] = verts;
        mesh.ClearSurfaces();
        mesh.AddSurfaceFromArrays(primitive, arrays);
        mesh.SurfaceSetMaterial(0, material);
    }
}