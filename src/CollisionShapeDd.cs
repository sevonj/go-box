using System.Diagnostics;
using GoBox.debug;
using Godot;

namespace GoBox;

/// <summary>
/// Same as CollisionShape3D but with debug draw support
/// </summary>
[GlobalClass]
public partial class CollisionShapeDd : CollisionShape3D {
    [Export] public bool DrawingEnabled { get; set; }
    [Export] public bool DrawWhenDisabled { get; set; }
    [Export] public ColliderType Purpose = ColliderType.Generic;
    [Export] public string Label;

    public enum ColliderType {
        Generic,
        Character,
        HurtBox,
        Interact,
        Trigger,
        Breakable,
    }

    private Color GetColor() {
        var col = Purpose switch {
            ColliderType.Character => Colors.OliveDrab,
            ColliderType.HurtBox => Colors.Yellow,
            ColliderType.Trigger => Colors.Orange,
            ColliderType.Interact => Colors.CadetBlue,
            ColliderType.Breakable => Colors.LightSalmon,
            _ => Colors.LightSteelBlue
        };

        if (Disabled) col.A = 0.5f;
        return col;
    }


    public override void _Process(double delta) {
        //if (Purpose switch {
        //        // ColliderType.Character => DrawHitboxes,
        //        ColliderType.Trigger => !Debug.DrawTriggers,
        //        ColliderType.Interact => !Debug.DrawInteracts,
        //        ColliderType.HurtBox => !Debug.DrawHurtboxes,
        //        ColliderType.Breakable => !Debug.DrawBreakables,
        //        _ => !Debug.DrawHitboxes,
        //    }) {
        //    return;
        //}

        if (Disabled && !DrawWhenDisabled) return;

        var camera = GetViewport().GetCamera3D();
        if (!IsInstanceValid(camera)) return;

        // if (GlobalPosition.DistanceTo(camera.GlobalPosition) > Debug.DebugDrawDistance) return;

        var color = GetColor();

        if (Label is { Length: > 0 }) {
            DebugDraw.DrawPoint3D(GlobalPosition, color);
            DebugDraw.DrawText3D(Label, GlobalPosition, color);
        }

        switch (Shape) {
            case BoxShape3D box: {
                var min = -box.Size / 2f;
                var max = box.Size / 2f;

                var a = GlobalTransform * new Vector3(min.X, min.Y, min.Z);
                var b = GlobalTransform * new Vector3(min.X, min.Y, max.Z);
                var c = GlobalTransform * new Vector3(min.X, max.Y, max.Z);
                var d = GlobalTransform * new Vector3(min.X, max.Y, min.Z);
                var e = GlobalTransform * new Vector3(max.X, min.Y, min.Z);
                var f = GlobalTransform * new Vector3(max.X, min.Y, max.Z);
                var g = GlobalTransform * new Vector3(max.X, max.Y, max.Z);
                var h = GlobalTransform * new Vector3(max.X, max.Y, min.Z);

                DebugDraw.DrawLine3D(a, b, color);
                DebugDraw.DrawLine3D(b, c, color);
                DebugDraw.DrawLine3D(c, d, color);
                DebugDraw.DrawLine3D(d, a, color);

                DebugDraw.DrawLine3D(e, f, color);
                DebugDraw.DrawLine3D(f, g, color);
                DebugDraw.DrawLine3D(g, h, color);
                DebugDraw.DrawLine3D(h, e, color);

                DebugDraw.DrawLine3D(a, e, color);
                DebugDraw.DrawLine3D(b, f, color);
                DebugDraw.DrawLine3D(c, g, color);
                DebugDraw.DrawLine3D(d, h, color);

                return;
            }

            case CapsuleShape3D cap: {
                DebugDraw.DrawCapsule3D(cap, GlobalTransform, color);
                return;
            }

            case SphereShape3D sphere: {
                DebugDraw.DrawSphere3D(GlobalPosition, sphere.Radius, color);
                return;
            }

            default: {
                var pos = camera.UnprojectPosition(GlobalPosition);
                DebugDraw.DrawText2D($"UNHANDLED SHAPE {Shape.GetType()}", pos, color);
                return;
            }
        }
    }
}