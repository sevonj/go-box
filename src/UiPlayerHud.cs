using Godot;

namespace GoBox;

[GlobalClass]
public partial class UiPlayerHud(Player target) : Control {
    public override void _Ready() {
        AddChild(new UiCrosshair(target));
        AddChild(UiWeaponHud.Instantiate(target));
    }

    public override void _Process(double delta) {
        if (!IsInstanceValid(target)) {
            QueueFree();
            return;
        }

        Visible = !Game.World.Paused && target.HasControl();
    }
}