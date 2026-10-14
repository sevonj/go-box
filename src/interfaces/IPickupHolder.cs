using Godot;

namespace GoBox.interfaces;

public interface IPickupHolder {
    public Vector3 DesiredHoldPosition();
    public Vector3 DesiredHoldRotation();
}
