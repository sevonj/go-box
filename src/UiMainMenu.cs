using GoBox.content;
using Godot;

namespace GoBox;

[GlobalClass]
public partial class UiMainMenu : Control {
    public UiMainMenu() {
        Content.LoadContent();
    }
}