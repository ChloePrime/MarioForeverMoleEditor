using ChloePrime.Godot.Util;
using Godot;

namespace ChloePrime.MarioForever.Enemy;

[GlobalClass]
[Icon("res://engine/resources/enemies/AT_goomba_photo.tres")]
public partial class GoombaPhoto : GravityObjectBase, ICorpse
{
    public override void _Ready()
    {
        base._Ready();
        this.GetNode(out _sprite, NpSprite);
    }

    private static readonly NodePath NpSprite = "Sprite";
    private Sprite2D _sprite;
}