using System;
using GoBox.interfaces;
using Godot;

namespace GoBox;

[GlobalClass]
public partial class WpnCorruptor : Weapon {
    [Export] public uint Intensity = 10;
    [Export] public bool DontTouchPositionExponent = true;

    private RandomNumberGenerator _rng = new();
    private const string BeamMatPath = "res://assets/materials/mat_fx_corruptobeam.tres";
    [Export] private Marker3D _beamStartMarker;


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
        if ((GodotObject)result["collider"] is not IProp prop) return;
        var mesh = prop.GetMesh();
        if (mesh == null) return;
        CreateBeamFx((Vector3)result["position"]);
        Corrupt(mesh);
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
        var iterations = numFloats * Intensity / 100;
        fixed (Vector3* p = verts) {
            var buffer = (int*)p;
            for (var i = 0; i < iterations; i++) {
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

    private void CreateBeamFx(Vector3 target) {
        var start = _beamStartMarker.GlobalPosition;

        var mesh = new ImmediateMesh();
        const float beamWidth = 0.2f;
        var len = start.DistanceTo(target);
        mesh.SurfaceBegin(Mesh.PrimitiveType.TriangleStrip);
        mesh.SurfaceSetUV(new Vector2(0f, 0f));
        mesh.SurfaceAddVertex(new Vector3(0f, beamWidth / 2f, 0f));
        mesh.SurfaceSetUV(new Vector2(1f, 0f));
        mesh.SurfaceAddVertex(new Vector3(0f, beamWidth / 2f, -len));
        mesh.SurfaceSetUV(new Vector2(0f, 1f));
        mesh.SurfaceAddVertex(new Vector3(0f, -beamWidth / 2f, 0f));
        mesh.SurfaceSetUV(new Vector2(1f, 1f));
        mesh.SurfaceAddVertex(new Vector3(0f, -beamWidth / 2f, -len));
        mesh.SurfaceEnd();
        mesh.SurfaceSetMaterial(0, ResourceLoader.Load<Material>(BeamMatPath));

        var beam = new MeshInstance3D();
        beam.SetMesh(mesh);
        beam.Name = "FxCorruptoBeam";
        Game.World.AddActor(beam);
        beam.GlobalPosition = start;
        beam.LookAt(target);
        GetTree().CreateTimer(0.3).Timeout += beam.QueueFree;
    }
}