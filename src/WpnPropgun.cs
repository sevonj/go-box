using Godot;

namespace GoBox;

[GlobalClass]
public partial class WpnPropgun : Weapon {
    [Export] public PackedScene PropPrefab;
    [Export] public float LaunchImpulse = 50f;

    [Export] private Marker3D _propSpawnMarker;

    public static WpnPropgun Instantiate() =>
        (WpnPropgun)GD.Load<PackedScene>("res://scenes/wpn_propgun.tscn").Instantiate();

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
        prop?.ApplyImpulse(-GlobalBasis.Column2.Normalized() * prop.Mass * LaunchImpulse);
    }

    public override void FireAlt() {
        SpawnProp();
    }

    // --- Private

    private PhysProp SpawnProp() {
        var packed = LocalVars.SelectedPhysPropPacked;

        if (!IsInstanceValid(packed)) {
            GD.PushError("WpnPropGun: No prop");
            return null;
        }

        var inst = packed.Instantiate();
        if (inst is not PhysProp prop) {
            GD.PushError("WpnPropGun: Prop prefab is not a PhysProp");
            inst.Free();
            return null;
        }

        Game.World.AddActor(prop);
        var propRadiusOffset = -_propSpawnMarker.GlobalBasis.Column2.Normalized() * prop.Radius();
        prop.GlobalPosition = _propSpawnMarker.GlobalPosition + propRadiusOffset;
        prop.GlobalRotation = GlobalRotation;
        return prop;
    }
}