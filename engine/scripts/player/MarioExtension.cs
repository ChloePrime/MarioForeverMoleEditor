using ChloePrime.MarioForever.Util;
using Godot;
using Godot.Collections;

namespace ChloePrime.MarioForever.Player;

public static class MarioExtension
{
    public static Mario GetMario(this SceneTree tree)
    {
        return tree.GetPlayer() as Mario;
    }
    
    public static Node GetPlayer(this Node node)
    {
        return node.GetTree().GetPlayer();
    }
    
    public static Node GetPlayer(this SceneTree tree)
    {
        return tree?.GetFirstNodeInGroup(MaFo.Groups.Player) as Node2D;
    }
    
    public static Array<Node> GetAllPlayers(this SceneTree tree)
    {
        return tree?.GetNodesInGroup(MaFo.Groups.Player);
    }
}