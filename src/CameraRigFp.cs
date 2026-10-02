using System;
using Godot;

namespace GoBox;

[GlobalClass]
public partial class CameraRigFp(Player player, Node3D followTarget) : Node3D {
    [Export] public Vector2 MouseSensitivity { get; set; } = new(0.7f, 0.7f);
    [Export] public Vector2 GamepadSensitivity { get; set; } = new(2.5f, 2.5f);
    [Export] public bool InvertX { get; set; }
    [Export] public bool InvertY { get; set; }
    [Export] public float MaxTilt { get; set; } = Mathf.DegToRad(75);
    [Export] public float MinTilt { get; set; } = Mathf.DegToRad(-75);
    [Export] public bool InputEnabled { get; set; } = true;

    private Camera3D _camera;

    public Camera3D Camera() => _camera;


    public override void _Ready() {
        _camera = new Camera3D();
        AddChild(_camera);

        if (!IsInstanceValid(player)) {
            GD.PrintErr(GetPath(), " Invalid player!");
            QueueFree();
            return;
        }

        if (!IsInstanceValid(followTarget)) {
            GD.PrintErr(GetPath(), " Invalid followTarget!");
            QueueFree();
            return;
        }

        GlobalRotation = followTarget.GlobalRotation;
    }

    public override void _PhysicsProcess(double delta) {
        if (!IsInstanceValid(followTarget) || !IsInstanceValid(player)) {
            QueueFree();
            return;
        }

        if (!IsInstanceValid(_camera)) {
            GD.PrintErr(GetPath(), " NO CAMERA");
            return;
        }

        GlobalPosition = followTarget.GlobalPosition;

        var inputVec = Input.GetVector(
            "look_right",
            "look_left",
            "look_down",
            "look_up") * GamepadSensitivity * (float)delta;

        if (!player.HasControl()) inputVec *= 0f;
        if (InvertX) inputVec.X *= -1;
        if (InvertY) inputVec.Y *= -1;

        RotateView(inputVec);
    }

    public override void _Input(InputEvent @event) {
        switch (@event) {
            case InputEventMouseMotion motion: {
                var inputVec = -motion.Relative / 100f * MouseSensitivity;

                if (!player.HasControl()) inputVec *= 0f;
                if (InvertX) inputVec.X *= -1;
                if (InvertY) inputVec.Y *= -1;

                RotateView(inputVec);
                break;
            }
        }
    }

    private void RotateView(Vector2 inputVec) {
        RotateY(inputVec.X);
        var rotX = Mathf.Clamp(_camera.Rotation.X + inputVec.Y, MinTilt, MaxTilt);
        _camera.Rotation = Vector3.Right * rotX;
    }
}