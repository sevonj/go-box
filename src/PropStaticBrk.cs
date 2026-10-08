using GoBox.content;
using GoBox.interfaces;
using Godot;

namespace GoBox;

[GlobalClass]
public partial class PropStaticBrk : StaticBody3D, IHealth {
    [Export] public Node3D Model { get; private set; }
    [Export] public Node3D ModelDestroy { get; private set; }
    [Export] public CollisionShape3D CollisionShape;

    public bool IsBroken { get; private set; }

    public PropStaticBrk() {
    }

    public PropStaticBrk(Node3D model, Node3D modelDestroy, CollisionShape3D collisionShape) {
        Model = model;
        Model.Name = "Model";
        ModelDestroy = modelDestroy;
        ModelDestroy.Name = "ModelDestroy";
        CollisionShape = collisionShape;
        CollisionShape.Name = "Coll";
    }

    public static PropStaticBrk FromDef(PropStaticBrkDef def) {
        var model = GoboxGltf.Load(Content.FullPath(def.ModelPath));
        if (model == null) {
            GD.PrintErr($"PropStaticBrk from def: couldn't load model at {def.ModelPath}!");
            return null;
        }

        var modelDestroy = GoboxGltf.Load(Content.FullPath(def.ModelDestroyPath));
        if (modelDestroy == null) {
            GD.PrintErr($"PropStaticBrk from def: couldn't load modelDestroy at {def.ModelDestroyPath}!");
            model.Free();
            return null;
        }

        var collisionShape = GoboxColl.Load(Content.FullPath(def.CollPath));
        if (collisionShape == null) {
            GD.PrintErr($"PropStaticBrk from def: couldn't load coll at {def.CollPath}!");
            model.Free();
            modelDestroy.Free();
            return null;
        }

        return new PropStaticBrk(model, modelDestroy, collisionShape);
    }

    public void Break() {
        if (IsBroken) return;
        IsBroken = true;

        Model.Hide();
        ModelDestroy.Show();

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
        ModelDestroy.Hide();
    }

    public override void _Ready() {
        AddChild(Model);
        AddChild(CollisionShape);
        AddChild(ModelDestroy);
        ModelDestroy.Hide();
    }

    public override void _Process(double delta) {
        if (Input.IsKeyPressed(Key.P)) Reset();
    }

    // --- IHealth

    public int Hp { get; set; } = 1;
    public int MaxHp { get; set; } = 1;
    public bool IsDead() => IsBroken;
    public void Damage(int dmg) => Break();
    public void Kill() => Break();
}