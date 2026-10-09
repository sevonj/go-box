using GoBox.content;
using GoBox.interfaces;
using Godot;

namespace GoBox;

[GlobalClass]
public partial class PropStatic : StaticBody3D, IProp {
    [Export] public MeshInstance3D Model { get; private set; }
    [Export] public CollisionShape3D CollisionShape;

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

    public override void _Ready() {
        AddChild(Model);
        AddChild(CollisionShape);
    }

    // --- IProp

    public ArrayMesh GetMesh() => (ArrayMesh)Model.Mesh;
}