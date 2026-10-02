/*using Godot;
using sharp.util;

namespace sharp.debug;

[GlobalClass]
[Tool]
public partial class DebugTrajectoryVisualizer : MeshInstance3D {
    [Export] public uint Steps = 50;
    [Export] public Trajectory Trajectory;

    public override void _EnterTree() {
        if (!Engine.IsEditorHint())
            return;
        RebuildMesh();
    }

    public void RebuildMesh() {
        var mesh = (ImmediateMesh)Mesh;
        if (!IsInstanceValid(mesh)) {
            mesh = new ImmediateMesh();
            Mesh = mesh;
        }

        mesh.ClearSurfaces();

        if (!IsInstanceValid(Trajectory))
            return;

        mesh.SurfaceBegin(Mesh.PrimitiveType.LineStrip);
        mesh.SurfaceAddVertex(Vector3.Zero);

        double time = 0;
        var delta = 1d / Engine.PhysicsTicksPerSecond;
        var offset = Vector3.Zero;
        for (var _ = 0; _ < Steps; _++) {
            time += delta;
            offset.Y += Trajectory.VSpeedAt(time) * (float)delta;
            offset.Z += Trajectory.HSpeed * (float)delta;

            mesh.SurfaceAddVertex(offset);
        }

        mesh.SurfaceEnd();
    }
}*/