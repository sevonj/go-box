namespace GoBox.interfaces;

public interface ISpawnable : IActor {
    /// <returns>Id of this object (not unique instance id)</returns>
    public string GetId();
}