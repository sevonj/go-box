using Godot;

namespace GoBox;

[GlobalClass]
public partial class Weapon : Node3D {
    public virtual string WpnName() => "PLACEHOLDER";
    public virtual string WpnDescription() => "PLACEHOLDER";

    public virtual void Fire() {
    }

    public virtual void FireAlt() {
    }
}