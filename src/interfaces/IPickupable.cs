using Godot;

namespace GoBox.interfaces;

public interface IPickupable {
    public Node3D AsNode3D();
    public bool AllowPlayerPickup();
    public float Radius();

    /// <returns>The one holding me, or null.</returns>
    public IPickupHolder Holder();

    public void Pickup(IPickupHolder holder);
    public void Drop();
}