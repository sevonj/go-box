using GoBox.interfaces;
using Godot;

namespace GoBox;

public partial class PlayerPickupMgr : Node3D, IPickupHolder {
    public bool Enabled = true;
    public float PickDistance = 2f;
    public float HoldDistance = 1f;
    public float ThrowForce = 10f;
    public Node3D FollowTarget;

    private IPickupable _carrying;
    private IPickupable _pickable;
    private Node3D _pickupTarget = new();
    private bool _pickupQueued;
    private bool _dropQueued;
    private bool _throwQueued;

    public bool CanPickup => !IsCarrying && (_pickable?.AllowPlayerPickup() ?? false) && _pickable.Holder() == null;
    public bool IsCarrying => _carrying != null;

    // --- Node

    public override void _Ready() {
        AddChild(_pickupTarget);
    }

    public override void _Process(double delta) {
        if (!Enabled) return;

        if (_carrying != null) {
            if (Input.IsActionJustPressed("pickup_drop")) _dropQueued = true;
            else if (Input.IsActionJustPressed("pickup_throw")) _throwQueued = true;
        }
        else if (Input.IsActionJustPressed("pickup")) _pickupQueued = true;
    }

    public override void _PhysicsProcess(double delta) {
        _pickable = null;
        var pickupQueued = _pickupQueued;
        var dropQueued = _dropQueued;
        var throwQueued = _throwQueued;
        _pickupQueued = false;
        _dropQueued = false;
        _throwQueued = false;

        if (!Enabled) return;
        if (FollowTarget == null) return;

        _pickupTarget.Position = Vector3.Forward * (HoldDistance + _carrying?.Radius() ?? 0f);

        GlobalPosition = FollowTarget.GlobalPosition;
        GlobalRotation = FollowTarget.GlobalRotation;

        if (_carrying != null) {
            if (_carrying.Holder() != this) {
                GD.PrintErr($"PlayerPickupMgr: wtf — I'm holding a pickup picked up by someone else! (held by: {_carrying.Holder()})");
                _carrying = null;
                return;
            }

            if (dropQueued) Drop();
            else if (throwQueued) Throw();

            return;
        }

        _pickable = FindPickup();

        if (_pickable == null) return;
        if (_pickable.Holder() != null) return;

        if (pickupQueued) Pickup(_pickable);
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
                GlobalPosition - GlobalBasis.Column2 * PickDistance
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
        // Update with new radius for current tick. Fixes punching yourself in the face with the prop when picking it up.
        _pickupTarget.Position = Vector3.Forward * (HoldDistance + _carrying?.Radius() ?? 0f);
    }

    private void Drop() {
        _carrying?.Drop();
        _carrying = null;
    }

    private void Throw() {
        _carrying?.Drop();
        if (_carrying is IPhysActor physActor) {
            physActor.ApplyImpulse(-GlobalBasis.Column2 * ThrowForce);
        }

        _carrying = null;
    }
}