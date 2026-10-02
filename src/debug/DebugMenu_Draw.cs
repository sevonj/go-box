/*using Godot;

namespace sharp.debug;

public partial class DebugMenu {
    private static DebugTab TabDraw() {
        return new() {
            Name = "debug_draw", Commands = [
                new() {
                    Name = "debug_draw_distance",
                    Unit = "m",
                    GetValue = () => Debug.DebugDrawDistance,
                    SetValue = val => {
                        if (val.VariantType != Variant.Type.Float) return;
                        Debug.DebugDrawDistance = val.AsSingle();
                    }
                },
                new() {
                    Name = "draw_player_movement",
                    GetValue = () => Debug.DrawPlayerMovement,
                    SetValue = val => {
                        if (val.VariantType != Variant.Type.Bool) return;
                        Debug.DrawPlayerMovement = val.AsBool();
                    }
                },
                new() {
                    Name = "draw_player_interact",
                    GetValue = () => Debug.DrawPlayerInteract,
                    SetValue = val => {
                        if (val.VariantType != Variant.Type.Bool) return;
                        Debug.DrawPlayerInteract = val.AsBool();
                    }
                },
                new() {
                    Name = "draw_hitboxes",
                    GetValue = () => Debug.DrawHitboxes,
                    SetValue = val => {
                        if (val.VariantType != Variant.Type.Bool) return;
                        Debug.DrawHitboxes = val.AsBool();
                    }
                },
                new() {
                    Name = "draw_hurtboxes",
                    GetValue = () => Debug.DrawHurtboxes,
                    SetValue = val => {
                        if (val.VariantType != Variant.Type.Bool) return;
                        Debug.DrawHurtboxes = val.AsBool();
                    }
                },
                new() {
                    Name = "draw_triggers",
                    GetValue = () => Debug.DrawTriggers,
                    SetValue = val => {
                        if (val.VariantType != Variant.Type.Bool) return;
                        Debug.DrawTriggers = val.AsBool();
                    }
                },
                new() {
                    Name = "draw_interacts",
                    GetValue = () => Debug.DrawInteracts,
                    SetValue = val => {
                        if (val.VariantType != Variant.Type.Bool) return;
                        Debug.DrawInteracts = val.AsBool();
                    }
                },
                new() {
                    Name = "draw_info_nodes",
                    GetValue = () => Debug.DrawInfoNodes,
                    SetValue = val => {
                        if (val.VariantType != Variant.Type.Bool) return;
                        Debug.DrawInfoNodes = val.AsBool();
                    }
                },
                new() {
                    Name = "draw_breakables",
                    GetValue = () => Debug.DrawBreakables,
                    SetValue = val => {
                        if (val.VariantType != Variant.Type.Bool) return;
                        Debug.DrawBreakables = val.AsBool();
                    }
                }
            ],
        };
    }
}*/