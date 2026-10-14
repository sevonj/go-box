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
    private IPickupable _pickable;

    public static WpnManipulator Instantiate() => (WpnManipulator)GD.Load<PackedScene>("res://scenes/wpn_manipulator.tscn").Instantiate();

    private WpnManipulator() {
        AddChild(_pickupTarget);
    }


    // --- Node

    public override void _Ready() {
        if (!IsInstanceValid(_pickupTarget))
            GD.PushError("WpnManipulator: No _pickupTarget");
    }

    public override void _PhysicsProcess(double delta) {
        if (!Visible) {
            Drop();
            return;
        }

        _pickupTarget.Position = Vector3.Forward * _holdDistance;

        if (_carrying == null) {
            _pickable = FindPickup();
            return;
        }

        if (_carrying.Holder() != this) {
            GD.PrintErr($"WpnManipulator: wtf — I'm holding a pickup picked up by someone else! (held by: {_carrying.Holder()})");
            _carrying = null;
            return;
        }

        var targetPos = _pickupTarget.GlobalPosition;
        var targetRot = _pickupTarget.GlobalRotation;

        if (_carrying is PropPhys physProp) {
            var posDiff = targetPos - physProp.GlobalPosition;
            physProp.LinearVelocity = posDiff * 100f;

            var currentRot = physProp.GlobalBasis.GetRotationQuaternion();
            var targetQuat = Quaternion.FromEuler(targetRot);
            var rotDiff = targetQuat * currentRot.Inverse();
            if (rotDiff.W < 0f) rotDiff *= -1f;

            var w = Mathf.Clamp(rotDiff.W, -1f, 1f);
            var angle = 2f * Mathf.Acos(w);
            var s = Mathf.Sqrt(1f - w * w);

            var alreadyAligned = Mathf.IsZeroApprox(s * angle);
            if (alreadyAligned) {
                physProp.AngularVelocity = Vector3.Zero;
                return;
            }

            var axis = new Vector3(rotDiff.X, rotDiff.Y, rotDiff.Z) / s;
            physProp.AngularVelocity = axis * angle * 50f;

            return;
        }

        var threeDee = (Node3D)_carrying;
        threeDee.GlobalPosition = targetPos;
        threeDee.GlobalRotation = targetRot;
    }


    // --- Weapon

    public override string WpnName() => "Manipulator";
    public override string WpnDescription() => "Fire: Pick up or drop object";

    public override void Fire() {
        if (_carrying != null) {
            Drop();
            return;
        }

        if (_pickable == null) return;
        if (_pickable.Holder() != null) return;

        Pickup(_pickable);
    }

    public override void FireAlt() {
    }


    // --- IPickupHolder

    public Vector3 DesiredHoldPosition() => _pickupTarget.GlobalPosition;
    public Vector3 DesiredHoldRotation() => _pickupTarget.GlobalRotation;


    // --- Private

    private IPickupable FindPickup() {
        var spaceState = GetWorld3D().DirectSpaceState;
        var result = spaceState.IntersectRay(
            PhysicsRayQueryParameters3D.Create(
                GlobalPosition,
                GlobalPosition - GlobalBasis.Column2 * MaxDistance
            )
        );
        if (result.Count == 0) return null;
        if ((GodotObject)result["collider"] is IPickupable pickupable) return pickupable;
        return null;
    }

    private void Pickup(IPickupable pickupable) {
        Drop();
        if (_pickable.Holder() != null) return;
        _carrying = pickupable;
        _carrying.Pickup(this);
        _pickupTarget.GlobalRotation = _carrying.AsNode3D().GlobalRotation;
        _holdDistance = GlobalPosition.DistanceTo(_carrying.AsNode3D().GlobalPosition);
        // Update with new radius for current tick. Fixes punching yourself in the face with the prop when picking it up.
        _pickupTarget.Position = Vector3.Forward * _holdDistance;
    }

    private void Drop() {
        _carrying?.Drop();
        _carrying = null;
    }
}