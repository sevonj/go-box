using Godot;

namespace GoBox;

[GlobalClass]
public partial class UiWeaponHud : Control {
    [Export] private Label _wpnNameLabel;
    [Export] private Label _wpnDescriptionLabel;
    [Export] private TextureRect _wpnIcon;

    private Player _player;

    public static UiWeaponHud Instantiate(Player target) {
        var hud = (UiWeaponHud)GD.Load<PackedScene>("res://ui/ui_weapon_hud.tscn").Instantiate();
        hud._player = target;
        return hud;
    }

    private UiWeaponHud() {
    }

    public override void _Ready() {
    }

    public override void _Process(double delta) {
        if (!IsVisibleInTree()) return;

        if (!IsInstanceValid(_player)) return;
        if (!_player.HasControl()) return;
        var weapon = _player.CurrentWeapon();
        if (!IsInstanceValid(weapon)) {
            Visible = false;
            return;
        }

        Visible = true;

        var rect = GetViewportRect();
        GlobalPosition = rect.Position;
        Size = rect.Size;

        _wpnNameLabel.Text = weapon.WpnName();
        _wpnDescriptionLabel.Text = weapon.WpnDescription();
    }
}