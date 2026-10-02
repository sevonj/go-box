using Godot;

namespace GoBox;

[GlobalClass]
public partial class UiPictureButton : Button {
    public new string Text {
        get => _label.Text;
        set => _label.Text = value;
    }

    public Texture2D Texture {
        get => _picture.Texture;
        set => _picture.Texture = value;
    }

    public Color BackgroundColor {
        get => _picture.BackgroundColor;
        set => _picture.BackgroundColor = value;
    }

    [Export] private Label _label;
    [Export] private UiPictureFrame _picture;

    public static UiPictureButton Instantiate() =>
        (UiPictureButton)GD.Load<PackedScene>("res://ui/ui_picture_button.tscn").Instantiate();

    private UiPictureButton() {
    }

    public override void _Ready() {
        if (!IsInstanceValid(_label))
            GD.PrintErr(GetPath(), " _label not valid");
        if (!IsInstanceValid(_picture))
            GD.PrintErr(GetPath(), " _picture not valid");
    }
}