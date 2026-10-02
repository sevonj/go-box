using System;
using System.Collections.Generic;
using Godot;

namespace GoBox.debug;

/// <summary>
///     Helper for drawing debug gizmos on screen.
///     Don't instance this, just call the methods.
/// </summary>
public partial class DebugDraw : Node2D {
    private const string FontPath = "res://assets/font/font_debug.tres";
    private static DebugDraw _instance;
    private static Font _font;

    private bool queueDarkBackground;

    private readonly List<QueueLine> lineQueue = [];
    private readonly List<QueuePoint> pointQueue = [];
    private readonly List<QueueCircle> circleQueue = [];
    private readonly List<QueueArc> arcQueue = [];
    private readonly List<QueueText> textQueue = [];

    private readonly List<QueueLine> pLineQueue = [];
    private readonly List<QueuePoint> pPointQueue = [];
    private readonly List<QueueCircle> pCircleQueue = [];
    private readonly List<QueueArc> pArcQueue = [];
    private readonly List<QueueText> pTextQueue = [];

    private readonly List<QueueSphere3D> sphere3dQueue = [];
    private readonly List<QueueCapsule3D> capsule3dQueue = [];
    private readonly List<QueueText3D> text3dQueue = [];

    private readonly List<QueueSphere3D> pSphere3dQueue = [];
    private readonly List<QueueCapsule3D> pCapsule3dQueue = [];
    private readonly List<QueueText3D> pText3dQueue = [];

    // ---

    public static Vector2 GetTextSize(string text) {
        return !IsInstanceValid(_font) ? Vector2.Zero : _font.GetMultilineStringSize(text);
    }

    private DebugDraw() {
        // ProcessPriority = ProcessPriorities.DebugDraw;
        // ProcessPhysicsPriority = ProcessPriorities.PDebugDraw;
    }


    ~DebugDraw() {
        _instance = null;
    }

    private static DebugDraw GetInstance() {
        if (!IsInstanceValid(_instance)) {
            _instance = new DebugDraw();
            var sceneTree = (SceneTree)Engine.GetMainLoop();
            sceneTree.CurrentScene.AddChild(_instance);
        }

        return _instance;
    }

    /// <summary>
    ///     Draw dark tint overlay this frame. For debug menu, etc.
    /// </summary>
    public static void DrawDarkBackground() {
        GetInstance().queueDarkBackground = true;
    }

    /// <summary>
    ///     Draw a line in world
    /// </summary>
    /// <param name="a">3D point in world space</param>
    /// <param name="b">3D point in world space</param>
    /// <param name="color"></param>
    /// <param name="phys">Drawn shapes will clear on physics tick instead of immediately on next frame. Use if you're calling on phys process.</param>
    public static void DrawLine3D(Vector3 a, Vector3 b, Color color, bool phys = false) {
        var camera = GetInstance().GetViewport().GetCamera3D();
        if (camera.IsPositionBehind(a) || camera.IsPositionBehind(b))
            return;

        var queue = phys ? GetInstance().pLineQueue : GetInstance().lineQueue;
        queue.Add(new QueueLine {
            A = camera.UnprojectPosition(a),
            B = camera.UnprojectPosition(b),
            Color = color
        });
    }

    /// <summary>
    ///     Draw a line on screen
    /// </summary>
    /// <param name="a">Screen-space start position</param>
    /// <param name="b">Screen-space end position</param>
    /// <param name="color"></param>
    /// <param name="phys">Drawn shapes will clear on physics tick instead of immediately on next frame. Use if you're calling on phys process.</param>
    public static void DrawLine2D(Vector2 a, Vector2 b, Color color, bool phys = false) {
        var queue = phys ? GetInstance().pLineQueue : GetInstance().lineQueue;
        queue.Add(new QueueLine {
            A = a,
            B = b,
            Color = color
        });
    }

    /// <summary>
    ///     Draw a point world
    /// </summary>
    /// <param name="origin">3D point in world space</param>
    /// <param name="color"></param>
    /// <param name="phys">Drawn shapes will clear on physics tick instead of immediately on next frame. Use if you're calling on phys process.</param>
    public static void DrawPoint3D(Vector3 origin, Color color, bool phys = false) {
        var camera = GetInstance().GetViewport().GetCamera3D();
        if (camera.IsPositionBehind(origin))
            return;

        var queue = phys ? GetInstance().pPointQueue : GetInstance().pointQueue;
        queue.Add(new QueuePoint {
            Position = camera.UnprojectPosition(origin),
            Color = color
        });
    }

    /// <summary>
    ///     Draw a point on screen
    /// </summary>
    /// <param name="position">Screen-space position</param>
    /// <param name="color"></param>
    /// <param name="phys">Drawn shapes will clear on physics tick instead of immediately on next frame. Use if you're calling on phys process.</param>
    public static void DrawPoint2D(Vector2 position, Color color, bool phys = false) {
        var queue = phys ? GetInstance().pPointQueue : GetInstance().pointQueue;
        queue.Add(new QueuePoint {
            Position = position,
            Color = color
        });
    }

    /// <summary>
    ///     Draw a sphere in 3D world space
    /// </summary>
    /// <param name="origin">3D point in world space</param>
    /// <param name="r">3D radius</param>
    /// <param name="color"></param>
    /// <param name="phys">Drawn shapes will clear on physics tick instead of immediately on next frame. Use if you're calling on phys process.</param>
    public static void DrawSphere3D(Vector3 origin, float r, Color color, bool phys = false) {
        var queue = phys ? GetInstance().pSphere3dQueue : GetInstance().sphere3dQueue;
        queue.Add(new QueueSphere3D() {
            Position = origin,
            Radius = r,
            Color = color
        });
    }

    /// <summary>
    ///     Draw a circle in 2D screen space
    /// </summary>
    /// <param name="position">Screen-space position</param>
    /// <param name="r">Screen-space radius</param>
    /// <param name="color"></param>
    /// <param name="phys">Drawn shapes will clear on physics tick instead of immediately on next frame. Use if you're calling on phys process.</param>
    public static void DrawCircle2D(Vector2 position, float r, Color color, bool phys = false) {
        var queue = phys ? GetInstance().pCircleQueue : GetInstance().circleQueue;
        queue.Add(new QueueCircle {
            Position = position,
            R = r,
            Color = color
        });
    }

    /// <summary>
    ///     Draw an arc in 2D screen space
    /// </summary>
    /// <param name="position">Screen-space position</param>
    /// <param name="r">Screen-space radius</param>
    /// <param name="startAngle"></param>
    /// <param name="endAngle"></param>
    /// <param name="color"></param>
    /// <param name="phys">Drawn shapes will clear on physics tick instead of immediately on next frame. Use if you're calling on phys process.</param>
    public static void DrawArc2D(Vector2 position, float r, float startAngle, float endAngle, Color color,
        bool phys = false) {
        var queue = phys ? GetInstance().pArcQueue : GetInstance().arcQueue;
        queue.Add(new QueueArc {
            Position = position,
            R = r,
            StartAngle = startAngle,
            EndAngle = endAngle,
            Color = color
        });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="shape"></param>
    /// <param name="xform"></param>
    /// <param name="color"></param>
    /// <param name="phys">Drawn shapes will clear on physics tick instead of immediately on next frame. Use if you're calling on phys process.</param>
    public static void DrawCapsule3D(CapsuleShape3D shape, Transform3D xform, Color color, bool phys = false) {
        var actualRadius = Mathf.Min(shape.Radius, shape.Height / 2f);
        var queue = phys ? GetInstance().pCapsule3dQueue : GetInstance().capsule3dQueue;
        queue.Add(new QueueCapsule3D {
            PosA = xform * (Vector3.Up * (shape.Height / 2f - actualRadius)),
            PosB = xform * -(Vector3.Up * (shape.Height / 2f - actualRadius)),
            Radius = shape.Radius,
            Color = color
        });
    }

    /// <summary>
    ///     Draw a string in 2D screen space
    /// </summary>
    /// <param name="text">The string</param>
    /// <param name="position">Screen-space position</param>
    /// <param name="color"></param>
    /// <param name="phys">Drawn shapes will clear on physics tick instead of immediately on next frame. Use if you're calling on phys process.</param>
    public static void DrawText2D(string text, Vector2 position, Color color, bool phys = false) {
        var queue = phys ? GetInstance().pTextQueue : GetInstance().textQueue;
        queue.Add(new QueueText {
            Text = text,
            Position = position,
            Color = color
        });
    }

    /// <summary>
    ///     Draw a string in 3D world space
    /// </summary>
    /// <param name="text">The string</param>
    /// <param name="origin">Screen-space position</param>
    /// <param name="color"></param>
    /// <param name="phys">Drawn shapes will clear on physics tick instead of immediately on next frame. Use if you're calling on phys process.</param>
    public static void DrawText3D(string text, Vector3 origin, Color color, bool phys = false) {
        var queue = phys ? GetInstance().pText3dQueue : GetInstance().text3dQueue;
        queue.Add(new QueueText3D {
            Text = text,
            Position = origin,
            Color = color
        });
    }


// ---
    public override void _Ready() {
        _font = (Font)ResourceLoader.Load(FontPath, "Font");
    }

    public override void _Process(double delta) {
        QueueRedraw();
    }

    public override void _PhysicsProcess(double _) {
        ClearPhysQueues();
    }

    public override void _Draw() {
        if (!IsInstanceValid(GetInstance().GetViewport().GetCamera3D())) {
            DrawString(_font, new Vector2(2f, 200f), "DebugDraw: No Camera!");
            ClearQueues();
            return;
        }

        if (queueDarkBackground) {
            var vp = GetViewport();
            DrawRect(vp.GetVisibleRect(), new Color(0f, 0f, 0f, 0.7f));
        }

        foreach (var sphere in sphere3dQueue) {
            BreakSphere3D(sphere);
        }

        foreach (var cap in capsule3dQueue) {
            BreakCapsule3D(cap);
        }

        foreach (var text in text3dQueue) {
            BreakText3D(text);
        }

        foreach (var sphere in pSphere3dQueue) {
            BreakSphere3D(sphere);
        }

        foreach (var cap in pCapsule3dQueue) {
            BreakCapsule3D(cap);
        }

        foreach (var text in pText3dQueue) {
            BreakText3D(text);
        }


        foreach (var l in lineQueue) {
            DrawLine(l.A, l.B, l.Color);
        }

        foreach (var p in pointQueue) {
            Rect2 rect = new(new Vector2(p.Position.X - 4, p.Position.Y - 4), new Vector2(8, 8));
            DrawRect(rect, p.Color, false);
        }

        foreach (var c in circleQueue) {
            DrawCircle(c.Position, c.R, c.Color, false);
        }

        foreach (var a in arcQueue) {
            var pointcount = Mathf.Abs((a.EndAngle - a.StartAngle) / Mathf.Pi * 360f);
            DrawArc(a.Position, a.R, a.StartAngle, a.EndAngle,
                (int)Mathf.Max(2, pointcount),
                a.Color);
        }

        foreach (var t in textQueue) {
            DrawMultilineString(_font, t.Position, t.Text, modulate: t.Color);
            DrawMultilineStringOutline(_font, t.Position, t.Text, modulate: Colors.Black);
        }


        foreach (var l in pLineQueue) {
            DrawLine(l.A, l.B, l.Color);
        }

        foreach (var p in pPointQueue) {
            Rect2 rect = new(new Vector2(p.Position.X - 4, p.Position.Y - 4), new Vector2(8, 8));
            DrawRect(rect, p.Color, false);
        }

        foreach (var c in pCircleQueue) {
            DrawCircle(c.Position, c.R, c.Color, false);
        }

        foreach (var a in pArcQueue) {
            var pointcount = Mathf.Abs((a.EndAngle - a.StartAngle) / Mathf.Pi * 360f);
            DrawArc(a.Position, a.R, a.StartAngle, a.EndAngle,
                (int)Mathf.Max(2, pointcount),
                a.Color);
        }

        foreach (var t in pTextQueue) {
            DrawMultilineString(_font, t.Position, t.Text, modulate: t.Color);
            DrawMultilineStringOutline(_font, t.Position, t.Text, modulate: Colors.Black);
        }

        ClearQueues();
    }

    // 3d to 2d
    private void BreakSphere3D(QueueSphere3D shape) {
        var camera = GetInstance().GetViewport().GetCamera3D();
        if (camera.IsPositionBehind(shape.Position) ||
            Mathf.IsZeroApprox(shape.Position.DistanceTo(camera.GlobalPosition))) return;

        var rSample = camera.UnprojectPosition(shape.Position + camera.GlobalTransform.Basis.Column0 * shape.Radius);
        var position = camera.UnprojectPosition(shape.Position);

        GetInstance().circleQueue.Add(new QueueCircle {
            Position = position,
            R = position.DistanceTo(rSample),
            Color = shape.Color
        });
    }

    // 3d to 2d
    private void BreakCapsule3D(QueueCapsule3D shape) {
        var camera = GetInstance().GetViewport().GetCamera3D();
        var camPos = camera.GlobalPosition;
        var camXform = camera.GlobalTransform;

        Vector2 a;
        float rA;
        {
            var pos = shape.PosA;
            if (camera.IsPositionBehind(pos) || pos.IsEqualApprox(camPos)) return;
            var rSample = camera.UnprojectPosition(pos + camXform.Basis.Column0 * shape.Radius);

            a = camera.UnprojectPosition(pos);
            rA = a.DistanceTo(rSample);
        }
        Vector2 b;
        float rB;
        {
            var pos = shape.PosB;
            if (camera.IsPositionBehind(pos) || pos.IsEqualApprox(camPos)) return;
            var rSample = camera.UnprojectPosition(pos + camXform.Basis.Column0 * shape.Radius);

            b = camera.UnprojectPosition(pos);
            rB = b.DistanceTo(rSample);
        }

        var ninetydeg = Mathf.Pi / 2f;
        var angle = (a - b).Angle() + ninetydeg;

        var angleOff = Mathf.Asin((rA - rB) / a.DistanceTo(b));
        if (Mathf.IsNaN(angleOff)) {
            if (rA > rB) DrawCircle2D(a, rA, shape.Color);
            else DrawCircle2D(b, rB, shape.Color);
            return;
        }

        DrawArc2D(a, rA, angle - ninetydeg * 2f - angleOff, angle + angleOff, shape.Color);
        DrawArc2D(b, rB, angle + angleOff, angle + ninetydeg * 2f - angleOff, shape.Color);

        var aL = a + new Vector2(0f, -rA).Rotated(angleOff + ninetydeg + angle);
        var bL = b + new Vector2(0f, -rB).Rotated(angleOff + ninetydeg + angle);
        var aR = a + new Vector2(0f, -rA).Rotated(-angleOff - ninetydeg + angle);
        var bR = b + new Vector2(0f, -rB).Rotated(-angleOff - ninetydeg + angle);

        DrawLine2D(aL, bL, shape.Color);
        DrawLine2D(aR, bR, shape.Color);
    }


    // 3d to 2d
    private void BreakText3D(QueueText3D text) {
        var camera = GetInstance().GetViewport().GetCamera3D();
        if (camera.IsPositionBehind(text.Position) ||
            Mathf.IsZeroApprox(text.Position.DistanceTo(camera.GlobalPosition))) return;

        var position = camera.UnprojectPosition(text.Position);

        GetInstance().textQueue.Add(new QueueText {
            Text = text.Text,
            Position = position,
            Color = text.Color
        });
    }

    private void ClearQueues() {
        queueDarkBackground = false;

        pointQueue.Clear();
        lineQueue.Clear();
        circleQueue.Clear();
        arcQueue.Clear();
        textQueue.Clear();

        sphere3dQueue.Clear();
        capsule3dQueue.Clear();
        text3dQueue.Clear();
    }

    private void ClearPhysQueues() {
        pPointQueue.Clear();
        pLineQueue.Clear();
        pCircleQueue.Clear();
        pArcQueue.Clear();
        pTextQueue.Clear();

        pSphere3dQueue.Clear();
        pCapsule3dQueue.Clear();
        pText3dQueue.Clear();
    }

    private struct QueuePoint {
        public Vector2 Position { get; init; }
        public Color Color { get; init; }
    }

    private struct QueueLine {
        public Vector2 A { get; init; }
        public Vector2 B { get; init; }
        public Color Color { get; init; }
    }

    private struct QueueCircle {
        public Vector2 Position { get; init; }
        public float R { get; init; }
        public Color Color { get; init; }
    }

    private struct QueueArc {
        public Vector2 Position { get; init; }
        public float R { get; init; }
        public float StartAngle { get; init; }
        public float EndAngle { get; init; }
        public Color Color { get; init; }
    }

    private struct QueueText {
        public string Text { get; init; }
        public Vector2 Position { get; init; }
        public Color Color { get; init; }
    }

    private struct QueueSphere3D {
        public Vector3 Position { get; init; }
        public float Radius { get; init; }
        public Color Color { get; init; }
    }

    private struct QueueShpere3D {
        public Vector3 Position { get; init; }
        public float Radius { get; init; }
        public Color Color { get; init; }
    }

    private struct QueueCapsule3D {
        public Vector3 PosA { get; init; }
        public Vector3 PosB { get; init; }
        public float Radius { get; init; }
        public Color Color { get; init; }
    }

    private struct QueueText3D {
        public string Text { get; init; }
        public Vector3 Position { get; init; }
        public Color Color { get; init; }
    }
}