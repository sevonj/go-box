using Godot;

namespace GoBox;

[GlobalClass]
public partial class UiPictureFrame : AspectRatioContainer {
    public Texture2D Texture {
        get => _textureRect.Texture;
        set => _textureRect.Texture = value;
    }

    public Color BackgroundColor {
        get => _bgColorRect.Color;
        set => _bgColorRect.Color = value;
    }

    [Export] private TextureRect _textureRect;
    [Export] private TextureRect _placeholder;
    [Export] private ColorRect _bgColorRect;


    public UiPictureFrame Instantiate() =>
        (UiPictureFrame)GD.Load<PackedScene>("res://ui/ui_picture_frame.tscn").Instantiate();

    private UiPictureFrame() {
    }


    public override void _Ready() {
        if (!IsInstanceValid(_textureRect))
            GD.PrintErr(GetPath(), " _textureRect not valid");
        if (!IsInstanceValid(_placeholder))
            GD.PrintErr(GetPath(), " _placeholder not valid");
        if (!IsInstanceValid(_bgColorRect))
            GD.PrintErr(GetPath(), " _bgColorRect not valid");
    }

    public override void _Process(double delta) {
        _placeholder.Visible = !IsInstanceValid(_textureRect.Texture);
    }
}