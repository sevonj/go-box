namespace GoBox.interfaces;

public interface ISaveable : IActor {
    public string SaveState();
    public void LoadState(string json);
}