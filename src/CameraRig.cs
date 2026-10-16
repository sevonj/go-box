using System;
using Godot;

namespace GoBox;

[GlobalClass]
public abstract partial class CameraRig : Node3D {
    [Export] public Vector2 MouseSensitivity { get; set; } = new(0.7f, 0.7f);
    [Export] public Vector2 GamepadSensitivity { get; set; } = new(2.5f, 2.5f);
    [Export] public bool InvertX { get; set; }
    [Export] public bool InvertY { get; set; }
    [Export] public float MaxTilt { get; set; } = Mathf.DegToRad(75);
    [Export] public float MinTilt { get; set; } = Mathf.DegToRad(-75);
    [Export] public bool InputEnabled { get; set; } = true;

    public Camera3D Camera { get; protected set; }
    public Node3D FollowTarget;


    public abstract void Reset();
}