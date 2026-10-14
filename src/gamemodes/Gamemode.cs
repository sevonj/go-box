using Godot;

namespace GoBox.gamemodes;

[GlobalClass]
public partial class Gamemode : Node {
    public bool AllowContentMenu { get; protected set; } = true;

    // -- Node

    public override void _Ready() {
        Name = "Gamemode";
        Game.World.SpawnPlayer();
    }
}