using GoBox.interfaces;
using Godot;

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

    public override string WpnName() => "Propgun";
    public override string WpnDescription() => "Fire: Shoot out a prop. Alt fire: spawn prop. TAB: prop menu";

    public override void Fire() {
        var prop = SpawnProp();
        if (prop is IPhysActor physProp) {
            physProp.ApplyImpulse(-GlobalBasis.Column2.Normalized() * LaunchImpulse);
        }
    }

    public override void FireAlt() {
        SpawnProp();
    }

    // --- Private

    private Node3D SpawnProp() {
        if (LocalVars.SelectedSpawnableDef == null) {
            GD.PushError("WpnPropGun: No spawnable selected");
            return null;
        }

        var prop = content.Content.InstantiateProp(LocalVars.SelectedSpawnableDef.Id);
        if (prop == null) {
            return null;
        }

        Game.World.AddActor(prop);
        prop.GlobalPosition = _propSpawnMarker.GlobalPosition;
        prop.GlobalRotation = GlobalRotation;
        return prop;
    }
}