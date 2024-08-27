using ChloePrime.MarioForever.Player;
using ChloePrime.MarioForever.UI;
using Godot;

namespace ChloePrime.MarioForever.Util;

public struct PlayerFinder
{
    public PlayerFinder(Node node) : this(node.GetTree())
    {
    }
    
    public PlayerFinder(SceneTree tree)
    {
        _tree = tree;
    }
    
    public Node2D Player
    {
        get
        {
            var ret = _player ??= _tree.GetPlayer() as Node2D;
            return GodotObject.IsInstanceValid(ret) ? ret : _player = null;
        }
    }

    public bool TryGet(out Node2D mario)
    {
        return GodotObject.IsInstanceValid(mario = Player);
    }
    
    public bool TryGetMario(out Mario mario)
    {
        TryGet(out var player);
        return (mario = player as Mario) != null;
    }

    private SceneTree _tree;
    private Node2D _player;
}