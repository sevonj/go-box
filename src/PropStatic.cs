using GoBox.content;
using Godot;

namespace GoBox;

[GlobalClass]
public partial class PropStatic : StaticBody3D {
    [Export] public Node3D Model { get; private set; }
    [Export] public CollisionShape3D CollisionShape;

    public PropStatic() {
    }

    public PropStatic(Node3D model, CollisionShape3D collisionShape) {
        Model = model;
        Model.Name = "Model";
        CollisionShape = collisionShape;
        CollisionShape.Name = "Coll";
    }

    public static PropStatic FromDef(PropStaticDef staticDef) {
        var model = GoboxGltf.Load(Content.FullPath(staticDef.ModelPath));
        if (model == null) {
            GD.PrintErr($"PropStatic from def: couldn't load model at {staticDef.ModelPath}!");
            return null;
        }

        var collisionShape = GoboxColl.Load(Content.FullPath(staticDef.CollPath));
        if (collisionShape == null) {
            GD.PrintErr($"PropStatic from def: couldn't load coll at {staticDef.CollPath}!");
            return null;
        }

        return new PropStatic(model, collisionShape);
    }

    public override void _Ready() {
        AddChild(Model);
        AddChild(CollisionShape);
    }
}