using GoBox.interfaces;
using Godot;

namespace GoBox;

[GlobalClass]
public partial class Seat : RigidBody3D, IPhysActor, IInteractable, IPickupable {
    public Player Occupant { get; private set; }
    public bool Occupied => Occupant != null;

    [Export] private Marker3D _cameraMarker;


    private IPickupHolder _holder;
    private bool _previouslyOccupied;


    // --- Node

    public override void _Ready() {
        CollisionLayer = (uint)(ColLayer.Props | ColLayer.Toolgunnable);
        CollisionMask = (uint)(ColLayer.World | ColLayer.PhysActors);
    }

    public override void _Notification(int what) {
        if (what == NotificationPredelete) {
            Exit();
        }
    }


    public override void _Process(double delta) {
        if (!Game.World.Bounds.HasPoint(GlobalPosition)) {
            QueueFree();
        }

        if (!Occupied) return;

        // TODO: Remove all input code from here and make player responsible for this instead.
        if (Input.IsActionJustPressed("interact") && _previouslyOccupied) {
            Exit();
        }

        _previouslyOccupied = Occupied;
    }

    // --- IInteractable

    public bool Available() => Occupied;

    public void Interact(Player whosThere) {
        if (whosThere == Occupant) Exit();
        else Enter(whosThere);
    }

    public string Prompt => "Sit";


    // --- IPickupable

    public Node3D AsNode3D() => this;
    public bool AllowPlayerPickup() => true;
    public float Radius() => 0.5f /* TODO */;
    public IPickupHolder Holder() => _holder;
    public void Pickup(IPickupHolder holder) => _holder = holder;
    public void Drop() => _holder = null;


    // --- IPhysActor

    public void ApplyForce(Vector3 force) => ApplyCentralForce(force);
    public void ApplyImpulse(Vector3 impulse) => ApplyCentralImpulse(impulse);


    // --- Private

    private void Enter(Player player) {
        if (Occupied) return;
        Occupant = player;
        Occupant.GetParent()?.RemoveChild(Occupant);
        AddChild(Occupant);
        Occupant.AssumeControl(this);
        Occupant.CameraRig.FollowTarget = _cameraMarker;
        Occupant.CameraRig.Reset();

        Occupant.Freeze = true;
        Occupant.CollisionLayer = 0;
        Occupant.Position = Vector3.Zero;
        Occupant.Rotation = Vector3.Zero;
    }

    private void Exit() {
        if (Occupant == null) return;
        Occupant.GetParent()?.RemoveChild(Occupant);
        Game.World.AddActor(Occupant);
        Occupant.UnassumeControl();
        Occupant.GlobalPosition = GlobalPosition;
        Occupant = null;
    }
}