using GoBox.interfaces;
using Godot;

namespace GoBox;

[GlobalClass]
public partial class WpnManipulator : Weapon, IPickupHolder {
    [Export] public PackedScene PropPrefab;
    [Export] public float LaunchImpulse = 50f;
    private const float MaxDistance = 500f;
    private float _holdDistance = 1f;

    private Marker3D _pickupTarget = new();

    private IPickupable _carrying;
    private Vector3 _carryingGrabOffset;
    private IPickupable _pickable;
    private Vector3 _pickableRaycastPoint;

    public static WpnManipulator Instantiate() => (WpnManipulator)GD.Load<PackedScene>("res://scenes/wpn_manipulator.tscn").Instantiate();

    private WpnManipulator() {
        AddChild(_pickupTarget);
    }


    // --- Node

    public override void _Ready() {
        if (!IsInstanceValid(_pickupTarget))
            GD.PushError("WpnManipulator: No _pickupTarget");
    }

    public override void _Input(InputEvent @event) {
        switch (@event) {
            case InputEventMouseMotion motion: {
                if (!Input.IsActionPressed("pickup")) {
                    return;
                }

                var inputVec = -motion.Relative * 0.005f;
                _pickupTarget.Rotate(GlobalBasis.Column1, -inputVec.X);
                _pickupTarget.Rotate(GlobalBasis.Column0, inputVec.Y);

                break;
            }
        }
    }

    public override void _PhysicsProcess(double delta) {
        if (!Visible) {
            Drop();
            return;
        }

        _pickupTarget.Position = Vector3.Forward * _holdDistance;

        if (_carrying == null) {
            (_pickable, _pickableRaycastPoint) = FindPickup();
            return;
        }

        if (_carrying.Holder() != this) {
            GD.PrintErr($"WpnManipulator: wtf — I'm holding a pickup picked up by someone else! (held by: {_carrying.Holder()})");
            _carrying = null;
            return;
        }

        if (Input.IsActionPressed("pickup")) {
            var inputVec = Input.GetVector(
                "look_right",
                "look_left",
                "look_down",
                "look_up") * 4f * (float)delta;
            _pickupTarget.Rotate(GlobalBasis.Column1, -inputVec.X);
            _pickupTarget.Rotate(GlobalBasis.Column0, inputVec.Y);
        }


        var targetPos = _pickupTarget.GlobalPosition;
        var targetRot = _pickupTarget.GlobalRotation;

        if (_carrying is PropPhys physProp) {
            var currentRot = physProp.GlobalBasis.GetRotationQuaternion();
            var targetQuat = Quaternion.FromEuler(targetRot);
            var rotDiff = targetQuat * currentRot.Inverse();
            if (rotDiff.W < 0f) rotDiff *= -1f;

            var w = Mathf.Clamp(rotDiff.W, -1f, 1f);
            var angle = 2f * Mathf.Acos(w);
            var s = Mathf.Sqrt(1f - w * w);

            var alreadyAligned = Mathf.IsZeroApprox(s * angle);
            if (alreadyAligned) physProp.AngularVelocity = Vector3.Zero;
            else {
                var axis = new Vector3(rotDiff.X, rotDiff.Y, rotDiff.Z) / s;
                physProp.AngularVelocity = axis * angle * 50f;
            }

            targetPos -= physProp.GlobalTransform * _carryingGrabOffset - physProp.GlobalPosition;

            var posDiff = targetPos - physProp.GlobalPosition;
            physProp.LinearVelocity = posDiff * 100f;

            return;
        }

        var threeDee = (Node3D)_carrying;
        threeDee.GlobalRotation = targetRot;
        targetPos -= threeDee.GlobalTransform * _carryingGrabOffset - threeDee.GlobalPosition;
        threeDee.GlobalPosition = targetPos;
    }


    // --- Weapon

    public override string WpnName() => "Manipulator";
    public override string WpnDescription() => "Fire: Pick up or drop object.";

    public override void Fire() {
        if (_carrying != null) {
            Drop();
            return;
        }

        if (_pickable == null) return;
        if (_pickable.Holder() != null) return;

        Pickup(_pickable, _pickableRaycastPoint);
    }

    public override void FireAlt() {
    }

    public override bool OverrideCamera() {
        return _carrying != null && Input.IsActionPressed("pickup");
    }


    // --- IPickupHolder

    public Vector3 DesiredHoldPosition() => _pickupTarget.GlobalPosition;
    public Vector3 DesiredHoldRotation() => _pickupTarget.GlobalRotation;


    // --- Private

    private (IPickupable, Vector3) FindPickup() {
        var spaceState = GetWorld3D().DirectSpaceState;
        var raycastParams = PhysicsRayQueryParameters3D.Create(
            GlobalPosition,
            GlobalPosition - GlobalBasis.Column2 * MaxDistance
        );
        raycastParams.CollideWithAreas = true;
        var result = spaceState.IntersectRay(raycastParams);

        if (
            result.Count == 0 ||
            (GodotObject)result["collider"] is not IPickupable pickupable
        ) {
            return (null, Vector3.Zero);
        }

        var position = (Vector3)result["position"];
        return (pickupable, position);
    }

    private void Pickup(IPickupable pickupable, Vector3 grabPos) {
        Drop();
        if (_pickable.Holder() != null) return;
        _carrying = pickupable;
        _carrying.Pickup(this);
        _pickupTarget.GlobalRotation = _carrying.AsNode3D().GlobalRotation;
        _carryingGrabOffset = pickupable.AsNode3D().GlobalTransform.Inverse() * grabPos;
        _holdDistance = GlobalPosition.DistanceTo(grabPos);
        // Update with new radius for current tick. Fixes punching yourself in the face with the prop when picking it up.
        _pickupTarget.Position = Vector3.Forward * _holdDistance;
    }

    private void Drop() {
        _carrying?.Drop();
        _carrying = null;
    }
}