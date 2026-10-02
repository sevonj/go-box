using Godot;
using Godot.Collections;

namespace GoBox;

[GlobalClass]
public partial class World {
    private Node _envCont;
    private Node _staticCont;
    private Node _actorCont;
    private Node _uiCont;

    private Array<InfoPlayerStart> _playerStarts = [];

    private UiPauseMenu _uiPauseMenu = UiPauseMenu.Instantiate();
    private UiContentMenu _uiContentMenu = UiContentMenu.Instantiate();

    private Player _player;
    private System.Collections.Generic.Dictionary<ulong, PhysProp> _physProps = [];

    // --- State
    public bool Paused { get; private set; }
    private Vector2 _lastCursorPos;

    public bool IsMenuOpen() => Paused || _uiContentMenu.IsOpen();
}

public partial class World {
    public void AddActor(Node3D node) {
        if (!IsInstanceValid(node)) {
            GD.PrintErr("World::AddActor(): Attempted to add invalid node");
            return;
        }

        if (IsInstanceValid(node.GetParent())) {
            GD.PushWarning("World::AddActor(): Node is already parented.");
            return;
        }

        _actorCont.AddChild(node);
    }

    public void AddUi(Control control) {
        if (!IsInstanceValid(control)) {
            GD.PrintErr("World::AddUi(): Attempted to add invalid node");
            return;
        }

        if (IsInstanceValid(control.GetParent())) {
            GD.PushWarning("World::AddActor(): Node is already parented.");
            return;
        }

        _uiCont.AddChild(control);
    }

    public void RegisterPlayerStart(InfoPlayerStart playerStart) {
        _playerStarts.Add(playerStart);
    }

    public void RegisterPhysProp(PhysProp physProp) {
        if (!IsInstanceValid(physProp)) return;
        var key = physProp.GetInstanceId();
        if (_physProps.ContainsKey(key)) return;
        _physProps.Add(key, physProp);
    }

    public void ClearPhysProps() {
        foreach (var (_, pp) in _physProps) pp.QueueFree();
        _physProps.Clear();
    }
}

public partial class World : Node {
    public override void _Ready() {
        // -- Setup
        _envCont = GetNodeOrNull("Env");
        _staticCont = GetNodeOrNull("Static");
        _actorCont = GetNodeOrNull("Actors");
        _uiCont = GetNodeOrNull("Ui");

        if (!IsInstanceValid(_envCont)) {
            _envCont = new Node();
            _envCont.Name = "Env";
            AddChild(_envCont);
        }

        if (!IsInstanceValid(_staticCont)) {
            _staticCont = new Node();
            _staticCont.Name = "Static";
            AddChild(_staticCont);
        }

        if (!IsInstanceValid(_actorCont)) {
            _actorCont = new Node();
            _actorCont.Name = "Actors";
            AddChild(_actorCont);
        }

        if (!IsInstanceValid(_uiCont)) {
            _uiCont = new Node();
            _uiCont.Name = "Ui";
            AddChild(_uiCont);
        }

        ProcessMode = ProcessModeEnum.Always;
        _envCont.ProcessMode = ProcessModeEnum.Pausable;
        _staticCont.ProcessMode = ProcessModeEnum.Pausable;
        _actorCont.ProcessMode = ProcessModeEnum.Pausable;
        _uiCont.ProcessMode = ProcessModeEnum.Always;

        _uiCont.AddChild(_uiPauseMenu);
        _uiCont.AddChild(_uiContentMenu);

        // -- Start Game
        SpawnPlayer();
    }

    public override void _Process(double delta) {
        if (Input.MouseMode == Input.MouseModeEnum.Visible) {
            _lastCursorPos = GetViewport().GetMousePosition();
        }

        /*if (DebugMenu.Instance.Open && !DebugMenu.Instance.Advance) {
            pauseMenu.ProcessMode = ProcessModeEnum.Disabled;
            gameOver.ProcessMode = ProcessModeEnum.Disabled;
            GetTree().Paused = true;
            return;
        }*/

        // pauseMenu.ProcessMode = ProcessModeEnum.Always;
        // gameOver.ProcessMode = ProcessModeEnum.Always;


        if (Input.IsActionJustPressed("pause")) {
            Paused = !Paused;
            if (Paused)
                _uiPauseMenu.Open();
            else
                _uiPauseMenu.Close();
        }

        if (!Paused && Input.IsActionJustPressed("ui_content_menu")) {
            if (!_uiContentMenu.IsOpen())
                _uiContentMenu.Open();
            else
                _uiContentMenu.Close();
        }


        GetTree().Paused = Paused;
        var captureMouse = !IsMenuOpen();
        var wasCaptured = Input.MouseMode == Input.MouseModeEnum.Captured;
        Input.SetMouseMode(captureMouse ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible);

        if (Input.MouseMode == Input.MouseModeEnum.Visible && wasCaptured) {
            // Restore cursor pos
            GetViewport().WarpMouse(_lastCursorPos);
        }
    }

    private void SpawnPlayer() {
        _player?.QueueFree();

        var playa = Player.Instantiate();
        _player = playa;

        AddActor(playa);

        if (_playerStarts.Count > 0) {
            var playerStart = _playerStarts[0];
            playa.GlobalPosition = playerStart.GlobalPosition;
            playa.GlobalRotation = playerStart.GlobalRotation;
        }
        else {
            GD.PushWarning("World: No player spawns! Using default coords instead.");
        }
    }
}