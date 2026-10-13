using Godot;

namespace GoBox;

[GlobalClass]
public partial class UiResultsScreen : Control {
    [Signal]
    public delegate void ClosedEventHandler();

    [Export] private Button _restartButton;
    [Export] private Button _exitButton;
    [Export] private Button _freeRoamButton;

    public static UiResultsScreen Instantiate() => (UiResultsScreen)ResourceLoader.Load<PackedScene>("res://ui/ui_results_screen.tscn").Instantiate();

    private UiResultsScreen() {
    }

    // --- Node
    public override void _Ready() {
        _restartButton.Pressed += Game.ReloadStage;
        _exitButton.Pressed += Game.QuitToTitle;
        _freeRoamButton.Pressed += QueueFree;

        _restartButton.Pressed += () => EmitSignal(SignalName.Closed);
        _exitButton.Pressed += () => EmitSignal(SignalName.Closed);
        _freeRoamButton.Pressed += () => EmitSignal(SignalName.Closed);
    }
}