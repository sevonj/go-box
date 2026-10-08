using Godot;

namespace GoBox.interfaces;

public interface IHealth {
    [Signal]
    public delegate void DiedEventHandler();

    public int Hp { get; set; }
    public int MaxHp { get; set; }
    public bool IsDead();
    public void Damage(int dmg /*, DamageTypes type = DamageTypes.Unset */);
    public void Kill( /* DamageTypes type = DamageTypes.Unset */);
}