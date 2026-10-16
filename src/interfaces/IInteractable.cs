namespace GoBox.interfaces;

public interface IInteractable {
    public bool Available();
    public void Interact(Player whosThere);
    public string Prompt => "Use";
}