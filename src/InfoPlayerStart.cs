using Godot;

namespace GoBox;

[GlobalClass]
public partial class InfoPlayerStart : Marker3D {
    // [Export] private bool overrideDefault;

    public override void _Ready() {
        Game.World.RegisterPlayerStart(this);

        // if (overrideDefault) Game.World.DefaultPlayerStart = this;
    }

    public override void _Process(double delta) {
        //if (Debug.DrawInfoNodes) _DebugDraw();
    }

    /* private void _DebugDraw() {
        var camera = GetViewport().GetCamera3D();
        if (!IsInstanceValid(camera)) return;
        if (GlobalPosition.DistanceTo(camera.GlobalPosition) > Debug.DebugDrawDistance) return;

        DebugDraw.DrawPoint3D(GlobalPosition, Colors.LimeGreen);
        DebugDraw.DrawText3D(GetType().Name, GlobalPosition, Colors.LimeGreen);
        DebugDraw.DrawLine3D(GlobalPosition, GlobalPosition - GlobalBasis.Column2, Colors.LimeGreen);
        var arrowOff = -GlobalBasis.Column2 * 0.8f + GlobalBasis.Column0 * 0.2f;
        var arrowOff2 = -GlobalBasis.Column2 * 0.8f - GlobalBasis.Column0 * 0.2f;
        DebugDraw.DrawLine3D(GlobalPosition - GlobalBasis.Column2, GlobalPosition + arrowOff, Colors.LimeGreen);
        DebugDraw.DrawLine3D(GlobalPosition - GlobalBasis.Column2, GlobalPosition + arrowOff2, Colors.LimeGreen);
    } */
}