/*
using System;
using Godot;

namespace sharp.debug;

[GlobalClass]
public partial class DebugMenu : Node {
    public static DebugMenu Instance {
        get {
            if (!IsInstanceValid(_instance)) {
                _instance = new DebugMenu();
                var sceneTree = (SceneTree)Engine.GetMainLoop();
                sceneTree.CurrentScene.AddChild(_instance);
            }

            return _instance;
        }
        private set => _instance = value;
    }

    public bool Open { get; set; }
    public bool Advance { get; private set; }

    private static DebugMenu _instance;

    private DebugTab[] tabs = [
        TabCommon(), TabDraw()
    ];

    private int tabIndex;
    private bool togglePreviouslyPressed;
    private bool previouslyClosed = true;
    private bool deferredAdvance;

    private DebugMenu() {
        ProcessPriority = ProcessPriorities.DebugMenu;
    }


    ~DebugMenu() {
        Instance = null;
    }

    public override void _Process(double delta) {
        Advance = deferredAdvance;
        deferredAdvance = false;

        var togglePressed = Input.IsJoyButtonPressed(0, JoyButton.Back) &&
                            Input.IsJoyButtonPressed(0, JoyButton.RightShoulder);
        if (togglePressed && !togglePreviouslyPressed) {
            if (!Open) Open = true;
            // Deferred to avoid triggering ActionJustPressed elsewhere
            else Callable.From(() => Open = false).CallDeferred();
        }

        togglePreviouslyPressed = togglePressed;
        var acceptInput = !previouslyClosed && !togglePressed;
        previouslyClosed = !Open;
        if (!Open) return;

        DebugDraw.DrawDarkBackground();
        var pos = new Vector2(200f, 180f);
        DebugDraw.DrawText2D("Super Secrit Hacker Menu", pos, Colors.Gray);
        pos.Y += 20f;

        if (Input.IsActionJustPressed("ui_prev_tab") && acceptInput) tabIndex -= 1;
        if (Input.IsActionJustPressed("ui_next_tab") && acceptInput) tabIndex += 1;

        tabIndex = Math.Clamp(tabIndex, 0, tabs.Length - 1);
        ref var tab = ref tabs[tabIndex];

        DebugDraw.DrawText2D("[L1] <", pos, Colors.Gray);
        pos.X += DebugDraw.GetTextSize("[L1] <").X + 8f;

        for (var i = 0; i < tabs.Length; i++) {
            var label = tabs[i].Name;
            var col = tabIndex == i ? Colors.Goldenrod : Colors.White;
            DebugDraw.DrawText2D(label, pos, col);
            pos.X += DebugDraw.GetTextSize(label).X + 8f;
        }

        DebugDraw.DrawText2D("> [R1]", pos, Colors.Gray);

        if (!acceptInput) return;

        BuildTab(delta, ref tab);
    }

    private void BuildTab(double delta, ref DebugTab tab) {
        if (Input.IsActionJustPressed("ui_up")) tab.HoverIndex -= 1;
        if (Input.IsActionJustPressed("ui_down")) tab.HoverIndex += 1;
        tab.HoverIndex = Math.Clamp(tab.HoverIndex, 0, tab.Commands.Length - 1);

        var pos = new Vector2(200f, 220f);
        for (var i = 0; i < tab.Commands.Length; i++) {
            pos.Y += 20f;
            DrawCommand(tab.Commands[i], pos, i == tab.HoverIndex);
        }

        var cmd = tab.Commands[tab.HoverIndex];

        if (Input.IsActionJustPressed("ui_accept") && cmd.IsActivatable()) {
            cmd.Activate();
            deferredAdvance = true;
        }

        if (cmd.GetValue == null) return;

        switch (cmd.GetValue().VariantType) {
            case Variant.Type.Bool: {
                if (Input.IsActionJustPressed("ui_accept")) cmd.SetValue(!cmd.GetValue().AsBool());
                break;
            }
            case Variant.Type.Float: {
                var mult = Input.IsActionPressed("ui_accept") ? 10f : 1f;
                if (Input.IsActionPressed("ui_left")) cmd.SetValue(cmd.GetValue().AsSingle() - delta * mult);
                if (Input.IsActionPressed("ui_right")) cmd.SetValue(cmd.GetValue().AsSingle() + delta * mult);
                break;
            }
        }
    }

    private static void DrawCommand(DebugCommand cmd, Vector2 pos, bool hover) {
        var press = Input.IsActionPressed("ui_accept");
        var col = hover ? press ? Colors.DarkGoldenrod : Colors.Goldenrod : Colors.White;
        string label;
        var cursor = hover ? '█' : ' ';
        if (cmd.GetValue != null) {
            switch (cmd.GetValue().VariantType) {
                case Variant.Type.Nil:
                    label = $"{cursor} [wtf: value != null but Type is Nil ???]";
                    break;
                case Variant.Type.Bool: {
                    var val = cmd.GetValue().AsBool() ? "[x]" : "[ ]";
                    label = $"{cursor} {cmd.Name} {val}";
                    break;
                }
                case Variant.Type.Float: {
                    var unit = cmd.Unit != null ? $" {cmd.Unit}" : "";
                    var val = cmd.GetValue().AsSingle().ToString("0.00");
                    label = $"{cursor} {cmd.Name} {(hover ? "<" : "")}{val}{unit}{(hover ? ">" : "")}";
                    break;
                }
                default:
                    label = $"{cursor} [UNHANDLED TYPE {cmd.GetValue().VariantType}]";
                    break;
            }
        }
        else {
            label = $"{cursor} {cmd.Name}";
        }

        DebugDraw.DrawText2D(label, pos, col);
    }

    private struct DebugTab {
        public string Name { get; init; }
        public DebugCommand[] Commands { get; init; }
        public int HoverIndex { get; set; }
    }

    private readonly struct DebugCommand {
        public string Name { get; init; }
        public string Unit { get; init; }
        public Func<Variant> GetValue { get; init; }
        public Action<Variant> SetValue { get; init; }
        public Action<DebugCommand> Action { private get; init; }

        public bool IsActivatable() {
            return Action != null;
        }

        public void Activate() {
            Action?.Invoke(this);
        }
    }
}
*/