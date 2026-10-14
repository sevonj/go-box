using Godot;

namespace GoBox.interfaces;

public interface IProp : IActor {
    /// <returns>Id this prop can be instantiated with</returns>
    public string GetId();

    public ArrayMesh GetMesh();
}