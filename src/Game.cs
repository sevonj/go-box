using System.Collections.Generic;
using System.Linq;
using GoBox.content;
using GoBox.gamemodes;
using GoBox.interfaces;
using GoBox.types;
using Godot;

namespace GoBox;

public static class Game {
    public static World World { get; private set; }
    public static Gamemode Gamemode { get; private set; }

    public static List<string> Stages {
        get {
            if (_stages == null) FindStages();
            return _stages;
        }
    }

    private static List<string> _stages;
    private const string ResetScenePath = "res://scenes/main_menu.tscn";


    public static void QuitToTitle() {
        StageExitCleanup();
        GetTree().ChangeSceneToFile(ResetScenePath);
    }

    public static void ExitGame() {
        GetTree().Quit();
    }

    public static void ReloadStage() {
        if (LocalVars.CurrentStageDef == null) {
            GD.PrintErr("Can't reload stage. Current stage def is null");
            QuitToTitle();
            return;
        }

        Gamemode gamemode;
        switch (LocalVars.CurrentGamemode) {
            case nameof(Gamemode): {
                gamemode = new Gamemode();
                break;
            }
            case nameof(GmTimeTrial): {
                gamemode = new GmTimeTrial();
                break;
            }
            case null: {
                GD.PrintErr("Can't reload stage. Current gamemode is null");
                QuitToTitle();
                return;
            }
            default: {
                GD.PrintErr($"Can't reload stage. Unknown gamemode '{LocalVars.CurrentGamemode}'");
                QuitToTitle();
                return;
            }
        }

        LoadStage(LocalVars.CurrentStageDef, gamemode);
    }

    public static void StageExitCleanup() {
        Gamemode = null;
        World = null;
        LocalVars.CurrentStageDef = null;
        LocalVars.CurrentGamemode = null;
        Input.SetMouseMode(Input.MouseModeEnum.Visible);
        GetTree().Paused = false;
    }

    public static void LoadStage(StageDef stageDef, Gamemode gamemode, bool debugColls = false) {
        StageExitCleanup();
        World = StageLoader.Load(stageDef, debugColls);

        if (World == null) {
            QuitToTitle();
            return;
        }

        LocalVars.CurrentStageDef = stageDef;
        LocalVars.CurrentGamemode = gamemode.GetType().Name;
        Gamemode = gamemode;
        GetTree().ChangeSceneToNode(World);
        World.AddChild(Gamemode);
    }

    public static string CreateSaveState() {
        if (World == null) {
            GD.PushError("Attempted to save state when World is null");
            return null;
        }

        var saveState = new SaveState {
            StageId = LocalVars.CurrentStageDef.Id
        };

        foreach (var actor in World.GetActors()) {
            if (actor is ISaveable saveable) {
                var id = saveable.GetId();
                if (string.IsNullOrEmpty(id)) continue;
                saveState.ActorIds.Add(id);
                saveState.ActorStates.Add(saveable.SaveState());
            }
        }

        return saveState.ToJson();
    }

    public static void LoadSaveState(SaveState saveState) {
        GD.Print("loading savestate");
        if (!Content.StageDefs.TryGetValue(saveState.StageId, out var stageDef)) {
            GD.PushError("Load save state: Stage not found");
            return;
        }

        var gamemode = new Gamemode();
        LoadStage(stageDef, gamemode);
        World.SaveState = saveState;
    }

    // --- Private

    private static SceneTree GetTree() {
        return (SceneTree)Engine.GetMainLoop();
    }

    private static void FindStages() {
        _stages = [];
        _stages.AddRange(DirAccess.GetFilesAt("res://scenes").Select(filename => "res://scenes/" + filename));
    }
}