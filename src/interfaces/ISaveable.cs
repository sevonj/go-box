namespace GoBox.interfaces;

public interface ISaveable : ISpawnable {
    public string SaveState();
    public void LoadState(string json);
}