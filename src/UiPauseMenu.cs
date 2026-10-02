using Godot;

namespace GoBox;

[GlobalClass]
public partial class UiPauseMenu {
    [Export] private Control _abilityTab;
    private double _animT = double.MaxValue;

    [ExportCategory("Map")] [Export] private Control _mapTab;
    [Export] private Camera3D _mapCamera;
    [Export] private SubViewportContainer _mapVpCont;
    [Export] private Label _mapYouAreHereLabel;
    [Export] private Label _mapAreaNameLabel;
    [Export] private Label _mapAreaDescLabel;

    [ExportCategory("Options")] [Export] private Control _optionsTab;
    [Export] private Button _quitToTitleButton;
    [Export] private Button _exitGameButton;

    private bool _isOpen;
    [ExportCategory("Tabs")] [Export] private TabContainer _tabContainer;


    public void Open() {
        _isOpen = true;
        _animT = 0d;
    }

    public void Close() {
        _isOpen = false;
        _animT = 0d;
    }

    public static UiPauseMenu Instantiate() =>
        (UiPauseMenu)GD.Load<PackedScene>("res://ui/ui_pause_menu.tscn").Instantiate();

    private UiPauseMenu() {
    }
}

public partial class UiPauseMenu : Control {
    public override void _Ready() {
        if (!IsInstanceValid(_tabContainer))
            GD.PrintErr(GetPath(), " tabcont missing");
        if (!IsInstanceValid(_mapTab))
            GD.PrintErr(GetPath(), " mapTab missing");
        if (!IsInstanceValid(_abilityTab))
            GD.PrintErr(GetPath(), " abilityTab missing");
        if (!IsInstanceValid(_optionsTab))
            GD.PrintErr(GetPath(), " optionsTab missing");


        if (!IsInstanceValid(_mapCamera))
            GD.PrintErr(GetPath(), " mapCamera missing");
        if (!IsInstanceValid(_mapVpCont))
            GD.PrintErr(GetPath(), " mapVpCont missing");
        if (!IsInstanceValid(_mapYouAreHereLabel))
            GD.PrintErr(GetPath(), " mapYouAreHereLabel missing");
        if (!IsInstanceValid(_mapAreaNameLabel))
            GD.PrintErr(GetPath(), " mapAreaNameLabel missing");
        if (!IsInstanceValid(_mapAreaDescLabel))
            GD.PrintErr(GetPath(), " mapAreaDescLabel missing");


        if (!IsInstanceValid(_quitToTitleButton))
            GD.PrintErr(GetPath(), " quitToTitleButton missing");
        if (!IsInstanceValid(_exitGameButton))
            GD.PrintErr(GetPath(), " exitGameButton missing");

        _tabContainer.CurrentTab = 0;
        ProcessMode = ProcessModeEnum.Always;


        _quitToTitleButton.Pressed += Game.QuitToTitle;
        _exitGameButton.Pressed += Game.ExitGame;
    }

    public override void _Process(double delta) {
        _animT += delta * 4d;
        AnimateInOut();

        if (!_isOpen) return;

        Show();
        var numTabs = _tabContainer.GetTabCount();

        if (Input.IsActionJustPressed("ui_next_tab") && _tabContainer.CurrentTab < numTabs - 1)
            _tabContainer.CurrentTab++;
        else if (Input.IsActionJustPressed("ui_prev_tab") && _tabContainer.CurrentTab > 0)
            _tabContainer.CurrentTab--;

        if (_mapTab.Visible) ProcessMapTab(delta);
    }
}

public partial class UiPauseMenu {
    private void AnimateInOut() {
        PivotOffset = Size / 2f;

        if (_isOpen) {
            Show();
            if (_animT <= 1d) {
                Scale = AnimatedScale((float)_animT);
                return;
            }

            Scale = Vector2.One;
        }
        else {
            if (_animT <= 1d) {
                Show();
                Scale = AnimatedScale(1f - (float)_animT);
                return;
            }

            Hide();
        }
    }

    private void ProcessMapTab(double _) {
        /*
        var stage = Game.Stage;

        mapAreaNameLabel.Text = stage.StageName;
        mapAreaDescLabel.Text = stage.StageDesc;

        var player = stage.GetPlayer();
        if (!IsInstanceValid(player)) return;
        mapYouAreHereLabel.Text = $"You are here {player.GlobalPosition}";


        var a = mapYouAreHereLabel.GlobalPosition + mapYouAreHereLabel.Size * Vector2.Right;
        var b = mapCamera.UnprojectPosition(player.GlobalPosition) + mapVpCont.GlobalPosition;

        DebugDraw.DrawLine2D(a, b, new Color(1f, 0f, 0f));
        DebugDraw.DrawPoint2D(b, new Color(1f, 0f, 0f));
        */
    }

    private static Vector2 AnimatedScale(float t) {
        return new Vector2(
            Mathf.Sqrt(Mathf.Sqrt(t)),
            Mathf.Sqrt(t)
        );
    }
}