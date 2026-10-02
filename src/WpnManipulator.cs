using Godot;

namespace GoBox;

[GlobalClass]
public partial class WpnManipulator {
    [Export] public PackedScene PropPrefab;
    [Export] public float LaunchImpulse = 50f;

    [Export] private Marker3D _propSpawnMarker;

    public static WpnManipulator Instantiate() =>
        (WpnManipulator)GD.Load<PackedScene>("res://scenes/wpn_manipulator.tscn").Instantiate();

    private WpnManipulator() {
    }

    public override void _Ready() {
        if (!IsInstanceValid(_propSpawnMarker))
            GD.PushError("WpnManipulator: No prop spawn marker");
    }
}

public partial class WpnManipulator : Weapon {
    public override void Fire() {
        var prop = SpawnProp();
        prop?.ApplyImpulse(-GlobalBasis.Column2.Normalized() * prop.Mass * LaunchImpulse);
    }

    public override void FireAlt() {
        SpawnProp();
    }
}

public partial class WpnManipulator {
    private PhysProp SpawnProp() {
        var packed = LocalVars.SelectedPhysPropPacked;

        if (!IsInstanceValid(packed)) {
            GD.PushError("WpnManipulator: No prop");
            return null;
        }

        var inst = packed.Instantiate();
        if (inst is not PhysProp prop) {
            GD.PushError("WpnManipulator: Prop prefab is not a PhysProp");
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