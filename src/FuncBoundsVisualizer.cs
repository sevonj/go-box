using Godot;
using Godot.Collections;

namespace GoBox;

[GlobalClass]
public partial class FuncBoundsVisualizer : MeshInstance3D {
    public Aabb Bounds {
        get => _bounds;
        set {
            _bounds = value;
            RebuildMesh();
        }
    }

    private Aabb _bounds;

    public FuncBoundsVisualizer() {
        Name = "FuncBoundsVisualizer";
    }

    // --- Node

    public override void _Ready() {
        RebuildMesh();
    }

    // --- Private

    private void RebuildMesh() {
        var min = _bounds.Position;
        var max = _bounds.End;

        Array<Vector3> vbuf = [
            new(min.X, max.Y, max.Z),
            new(max.X, max.Y, max.Z),
            new(min.X, min.Y, max.Z),
            new(max.X, min.Y, max.Z),
            new(max.X, min.Y, min.Z),
            new(max.X, max.Y, max.Z),
            new(max.X, max.Y, min.Z),
            new(min.X, max.Y, max.Z),
            new(min.X, max.Y, min.Z),
            new(min.X, min.Y, max.Z),
            new(min.X, min.Y, min.Z),
            new(max.X, min.Y, min.Z),
            new(min.X, max.Y, min.Z),
            new(max.X, max.Y, min.Z),
        ];

        var st = new SurfaceTool();
        st.Begin(Mesh.PrimitiveType.TriangleStrip);
        st.SetMaterial(ResourceLoader.Load<Material>("res://assets/materials/mat_bounds_visualizer.tres"));
        foreach (var v in vbuf) {
            st.AddVertex(v);
        }

        Mesh = st.Commit();
    }
}