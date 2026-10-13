using Godot;

namespace GoBox;

[GlobalClass]
public partial class FuncFinishLine : Area3D {
    [Signal]
    public delegate void PlayerEnteredEventHandler(Player player);


    public FuncFinishLine() {
        GD.Print($"I am a '{nameof(FuncFinishLine)}'");
        AddToGroup(nameof(FuncFinishLine));
    }

    // --- Node
    public override void _Ready() {
        BodyEntered += OnBodyEntered;
    }

    // --- Private
    private void OnBodyEntered(Node body) {
        if (body is Player player) {
            EmitSignal(SignalName.PlayerEntered, player);
        }
    }
}