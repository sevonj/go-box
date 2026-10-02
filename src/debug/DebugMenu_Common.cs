/*using Godot;

namespace sharp.debug;

public partial class DebugMenu {
    private static DebugTab TabCommon() {
        return new() {
            Name = "common", Commands = [
                new() { Name = "reset_player", Action = _ => { Game.Stage.ResetPlayer(); } },
                new() { Name = "reset_enemies", Action = _ => { Game.Stage.ResetEnemies(); } },
                new() { Name = "reset_breakables", Action = _ => { Game.Stage.ResetBreakables(); } },
                new() { Name = "clear_projectiles", Action = _ => { Game.Stage.ClearProjectiles(); } },
                new() {
                    Name = "npc_enable_think",
                    GetValue = () => Debug.NpcEnableThink,
                    SetValue = val => {
                        if (val.VariantType != Variant.Type.Bool) return;
                        Debug.NpcEnableThink = val.AsBool();
                    }
                },
                new() {
                    Name = "npc_god_mode",
                    GetValue = () => Debug.NpcGodMode,
                    SetValue = val => {
                        if (val.VariantType != Variant.Type.Bool) return;
                        Debug.NpcGodMode = val.AsBool();
                    }
                },
            ]
        };
    }
}*/