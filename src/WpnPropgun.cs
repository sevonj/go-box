using GoBox.interfaces;
using Godot;
using Godot.Collections;

namespace GoBox;

[GlobalClass]
public partial class WpnPropgun : Weapon {
    [Export] public PackedScene PropPrefab;
    [Export] public float LaunchImpulse = 50f;

    [Export] private Marker3D _propSpawnMarker;

    public static WpnPropgun Instantiate() => (WpnPropgun)GD.Load<PackedScene>("res://scenes/wpn_propgun.tscn").Instantiate();

    private WpnPropgun() {
    }

    public override void _Ready() {
        if (!IsInstanceValid(_propSpawnMarker))
            GD.PushError("WpnPropgun: No prop spawn marker");
    }

    // --- Weapon

    public override string WpnName() => "Spawngun";

    public override string WpnDescription() => "Fire: Spawn Spawnable\n" +
                                               "Alt fire: Pipette" +
                                               "\nTAB: prop menu";

    public override void Fire() {
        SpawnProp();
    }

    public override void FireAlt() {
        var result = Raycast();
        if (result == null || result.Count == 0) return;

        if ((GodotObject)result["collider"] is IProp prop) {
            LocalVars.SelectSpawnable(prop.GetId());
        }
    }

    // --- Private

    private void SpawnProp() {
        if (LocalVars.SelectedSpawnableDef == null) {
            GD.PushError("WpnPropGun: No spawnable selected");
            return;
        }


        var prop = content.Content.InstantiateProp(LocalVars.SelectedSpawnableDef.Id);
        if (prop == null) {
            return;
        }

        Game.World.AddActor(prop);

        if (prop is PropWheel wheel) {
            var result = Raycast();
            if (result is { Count: > 0 }
                && (GodotObject)result["collider"] is IPhysActor weldTarget
               ) {
                wheel.WeldTo(weldTarget, (Vector3)result["position"], (Vector3)result["normal"]);
                return;
            }
        }

        prop.GlobalPosition = _propSpawnMarker.GlobalPosition;
        prop.GlobalRotation = GlobalRotation;
    }

    private Dictionary Raycast() {
        var cam = GetViewport().GetCamera3D();
        if (!IsInstanceValid(cam)) {
            GD.PrintErr($"{GetPath()}: Couldn't access camera.");
            return null;
        }

        var spaceState = GetWorld3D().DirectSpaceState;
        var mousePos = GetViewport().GetMousePosition();

        var origin = cam.ProjectRayOrigin(mousePos);
        var end = origin + cam.ProjectRayNormal(mousePos) * 100f;
        var raycastParams = PhysicsRayQueryParameters3D.Create(origin, end);
        raycastParams.CollisionMask = (uint)ColLayer.Toolgunnable;
        raycastParams.CollideWithAreas = true;
        return spaceState.IntersectRay(raycastParams);
    }
}