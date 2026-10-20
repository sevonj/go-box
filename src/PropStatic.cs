using System.Text.Json;
using System.Text.Json.Serialization;
using GoBox.content;
using GoBox.interfaces;
using GoBox.types;
using Godot;

namespace GoBox;

[GlobalClass]
public partial class PropStatic : StaticBody3D, IProp, IPickupable, ISaveable {
    public string Id { get; init; }
    [Export] public MeshInstance3D Model { get; private set; }
    [Export] public CollisionShape3D CollisionShape;

    private IPickupHolder _holder;

    public PropStatic() {
    }

    public PropStatic(string id, ArrayMesh mesh, Shape3D coll) {
        Id = id;
        Model = new MeshInstance3D { Mesh = mesh };
        Model.Name = "Model";
        CollisionShape = new CollisionShape3D { Shape = coll };
        CollisionShape.Name = "Coll";
    }

    public static PropStatic FromDef(PropStaticDef def) {
        var id = def.Id;
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

        return new PropStatic(id, mesh, coll);
    }


    // --- Node

    public override void _Ready() {
        CollisionLayer = (uint)(ColLayer.Props | ColLayer.Toolgunnable);
        CollisionMask = 0;

        AddChild(Model);
        AddChild(CollisionShape);
    }


    // --- IProp

    public string GetId() => Id;
    public ArrayMesh GetMesh() => (ArrayMesh)Model.Mesh;


    // --- IPickupable

    public Node3D AsNode3D() => this;
    public bool AllowPlayerPickup() => false;
    public float Radius() => 0f;
    public IPickupHolder Holder() => _holder;
    public void Pickup(IPickupHolder holder) => _holder = holder;
    public void Drop() => _holder = null;


    // --- ISaveable

    private class PropStaticState {
        [JsonPropertyName("position")] public Vector3 Position { get; set; }
        [JsonPropertyName("rotation")] public Vector3 Rotation { get; set; }
    }

    public string SaveState() {
        var state = new PropStaticState {
            Position = GlobalPosition,
            Rotation = GlobalRotation
        };
        return JsonSerializer.Serialize(state, GoboxJson.JsonOptions);
    }

    public void LoadState(string json) {
        var state = JsonSerializer.Deserialize<PropStaticState>(json, GoboxJson.JsonOptions);
        if (state == null) {
            GD.PushError("PropStatic: Couldn't parse state");
            return;
        }

        GlobalPosition = state.Position;
        GlobalRotation = state.Rotation;
    }
}