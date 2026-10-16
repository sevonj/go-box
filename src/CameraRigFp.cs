using Godot;

namespace GoBox;

[GlobalClass]
public partial class CameraRigFp(Player player, Node3D followTarget) : CameraRig {
    private float _angle;

    public override void Reset() {
        if (!IsInstanceValid(FollowTarget) || !IsInstanceValid(player)) {
            QueueFree();
            return;
        }

        GlobalPosition = FollowTarget.GlobalPosition;
        GlobalRotation = FollowTarget.GlobalRotation;
        _angle = FollowTarget.Rotation.Y;
        Camera.Rotation = Vector3.Zero;
    }

    // --- Node

    public override void _Ready() {
        FollowTarget = followTarget;

        Camera = new Camera3D();
        AddChild(Camera);

        if (!IsInstanceValid(player)) {
            GD.PrintErr(GetPath(), " Invalid player!");
            QueueFree();
            return;
        }

        if (!IsInstanceValid(FollowTarget)) {
            GD.PrintErr(GetPath(), " Invalid followTarget!");
            QueueFree();
            return;
        }

        GlobalRotation = FollowTarget.GlobalRotation;
    }

    public override void _PhysicsProcess(double delta) {
        if (!IsInstanceValid(FollowTarget) || !IsInstanceValid(player)) {
            QueueFree();
            return;
        }

        if (!IsInstanceValid(Camera)) {
            GD.PrintErr(GetPath(), " NO CAMERA");
            return;
        }

        GlobalPosition = FollowTarget.GlobalPosition;
        GlobalRotation = FollowTarget.GlobalRotation;
        Rotate(GlobalTransform.Basis.Column1.Normalized(), _angle);

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

    // --- Private

    private void RotateView(Vector2 inputVec) {
        _angle += inputVec.X; // RotateY(inputVec.X);
        var rotX = Mathf.Clamp(Camera.Rotation.X + inputVec.Y, MinTilt, MaxTilt);
        Camera.Rotation = Vector3.Right * rotX;
    }
}