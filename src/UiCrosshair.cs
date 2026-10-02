using Godot;

namespace GoBox;

[GlobalClass]
public partial class UiCrosshair(Player target) : Control {
    private Player _player = target;

    private ColorRect _colorRect = new();

    private const float CrosshairSize = 3f;
    private Color _colorNormal = Colors.White;
    private Color _colorCanInteract = Colors.Goldenrod;
    private Color _colorInteracting = Colors.DarkGoldenrod;

    public override void _Ready() {
        AddChild(_colorRect);
        _colorRect.Size = new Vector2(CrosshairSize, CrosshairSize);
    }

    public override void _Process(double delta) {
        if (!IsVisibleInTree()) return;

        if (!IsInstanceValid(_player)) return;

        var rect = GetViewportRect();
        GlobalPosition = rect.Position;
        Size = rect.Size;

        _colorRect.Position = Size / 2f - _colorRect.Size / 2f;
        if (_player.IsInteracting()) _colorRect.Color = _colorInteracting;
        else if (_player.IsInteractionAvailable()) _colorRect.Color = _colorCanInteract;
        else _colorRect.Color = _colorNormal;
    }
}