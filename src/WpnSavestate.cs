using System;
using GoBox.interfaces;
using Godot;

namespace GoBox;

[GlobalClass]
public partial class WpnSavestate : Weapon {
    [Export] private Marker3D _beamStartMarker;
    [Export] private AudioStream _sfxNoUse;
    [Export] private AudioStream _sfxStore;
    [Export] private AudioStream _sfxApply;

    private AudioStreamPlayer _audioStreamPlayer = new();

    private const string BeamMatPath = "res://assets/materials/mat_fx_corruptobeam.tres";
    private const float BeamWidth = 0.2f;

    private string _saveState;
    private Type _targetType;

    public static WpnSavestate Instantiate() => (WpnSavestate)GD.Load<PackedScene>("res://scenes/wpn_savestate.tscn").Instantiate();

    private WpnSavestate() {
    }


    // --- Node

    public override void _Ready() {
        AddChild(_audioStreamPlayer);
        _audioStreamPlayer.Name = "AudioStreamPlayer";
    }


    // --- Weapon

    public override string WpnName() => "Savestate Gun";

    public override string WpnDescription() => "Possibly maybe somehow usable for save scumming.\n" +
                                               "Fire: Apply state\n" +
                                               "Alt fire: Store state\n" +
                                               "States are only applicable to objects of same type.";

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
        var raycastParams = PhysicsRayQueryParameters3D.Create(origin, end);
        raycastParams.CollisionMask = (uint)ColLayer.Toolgunnable;
        raycastParams.CollideWithAreas = true;
        var result = spaceState.IntersectRay(raycastParams);

        if (
            result.Count == 0
            || (GodotObject)result["collider"] is not ISaveable saveable
            || _saveState == null || _targetType == null || saveable.GetType() != _targetType
        ) {
            PlaySfx(_sfxNoUse);
            return;
        }

        saveable.LoadState(_saveState);

        CreateBeamFx((Vector3)result["position"]);
        PlaySfx(_sfxApply);
    }

    public override void FireAlt() {
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

        if (result.Count == 0 || (GodotObject)result["collider"] is not ISaveable saveable) {
            PlaySfx(_sfxNoUse);
            return;
        }

        _saveState = saveable.SaveState();
        _targetType = saveable.GetType();

        CreateBeamFx((Vector3)result["position"]);
        PlaySfx(_sfxStore);
    }


    // --- Private

    private void CreateBeamFx(Vector3 target) {
        var start = _beamStartMarker.GlobalPosition;

        var mesh = new ImmediateMesh();
        var len = start.DistanceTo(target);
        mesh.SurfaceBegin(Mesh.PrimitiveType.TriangleStrip);
        mesh.SurfaceSetUV(new Vector2(0f, 0f));
        mesh.SurfaceAddVertex(new Vector3(0f, BeamWidth / 2f, 0f));
        mesh.SurfaceSetUV(new Vector2(1f, 0f));
        mesh.SurfaceAddVertex(new Vector3(0f, BeamWidth / 2f, -len));
        mesh.SurfaceSetUV(new Vector2(0f, 1f));
        mesh.SurfaceAddVertex(new Vector3(0f, -BeamWidth / 2f, 0f));
        mesh.SurfaceSetUV(new Vector2(1f, 1f));
        mesh.SurfaceAddVertex(new Vector3(0f, -BeamWidth / 2f, -len));
        mesh.SurfaceEnd();
        mesh.SurfaceSetMaterial(0, ResourceLoader.Load<Material>(BeamMatPath));

        var beam = new MeshInstance3D();
        beam.SetMesh(mesh);
        beam.Name = "FxSavestateBeam";
        Game.World.AddActor(beam);
        beam.GlobalPosition = start;
        beam.LookAt(target);
        GetTree().CreateTimer(0.1).Timeout += beam.QueueFree;
    }

    private void PlaySfx(AudioStream sfx) {
        _audioStreamPlayer.VolumeLinear = sfx == _sfxNoUse ? 0.25f : 1f;
        _audioStreamPlayer.Stream = sfx;
        _audioStreamPlayer.Play();
    }
}