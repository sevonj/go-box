using System;
using Godot;

namespace GoBox;

[GlobalClass]
public partial class CameraRigFp(Player player, Node3D followTarget) : CameraRig {
    public override void _Ready() {
        Camera = new Camera3D();
        AddChild(Camera);

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

        if (!IsInstanceValid(Camera)) {
            GD.PrintErr(GetPath(), " NO CAMERA");
            return;
        }

        GlobalPosition = followTarget.GlobalPosition;

        var inputVec = Input.GetVector(
            "look_right",
            "look_left",
            "look_down",
            "look_up") * GamepadSensitivity * (float)delta;

        if (!player.HasCameraControl()) inputVec *= 0f;
        if (InvertX) inputVec.X *= -1;
        if (InvertY) inputVec.Y *= -1;

        RotateView(inputVec);
    }

    public override void _Input(InputEvent @event) {
        switch (@event) {
            case InputEventMouseMotion motion: {
                var inputVec = -motion.Relative / 100f * MouseSensitivity;

                if (!player.HasCameraControl()) inputVec *= 0f;
                if (InvertX) inputVec.X *= -1;
                if (InvertY) inputVec.Y *= -1;

                RotateView(inputVec);
                break;
            }
        }
    }

    private void RotateView(Vector2 inputVec) {
        RotateY(inputVec.X);
        var rotX = Mathf.Clamp(Camera.Rotation.X + inputVec.Y, MinTilt, MaxTilt);
        Camera.Rotation = Vector3.Right * rotX;
    }
}