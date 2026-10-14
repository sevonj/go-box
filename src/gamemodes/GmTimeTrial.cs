using Godot;

namespace GoBox.gamemodes;

[GlobalClass]
public partial class GmTimeTrial : Gamemode {
    private enum State {
        NotStarted,
        Running,
        Ended,
        FreeRoam
    }

    private UiTimerDisplay _uiTimer = UiTimerDisplay.Instantiate();

    private double _timer;
    private State _state = State.NotStarted;

    public void Start() {
        if (_state != State.NotStarted) return;
        _state = State.Running;
        LocalVars.DisablePlayerControl = false;
    }

    public void Finish() {
        if (_state != State.Running) return;
        _state = State.Ended;
        LocalVars.DisablePlayerControl = true;
        GD.Print("Finished");
        CreateResultsPrompt();
    }

    public void EnterFreeRoam() {
        if (_state != State.Ended) return;
        _state = State.FreeRoam;
        LocalVars.DisablePlayerControl = false;
        GD.Print("FreeRoam");
        AllowContentMenu = true;
    }

    public GmTimeTrial() {
        AllowContentMenu = false;
    }

    // --- Node

    public override void _Ready() {
        Name = "GmTimeTrial";

        var finishLinesExist = false;
        foreach (var node in GetTree().GetNodesInGroup(nameof(FuncFinishLine))) {
            if (node is not FuncFinishLine finishLine) {
                return;
            }

            finishLinesExist = true;
            finishLine.PlayerEntered += _ => Finish();
        }

        if (!finishLinesExist) {
            GD.PrintErr("Couldn't find any finish lines");
        }

        Game.World.AddUi(_uiTimer);
        Game.World.SpawnPlayer();
        LocalVars.DisablePlayerControl = true;

        CreateStartupPrompt();
    }

    public override void _Process(double delta) {
        _uiTimer.SetTime(_timer);

        switch (_state) {
            case State.NotStarted: {
                return;
            }
            case State.Running: {
                _timer += delta;
                return;
            }
            case State.Ended: {
                _uiTimer.Hide();
                return;
            }
            case State.FreeRoam: {
                return;
            }
        }
    }

    // --- Private

    private void CreateStartupPrompt() {
        var prompt = UiPressStartPrompt.Instantiate();
        prompt.Pressed += Start;
        prompt.Pressed += prompt.QueueFree;
        Game.World.AddUi(prompt);
    }

    private void CreateResultsPrompt() {
        var prompt = UiResultsScreen.Instantiate();
        prompt.Closed += () => LocalVars.DisablePlayerControl = false;
        Game.World.AddUi(prompt);
    }
}