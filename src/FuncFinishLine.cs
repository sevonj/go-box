using GoBox.interfaces;
using Godot;

namespace GoBox;

[GlobalClass]
public partial class FuncFinishLine : Area3D, IPickupable {
    [Signal]
    public delegate void PlayerEnteredEventHandler(Player player);

    private IPickupHolder _holder;

    public FuncFinishLine() {
        GD.Print($"I am a '{nameof(FuncFinishLine)}'");
        AddToGroup(nameof(FuncFinishLine));
    }


    // --- Node

    public override void _Ready() {
        CollisionLayer = (uint)(ColLayer.Toolgunnable | ColLayer.World);
        CollisionMask = (uint)ColLayer.PhysActors;
        
        BodyEntered += OnBodyEntered;
    }


    // --- IPickupable

    public Node3D AsNode3D() => this;
    public bool AllowPlayerPickup() => false;
    public float Radius() => 0f;
    public IPickupHolder Holder() => _holder;
    public void Pickup(IPickupHolder holder) => _holder = holder;
    public void Drop() => _holder = null;


    // --- Private

    private void OnBodyEntered(Node body) {
        if (body is Player player) {
            EmitSignal(SignalName.PlayerEntered, player);
        }
    }
}