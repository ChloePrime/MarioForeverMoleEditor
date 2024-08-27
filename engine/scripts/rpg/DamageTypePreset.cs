using Godot;

namespace ChloePrime.MarioForever.RPG;

[GlobalClass]
public partial class DamageTypePreset : Resource
{
    [Export] public DamageType Value { get; set; }
    public bool ContainsAny(DamageType predicate) => Value.ContainsAny(predicate);
    public bool ContainsAll(DamageType predicate) => Value.ContainsAll(predicate);

    public static implicit operator DamageType(DamageTypePreset self) => self.Value;
}