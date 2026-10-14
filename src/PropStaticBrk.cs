using GoBox.content;
using GoBox.interfaces;
using Godot;

namespace GoBox;

[GlobalClass]
public partial class PropStaticBrk : StaticBody3D, IProp, IHealth, IPickupable {
    [Export] public MeshInstance3D Model { get; private set; }
    [Export] public CollisionShape3D CollisionShape { get; private set; }
    [Export] public MeshInstance3D ModelDestroy { get; private set; }
    [Export] public CollisionShape3D CollisionShapeDestroy { get; private set; }

    public bool IsBroken { get; private set; }

    private IPickupHolder _holder;

    public PropStaticBrk() {
    }

    public PropStaticBrk(ArrayMesh mesh, Shape3D coll, ArrayMesh meshDestroy, Shape3D collDestroy) {
        Model = new MeshInstance3D { Mesh = mesh };
        Model.Name = "Model";
        CollisionShape = new CollisionShape3D { Shape = coll };
        CollisionShape.Name = "Coll";
        ModelDestroy = new MeshInstance3D { Mesh = meshDestroy };
        ModelDestroy.Name = "ModelDestroy";
        CollisionShapeDestroy = new CollisionShape3D { Shape = collDestroy };
        CollisionShapeDestroy.Name = "CollDestroy";
    }

    public static PropStaticBrk FromDef(PropStaticBrkDef def) {
        var mesh = Content.GetMesh(def.ModelPath);
        var coll = Content.GetCollMesh(def.CollPath);
        var meshDestroy = Content.GetMesh(def.ModelDestroyPath);
        var collDestroy = Content.GetCollMesh(def.CollDestroyPath);

        if (mesh == null || meshDestroy == null || coll == null || collDestroy == null) {
            GD.PrintErr($"PropStaticBrk: load failed!\n" +
                        $" - mesh: {mesh}\n" +
                        $" - coll: {coll}\n" +
                        $" - meshDestroy: {meshDestroy}\n" +
                        $" - collDestroy: {collDestroy}\n" +
                        $" - def: {def}");
            mesh?.Free();
            coll?.Free();
            meshDestroy?.Free();
            collDestroy?.Free();
            return null;
        }

        return new PropStaticBrk(mesh, coll, meshDestroy, collDestroy);
    }

    public void Break() {
        if (IsBroken) return;
        IsBroken = true;

        Model.Hide();
        CollisionShape.Disabled = true;
        ModelDestroy.Show();
        CollisionShapeDestroy.Disabled = false;

        // if (disableCollisionOnBreak) {
        //     foreach (var child in GetChildren()) {
        //         if (child is CollisionShape3D coll) coll.Disabled = true;
        //     }
        // }

        // foreach (var model in modelsWhole) model.Hide();
        // foreach (var model in modelsBroken) model.Show();

        // if (IsInstanceValid(particles)) particles.Emitting = true;
        // if (IsInstanceValid(animationPlayer)) {
        //     animationPlayer.SetCurrentAnimation("break");
        //     animationPlayer.Active = true;
        //     animationPlayer.Play();
        // }

        // if (gravy > Gravy.NONE) Game.Stage.SpawnGravy(gravy, GlobalPosition);
    }

    public void Reset() {
        // foreach (var child in GetChildren()) {
        //     if (child is CollisionShape3D coll) coll.Disabled = false;
        // }

        // foreach (var model in modelsWhole) model.Show();
        // foreach (var model in modelsBroken) model.Hide();

        // if (IsInstanceValid(animationPlayer)) {
        //     animationPlayer.SetCurrentAnimation("RESET");
        //     animationPlayer.Active = true;
        //     animationPlayer.Play();
        // }

        IsBroken = false;
        Visible = true;
        Model.Show();
        CollisionShape.Disabled = false;
        ModelDestroy.Hide();
        CollisionShapeDestroy.Disabled = true;
    }


    // --- Node

    public override void _Ready() {
        AddChild(Model);
        AddChild(CollisionShape);
        AddChild(ModelDestroy);
        AddChild(CollisionShapeDestroy);
        Reset();
    }

    public override void _Process(double delta) {
        if (Input.IsKeyPressed(Key.P)) Reset();
    }

    // --- IProp

    public ArrayMesh GetMesh() => (ArrayMesh)(IsBroken ? ModelDestroy : Model)?.Mesh;


    // --- IPickupable

    public Node3D AsNode3D() => this;
    public bool AllowPlayerPickup() => false;
    public float Radius() => 0f;
    public IPickupHolder Holder() => _holder;
    public void Pickup(IPickupHolder holder) => _holder = holder;
    public void Drop() => _holder = null;


    // --- IHealth

    public int Hp { get; set; } = 1;
    public int MaxHp { get; set; } = 1;
    public bool IsDead() => IsBroken;
    public void Damage(int dmg) => Break();
    public void Kill() => Break();
}