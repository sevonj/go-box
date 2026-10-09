using GoBox.interfaces;
using Godot;

namespace GoBox;

[GlobalClass]
public partial class WpnCorruptor : Weapon {
    [Export] public uint IntensityContinuous = 10;
    [Export] public uint IntensityBlast = 100;
    [Export] public bool DontTouchPositionExponent = true;

    private const string BeamMatPath = "res://assets/materials/mat_fx_corruptobeam.tres";
    private const float BeamWidthContinuous = 0.2f;
    private const float BeamWidthBlast = 0.5f;

    [Export] private Marker3D _beamStartMarker;
    [Export] private AudioStream _sfxNoUse;
    [Export] private AudioStream _sfxContinuous;
    [Export] private AudioStream _sfxBlast;
    private RandomNumberGenerator _rng = new();
    private AudioStreamPlayer _audioStreamPlayer = new();

    private bool _isFiringContinuously;
    private double _continuousFireTimer;
    private bool _continuousNoUsePlayed;


    public static WpnCorruptor Instantiate() =>
        (WpnCorruptor)GD.Load<PackedScene>("res://scenes/wpn_corruptor.tscn").Instantiate();

    private WpnCorruptor() {
    }

    // --- Node

    public override void _Ready() {
        _rng.Randomize();

        AddChild(_audioStreamPlayer);
        _audioStreamPlayer.Name = "AudioStreamPlayer";
    }

    public override void _Process(double delta) {
        _isFiringContinuously &= Input.IsActionPressed("wpn_fire");
        _continuousFireTimer -= delta;
        _continuousFireTimer = Mathf.Max(_continuousFireTimer, 0.0);

        if (_isFiringContinuously && _continuousFireTimer <= 0.0) {
            _continuousFireTimer += 0.3;

            var cam = GetViewport().GetCamera3D();
            if (!IsInstanceValid(cam)) {
                GD.PrintErr($"{GetPath()}: Couldn't access camera.");
                _isFiringContinuously = false;
                return;
            }

            var spaceState = GetWorld3D().DirectSpaceState;
            var mousePos = GetViewport().GetMousePosition();

            var origin = cam.ProjectRayOrigin(mousePos);
            var end = origin + cam.ProjectRayNormal(mousePos) * 100f;
            var query = PhysicsRayQueryParameters3D.Create(origin, end);
            var result = spaceState.IntersectRay(query);

            if (result.Count == 0 || (GodotObject)result["collider"] is not IProp prop) {
                if (!_continuousNoUsePlayed) PlaySfx(_sfxNoUse);
                _continuousNoUsePlayed = true;
                return;
            }

            var mesh = prop.GetMesh();
            if (mesh == null) {
                _isFiringContinuously = false;
                return;
            }

            CreateBeamFx((Vector3)result["position"], BeamWidthContinuous);
            PlaySfx(_sfxContinuous);
            Corrupt(mesh, IntensityContinuous);
            _continuousNoUsePlayed = true;
        }
    }

    // --- Weapon

    public override string WpnName() => "Corruptor";

    public override string WpnDescription() => "Shoot props for controlled bit flips. Probably safe.\n" +
                                               "Restart the game to undo.\n" +
                                               "Fire: Continuous Alt fire: Blast";

    public override void Fire() {
        _isFiringContinuously = true;
        _continuousNoUsePlayed = false;
    }

    public override void FireAlt() {
        _isFiringContinuously = false;

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

        if (result.Count == 0 || (GodotObject)result["collider"] is not IProp prop) {
            PlaySfx(_sfxNoUse);
            return;
        }

        var mesh = prop.GetMesh();
        if (mesh == null) return;
        CreateBeamFx((Vector3)result["position"], BeamWidthBlast);
        PlaySfx(_sfxBlast);
        Corrupt(mesh, IntensityBlast);
    }

    // --- Private

    private unsafe void Corrupt(ArrayMesh mesh, uint intensity) {
        var arrays = mesh.SurfaceGetArrays(0);
        var verts = arrays[(int)Mesh.ArrayType.Vertex].AsVector3Array();
        if (verts.Length == 0) return;

        var primitive = mesh.SurfaceGetPrimitiveType(0);
        var material = mesh.SurfaceGetMaterial(0);

        var numFloats = verts.Length * 3;
        var iterations = numFloats * intensity / 100;
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

    private void CreateBeamFx(Vector3 target, float beamWidth) {
        var start = _beamStartMarker.GlobalPosition;

        var mesh = new ImmediateMesh();
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
        GetTree().CreateTimer(0.1).Timeout += beam.QueueFree;
    }

    private void PlaySfx(AudioStream sfx) {
        //  if (sfx == _sfxContinuous && _audioStreamPlayer.Playing && _audioStreamPlayer.Stream == sfx) {
        //      return;
        //  }

        _audioStreamPlayer.VolumeLinear = sfx == _sfxNoUse ? 0.25f : 1f;
        _audioStreamPlayer.Stream = sfx;
        _audioStreamPlayer.Play();
    }

    /* private void StopContinuousSfx() {
         if (!_audioStreamPlayer.Playing || _audioStreamPlayer.Stream != _sfxContinuous) {
             return;
         }

         _audioStreamPlayer.Stop();
     } */
}