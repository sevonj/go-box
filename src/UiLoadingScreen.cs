using Godot;

namespace GoBox;

[GlobalClass]
public partial class UiLoadingScreen : Control {
    [Export] private Node3D _rotatingCube;


    public static UiLoadingScreen Instantiate() => (UiLoadingScreen)GD.Load<PackedScene>("res://ui/ui_loading_screen.tscn").Instantiate();

    public override void _Process(double delta) {
        _rotatingCube.RotateY((float)delta * 1f);
    }
}