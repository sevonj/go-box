using Godot;

namespace GoBox;

[GlobalClass]
public partial class VehicleWheel : Node3D {
    [Export] public float SpringLength = 1f;
    [Export] public float Radius = 0.29f;
    [Export] public float Width = 0.215f;

}