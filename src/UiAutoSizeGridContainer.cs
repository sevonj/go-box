using Godot;

namespace GoBox;

// This is wrapper instead of the GridContainer itself.
// Otherwise, switching to fewer columns doesn't work because Size includes overflow. 
[GlobalClass]
public partial class UiAutoSizeGridContainer : Control {
    [Export] public float MinColumnSize = 128.0f;
    [Export] public GridContainer Grid;

    public override void _Process(double delta) {
        var calculated = (int)Mathf.Floor(Size.X / MinColumnSize);
        Grid.Columns = Mathf.Max(calculated, 1);
    }
}