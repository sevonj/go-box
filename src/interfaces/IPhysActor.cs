using Godot;

namespace GoBox.interfaces;

public interface IPhysActor : IActor {
    public Vector3 GetLinearVelocity();
    public void ApplyForce(Vector3 force);
    public void ApplyImpulse(Vector3 impulse);
}