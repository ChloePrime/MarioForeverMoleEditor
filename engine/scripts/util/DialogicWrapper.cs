using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ChloePrime.Godot.Util;
using Godot;

namespace ChloePrime.MarioForever.Util;

public class DialogicWrapper
{
    public static Node Instance => _instance.Value;
    
    // public static void Start()
    
    private static readonly Lazy<Node> _instance = new(FindSingleton);

    [return: MaybeNull]
    private static Node FindSingleton()
    {
        return Engine.GetMainLoop() is SceneTree tree 
            ? tree.Root.Children().First(node => node.Name == (StringName)"Dialogic")
            : null;
    }
}