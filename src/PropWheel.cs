using System.Text.Json;
using System.Text.Json.Serialization;
using GoBox.content;
using GoBox.interfaces;
using Godot;

namespace GoBox;

[GlobalClass]
public partial class PropWheel : RigidBody3D, IProp, IPhysActor, IPickupable /*, ISaveable*/ {
    public string Id { get; init; }
    private IPickupHolder _holder;
    [Export] public MeshInstance3D Model { get; private set; }
    [Export] public CollisionShape3D CollisionShape { get; private set; }
    [Export] private HingeJoint3D _joint;
    [Export] private Vector3 _offset;

    public PropWheel() {
        ProcessPhysicsPriority = PProcessPriorities.PhysProp;
    }

    public PropWheel(string id, ArrayMesh mesh, Shape3D coll, float mass, Vector3 offset) {
        ProcessPhysicsPriority = PProcessPriorities.PhysProp;
        Id = id;

        if (mass <= 0f) {
            GD.PushWarning("PropWheel: Mass is zero or negative. Using 1.0 instead.");
            mass = 1f;
        }

        if (coll is ConcavePolygonShape3D) {
            GD.PushError("PropWheel: Concave shapes are not supported.");
        }

        Model = new MeshInstance3D { Mesh = mesh };
        Model.Name = "Model";
        CollisionShape = new CollisionShape3D { Shape = coll };
        CollisionShape.Name = "Coll";
        Mass = mass;
        _joint = new HingeJoint3D();
        _offset = offset;
    }

    public static PropWheel FromDef(PropWheelDef def) {
        var id = def.Id;
        var mesh = Content.GetMesh(def.ModelPath);
        var coll = Content.GetCollMesh(def.CollPath);
        var mass = def.Mass;
        var offset = def.Offset;

        if (mesh == null || coll == null) {
            GD.PushError($"PropStaticBrk: load failed!\n" +
                         $" - mesh: {mesh}\n" +
                         $" - coll: {coll}\n" +
                         $" - def: {def}");
            mesh?.Free();
            coll?.Free();
            return null;
        }

        return new PropWheel(id, mesh, coll, mass, offset);
    }

    public void WeldTo(IPhysActor prop, Vector3 position, Vector3 normal) {
        _joint.NodeA = null;
        _joint.NodeB = null;
        GlobalPosition = position;
        var up = normal == Vector3.Up ? Vector3.Forward : Vector3.Up;
        LookAt(GlobalPosition - normal, up);
        _joint.NodeA = prop.AsNode3D().GetPath();
        _joint.NodeB = GetPath();
    }

    // --- Node

    public override void _Ready() {
        CollisionLayer = (uint)(ColLayer.Props | ColLayer.Toolgunnable);
        CollisionMask = (uint)(ColLayer.World | ColLayer.PhysActors);

        // Game.World.RegisterPhysProp(this);
        ContactMonitor = true;
        MaxContactsReported = 16;

        AddChild(Model);
        AddChild(CollisionShape);
        Model.Position = _offset;
        CollisionShape.Position = _offset;
        AddChild(_joint);
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
        if (!Game.World.Bounds.HasPoint(GlobalPosition)) {
            QueueFree();
        }

        if (_holder != null) GravityScale = 0f;
        else GravityScale = 1f;

        foreach (var node in GetCollidingBodies()) {
            if (node is PropStaticBrk brk) brk.Break();
        }
    }


    // --- IProp

    public string GetId() => Id;
    public ArrayMesh GetMesh() => (ArrayMesh)Model.Mesh;


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


    // --- ISaveable

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions {
        Converters = { new Vector3JsonConv() }
    };

    private class PropWheelState {
        [JsonPropertyName("position")] public Vector3 Position { get; set; }
        [JsonPropertyName("rotation")] public Vector3 Rotation { get; set; }
        [JsonPropertyName("linear_velocity")] public Vector3 LinearVelocity { get; set; }
        [JsonPropertyName("angular_velocity")] public Vector3 AngularVelocity { get; set; }
    }

    public string SaveState() {
        var state = new PropWheelState {
            Position = GlobalPosition,
            Rotation = GlobalRotation,
            LinearVelocity = LinearVelocity,
            AngularVelocity = AngularVelocity
        };
        return JsonSerializer.Serialize(state, JsonOptions);
    }

    public void LoadState(string json) {
        var state = JsonSerializer.Deserialize<PropWheelState>(json, JsonOptions);
        if (state == null) {
            GD.PushError("PropWheel: Couldn't parse state");
            return;
        }

        GlobalPosition = state.Position;
        GlobalRotation = state.Rotation;
        LinearVelocity = state.LinearVelocity;
        AngularVelocity = state.AngularVelocity;
    }
}