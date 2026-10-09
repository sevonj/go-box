using GoBox.interfaces;
using Godot;

namespace GoBox;

[GlobalClass]
public partial class PhysProp : RigidBody3D, IProp, IPhysActor, IPickupable {
    private PlayerPickupMgr _holder;
    [Export] public MeshInstance3D Model { get; private set; }

    public PhysProp() {
        ProcessPhysicsPriority = PProcessPriorities.PhysProp;
    }

    public ArrayMesh GetMesh() {
        return (ArrayMesh)Model.Mesh;
    }

    public override void _Ready() {
        Game.World.RegisterPhysProp(this);
        ContactMonitor = true;
        MaxContactsReported = 16;
    }

    public override void _Process(double delta) {
        return;
        var fps = (float)Engine.GetFramesPerSecond();
        var pfps = Engine.PhysicsTicksPerSecond;

        if (fps > pfps || true) {
            Model.SetAsTopLevel(true);
            var weight = (float)delta * 25f;
            Model.GlobalPosition = Model.GlobalPosition.Lerp(GlobalPosition + LinearVelocity / fps, weight);
            Model.GlobalRotation = Model.GlobalRotation.Lerp(GlobalRotation, weight);
        }
        else {
            Model.SetAsTopLevel(false);
            Model.Position = new Vector3();
            Model.Rotation = new Vector3();
        }
    }

    public override void _PhysicsProcess(double delta) {
        if (_holder != null) {
            GravityScale = 0f;

            var posDiff = _holder.DesiredHoldPosition() - GlobalPosition;
            LinearVelocity = posDiff * 100f;

            var currentRot = GlobalBasis.GetRotationQuaternion();
            var targetRot = Quaternion.FromEuler(_holder.DesiredHoldRotation());
            var rotDiff = targetRot * currentRot.Inverse();
            if (rotDiff.W < 0f) rotDiff *= -1f;

            var w = Mathf.Clamp(rotDiff.W, -1f, 1f);
            var angle = 2f * Mathf.Acos(w);
            var s = Mathf.Sqrt(1f - w * w);

            var alreadyAligned = Mathf.IsZeroApprox(s * angle);
            if (!alreadyAligned) {
                var axis = new Vector3(rotDiff.X, rotDiff.Y, rotDiff.Z) / s;
                AngularVelocity = axis * angle * 50f;
            }
            else AngularVelocity = Vector3.Zero;
        }
        else {
            GravityScale = 1f;
        }

        foreach (var node in GetCollidingBodies()) {
            if (node is PropStaticBrk brk) brk.Break();
        }
    }

    // --- IPhysActor 

    public void ApplyForce(Vector3 force) => ApplyCentralForce(force);
    public void ApplyImpulse(Vector3 impulse) => ApplyCentralImpulse(impulse);


    // --- IPickupable

    public Node3D AsNode3D() => this;
    public float Radius() => 0.5f /* TODO */;
    public bool CanBePicked() => _holder == null;
    public PlayerPickupMgr Holder() => _holder;

    public void Pickup(PlayerPickupMgr pickupMgr) {
        _holder = pickupMgr;
        // Freeze = true;
    }

    public void Drop() {
        _holder = null;
        // Freeze = false;
    }
}