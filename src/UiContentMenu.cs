using System.Linq;
using Godot;

namespace GoBox;

[GlobalClass]
public partial class UiContentMenu : Control {
    [Export] private Window _propWindow;
    [Export] private Window _mapWindow;
    [Export] private Button _propWindowToggle;
    [Export] private Button _mapWindowToggle;

    private bool _propWindowOpen = true;
    private bool _mapWindowOpen;


    public bool IsOpen() => _isOpen;

    private bool _isOpen;

    public static UiContentMenu Instantiate() => (UiContentMenu)GD.Load<PackedScene>("res://ui/ui_content_menu.tscn").Instantiate();

    private UiContentMenu() {
    }

    public void Open() => _isOpen = true;
    public void Close() => _isOpen = false;


    public override void _Ready() {
        if (!IsInstanceValid(_propWindow))
            GD.PrintErr(GetPath(), " _propWindow is invalid");
        if (!IsInstanceValid(_mapWindow))
            GD.PrintErr(GetPath(), " _mapWindow is invalid");
        if (!IsInstanceValid(_propWindowToggle))
            GD.PrintErr(GetPath(), " _propWindowToggle is invalid");
        if (!IsInstanceValid(_mapWindowToggle))
            GD.PrintErr(GetPath(), " _mapWindowToggle is invalid");

        _propWindowToggle.ToggleMode = true;
        _mapWindowToggle.ToggleMode = true;
        _propWindowToggle.Pressed += () => _propWindowOpen = !_propWindowOpen;
        _mapWindowToggle.Pressed += () => _mapWindowOpen = !_mapWindowOpen;
        _propWindow.CloseRequested += () => _propWindowOpen = false;
        _mapWindow.CloseRequested += () => _mapWindowOpen = false;
    }

    public override void _Process(double delta) {
        Visible = _isOpen;
        _propWindow.Visible = Visible & _propWindowOpen;
        _mapWindow.Visible = Visible & _mapWindowOpen;

        if (!Visible) return;

        _propWindowToggle.ButtonPressed = _propWindowOpen;
        _mapWindowToggle.ButtonPressed = _mapWindowOpen;
    }
}