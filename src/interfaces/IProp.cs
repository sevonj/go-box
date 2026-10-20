using Godot;

namespace GoBox.interfaces;

public interface IProp : ISpawnable {
    public ArrayMesh GetMesh();
}