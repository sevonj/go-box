using Godot;

namespace GoBox;

[GlobalClass]
public partial class WpnManipulator : Weapon {
    [Export] public PackedScene PropPrefab;
    [Export] public float LaunchImpulse = 50f;

    [Export] private Marker3D _propSpawnMarker;

    public static WpnManipulator Instantiate() => (WpnManipulator)GD.Load<PackedScene>("res://scenes/wpn_manipulator.tscn").Instantiate();

    private WpnManipulator() {
    }

    // --- Node

    public override void _Ready() {
        if (!IsInstanceValid(_propSpawnMarker))
            GD.PushError("WpnManipulator: No prop spawn marker");
    }

    // --- Weapon

    public override void Fire() {
    }

    public override void FireAlt() {
    }
}