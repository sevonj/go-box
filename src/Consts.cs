using System;

namespace GoBox;

[Flags]
public enum ColLayer : uint {
    None = 0,
    World = 1 << 0,
    Player = 1 << 1,
    Props = 1 << 2,
    Toolgunnable = 1 << 8,

    PhysActors = Player | Props,


    /*
         PropsPassthrough = 1 << 3,
       Enemies = 1 << 4,
       StickyGrenade = 1 << 5,
       Gibs = 1 << 6,
       Losblock = 1 << 7,

     */
}

/*[Flags]
public enum ColMask : uint {
    World = 0,
    Player = ColLayer.World | ColLayer.Props,
    Props = ColLayer.World | ColLayer.Player | ColLayer.Props,
    Camera = ColLayer.World,
    EnvHazard = ColLayer.Player, // | ColLayer.Enemies | ColLayer.Props | ColLayer.StickyGrenade,

    // Enemies = ColLayer.World | ColLayer.Player,
    // PlayerProjectile = ColLayer.World | ColLayer.Props | ColLayer.Enemies,
    // EnemyProjectile = ColLayer.World | ColLayer.Player | ColLayer.Props | ColLayer.PropsPassthrough,
    // StickyGrenade = ColLayer.World | ColLayer.Props | ColLayer.PropsPassthrough | ColLayer.Enemies,
    // StickyExplosion = ColLayer.Player | ColLayer.Props | ColLayer.PropsPassthrough | ColLayer.Enemies,
    // PlayerMelee = ColLayer.World | ColLayer.Player | ColLayer.Props | ColLayer.PropsPassthrough | ColLayer.Enemies | ColLayer.StickyGrenade,
    // Gibs = ColLayer.World | ColLayer.Player | ColLayer.Props | ColLayer.Enemies,
    // Los = ColLayer.World | ColLayer.Props | ColLayer.Enemies | ColLayer.Losblock,
    // Items = ColLayer.Player,
    // Pitblock = ColLayer.Player | ColLayer.StickyGrenade,
}*/

[Flags]
public enum SurfaceFlags : uint {
    None = 0,
    NoJump = 1 << 0,
    NoLedgeGrab = 1 << 1,
    NoLedgePullUp = 1 << 2,
    DetonateOnContact = 1 << 3,
    DefuseOnContact = 1 << 4
}

public enum Gravy : uint {
    None = 0,
    Little = 1,
    Medium = 2,
    Lots = 3
}

public struct ProcessPriorities {
    public const int DebugMenu = -9997;
    public const int DebugDraw = 9999;
    public const int PDebugDraw = -9999;
    public const int FramePostProcess = 5000;
}

[Flags]
public enum DamageTypes : uint {
    Unset = 0,
    Explosive = 1 << 0,
    DirectPipe = 1 << 1,
    Drown = 1 << 2,
}

public struct PProcessPriorities {
    public const int Player = 1;
    public const int PhysProp = 2;
}