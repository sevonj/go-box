using GoBox.content;
using GoBox.interfaces;
using Godot;

namespace GoBox;

[GlobalClass]
public partial class PropPhys : RigidBody3D, IProp, IPhysActor, IPickupable {
    private IPickupHolder _holder;
    [Export] public MeshInstance3D Model { get; private set; }
    [Export] public CollisionShape3D CollisionShape { get; private set; }

    public PropPhys() {
        ProcessPhysicsPriority = PProcessPriorities.PhysProp;
    }

    public PropPhys(ArrayMesh mesh, Shape3D coll, float mass = 1f) {
        ProcessPhysicsPriority = PProcessPriorities.PhysProp;

        if (mass <= 0f) {
            GD.PushWarning("PropPhys: Mass is zero or negative. Using 1.0 instead.");
            mass = 1f;
        }

        if (coll is ConcavePolygonShape3D) {
            GD.PushError("PropPhys: Concave shapes are not supported.");
        }

        Model = new MeshInstance3D { Mesh = mesh };
        Model.Name = "Model";
        CollisionShape = new CollisionShape3D { Shape = coll };
        CollisionShape.Name = "Coll";
        Mass = mass;
    }

    public static PropPhys FromDef(PropPhysDef def) {
        var mesh = Content.GetMesh(def.ModelPath);
        var coll = Content.GetCollMesh(def.CollPath);
        var mass = def.Mass;

        if (mesh == null || coll == null) {
            GD.PushError($"PropStaticBrk: load failed!\n" +
                         $" - mesh: {mesh}\n" +
                         $" - coll: {coll}\n" +
                         $" - def: {def}");
            mesh?.Free();
            coll?.Free();
            return null;
        }

        return new PropPhys(mesh, coll, mass);
    }


    public ArrayMesh GetMesh() {
        return (ArrayMesh)Model.Mesh;
    }

    public override void _Ready() {
        Game.World.RegisterPhysProp(this);
        ContactMonitor = true;
        MaxContactsReported = 16;

        AddChild(Model);
        AddChild(CollisionShape);
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
        if (_holder != null) GravityScale = 0f;
        else GravityScale = 1f;

        foreach (var node in GetCollidingBodies()) {
            if (node is PropStaticBrk brk) brk.Break();
        }
    }

    // --- IPhysActor 

    public void ApplyForce(Vector3 force) => ApplyCentralForce(force);
    public void ApplyImpulse(Vector3 impulse) => ApplyCentralImpulse(impulse);

    // --- IPickupable

    public Node3D AsNode3D() => this;
    public bool AllowPlayerPickup() => true;
    public float Radius() => 0.5f /* TODO */;
    public IPickupHolder Holder() => _holder;
    public void Pickup(IPickupHolder holder) => _holder = holder;
    public void Drop() => _holder = null;
}