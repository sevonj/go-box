using System.Collections.Generic;
using System.Linq;
using Godot;

namespace GoBox;

public static class Game {
    public static World World { get; private set; }

    public static List<string> Stages {
        get {
            if (_stages == null) FindStages();
            return _stages;
        }
    }

    private static List<string> _stages;
    private const string ResetScenePath = "res://assets/ui/ui_main_menu.tscn";


    public static void QuitToTitle() {
        World = null;
        var tree = GetTree();
        tree.Paused = false;
        Input.SetMouseMode(Input.MouseModeEnum.Visible);
        tree.ChangeSceneToFile(ResetScenePath);
    }

    public static void ExitGame() {
        GetTree().Quit();
    }

    public static void LoadStage(content.StageDef stageDef, bool debugColls = false) {
        World = StageLoader.Load(stageDef, debugColls);

        if (World == null) {
            QuitToTitle();
            return;
        }

        var tree = GetTree();
        tree.ChangeSceneToNode(World);
        tree.Paused = false;
    }

    private static SceneTree GetTree() {
        return (SceneTree)Engine.GetMainLoop();
    }

    private static void FindStages() {
        _stages = [];
        _stages.AddRange(DirAccess.GetFilesAt("res://scenes").Select(filename => "res://scenes/" + filename));
    }
}