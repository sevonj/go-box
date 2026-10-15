using Godot;

namespace GoBox;

[GlobalClass]
public partial class UiContentSourceSeparator : MarginContainer {
    [Export] public Label Label { get; private set; }

    public static UiContentSourceSeparator Instantiate() => (UiContentSourceSeparator)GD.Load<PackedScene>("res://ui/ui_content_source_separator.tscn").Instantiate();

    private UiContentSourceSeparator() {
    }
}