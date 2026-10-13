using Godot;

namespace GoBox;

[GlobalClass]
public partial class UiPressStartPrompt : Control {
    [Signal]
    public delegate void PressedEventHandler();

    public static UiPressStartPrompt Instantiate() => (UiPressStartPrompt)GD.Load<PackedScene>("res://ui/ui_press_start_prompt.tscn").Instantiate();

    public override void _Process(double delta) {
        if (
            Input.IsActionJustPressed("ui_accept")
            || Input.IsMouseButtonPressed(MouseButton.Left)
            || Input.IsMouseButtonPressed(MouseButton.Right)
        ) {
            EmitSignal(SignalName.Pressed);
        }
    }
}