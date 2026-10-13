using Godot;

namespace GoBox;

[GlobalClass]
public partial class UiTimerDisplay : Control {
    [Export] private Label _label;

    public static UiTimerDisplay Instantiate() => (UiTimerDisplay)GD.Load<PackedScene>("res://ui/ui_timer_display.tscn").Instantiate();

    private UiTimerDisplay() {
    }

    public void SetTime(double time) {
        _label.Text = Util.FormatStopwatch(time);
    }

    // --- Private
}