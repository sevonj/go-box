using Godot;

namespace GoBox.interfaces;

public interface IPickupable {
    public Node3D AsNode3D();
    public float Radius();

    public bool CanBePicked();

    /// <returns>The one holding me, or null.</returns>
    public PlayerPickupMgr Holder();

    public void Pickup(PlayerPickupMgr pickupMgr);
    public void Drop();
}