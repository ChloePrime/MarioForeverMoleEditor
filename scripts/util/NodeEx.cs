using System.Collections.Generic;
using Godot;

namespace MixelTools.Util.Extensions;

public static partial class NodeEx
{
    public static void GetNode<T>(this Node self, out T target, NodePath path) where T : class
    {
        target = self.GetNode<T>(path);
    }
    
    public static void GetNodeOrNull<T>(this Node self, out T target, NodePath path) where T : class
    {
        target = self.GetNodeOrNull<T>(path);
    }

    public static void Load<T>(out T target, NodePath path) where T : class
    {
        target = GD.Load<T>(path);
    }
    
    public static IEnumerable<Node> Children(this Node node)
    {
        int count = node.GetChildCount();
        for (int i = 0; i < count; i++)
        {
            yield return node.GetChild(i);
        }
    }

    public static IEnumerable<Node> Walk(this Node root)
    {
        yield return root;

        int n = root.GetChildCount();
        for (int i = 0; i < n; i++)
        {
            foreach (var node in Walk(root.GetChild(i)))
            {
                yield return node;
            }
        }
    }

    public static Transform3D BestEffortGetGlobalTransform(this Node3D node)
    {
        Transform3D t = node.Transform;
        while ((node = node.GetParentOrNull<Node3D>()) != null)
        {
            t = node.Transform * t;
        }
        return t;
    }
}
