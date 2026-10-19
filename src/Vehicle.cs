using Godot;
using Godot.Collections;

namespace GoBox;

[GlobalClass]
public partial class Vehicle : RigidBody3D {
    [Export] public Array<Seat> Seats { get; set; }
}