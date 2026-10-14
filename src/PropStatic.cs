using GoBox.content;
using GoBox.interfaces;
using Godot;

namespace GoBox;

[GlobalClass]
public partial class PropStatic : StaticBody3D, IProp, IPickupable {
    [Export] public MeshInstance3D Model { get; private set; }
    [Export] public CollisionShape3D CollisionShape;

    private IPickupHolder _holder;

    public PropStatic() {
    }

    public PropStatic(ArrayMesh mesh, Shape3D coll) {
        Model = new MeshInstance3D { Mesh = mesh };
        Model.Name = "Model";
        CollisionShape = new CollisionShape3D { Shape = coll };
        CollisionShape.Name = "Coll";
    }

    public static PropStatic FromDef(PropStaticDef def) {
        var mesh = Content.GetMesh(def.ModelPath);
        var coll = Content.GetCollMesh(def.CollPath);

        if (mesh == null || coll == null) {
            GD.PrintErr($"PropStaticBrk: load failed!\n" +
                        $" - mesh: {mesh}\n" +
                        $" - coll: {coll}\n" +
                        $" - def: {def}");
            mesh?.Free();
            coll?.Free();
            return null;
        }

        return new PropStatic(mesh, coll);
    }


    // --- Node

    public override void _Ready() {
        AddChild(Model);
        AddChild(CollisionShape);
    }

    public override void _PhysicsProcess(double delta) {
        if (_holder != null) {
            GlobalPosition = _holder.DesiredHoldPosition();
            GlobalRotation = _holder.DesiredHoldRotation();
        }
    }


    // --- IProp

    public ArrayMesh GetMesh() => (ArrayMesh)Model.Mesh;


    // --- IPickupable

    public Node3D AsNode3D() => this;
    public bool AllowPlayerPickup() => false;
    public float Radius() => 0f;
    public IPickupHolder Holder() => _holder;
    public void Pickup(IPickupHolder holder) => _holder = holder;
    public void Drop() => _holder = null;
}