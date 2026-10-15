using Godot;

namespace GoBox.interfaces;

public interface IProp : IActor {
    /// <returns>Id of this object (not unique instance id)</returns>
    public string GetId();

    public ArrayMesh GetMesh();
}