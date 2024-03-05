using System.Collections.Generic;
using ChloePrime.MarioForever.Util;
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
    
    public static bool TryGetNode<T>(this Node self, out T target, NodePath path) where T : class
    {
        return (target = self.GetNodeOrNull<T>(path)) != null;
    }

    public static void GetParent<T>(this Node node, out T parent) where T : class
    {
        parent = node.GetParent<T>();
    }

    public static bool TryGetParent<T>(this Node node, out T parent) where T : class
    {
        return (parent = node.GetParentOrNull<T>()) != null;
    }

    public static void Instantiate<T>(this PackedScene prefab, out T instance) where T : class
    {
        instance = prefab.Instantiate<T>();
    }
    
    public static bool TryInstantiate<T>(this PackedScene prefab, out T instance, out Node fallback) where T : class
    {
        fallback = prefab.Instantiate();
        return (instance = fallback as T) != null;
    }

    public static void Load<T>(out T target, NodePath path) where T : class
    {
        target = GD.Load<T>(path);
    }

    public static T ForceLocalToScene<T>(this T resource) where T : Resource
    {
        return resource.ResourceLocalToScene ? resource : (T)resource.Duplicate();
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

    public static IEnumerable<KinematicCollision2D> GetSlideCollisions(this CharacterBody2D body)
    {
        var count = body.GetSlideCollisionCount();
        for (var i = 0; i < count; i++)
        {
            yield return body.GetSlideCollision(i);
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

    public static R Clone<R>(this R resource) where R : Resource
    {
        return (R)resource.Duplicate();
    }
    
    /// <summary>
    /// Add a child, and return the newly created child
    /// </summary>
    /// 
    /// <param name="root">Root Node. We prefer to use <see cref="FrameUtil.GetPreferredRoot"/></param>
    /// <param name="prefab">The prefab of the child to be created</param>
    /// <returns>The newly created child node</returns>
    public static Node SpawnChild(this Node root, PackedScene prefab)
    {
        var instance = prefab.Instantiate();
        root.AddChild(instance);
        return instance;
    }

    /// <summary>
    /// Add a child, and return both parent and child.
    /// </summary>
    /// 
    /// <param name="root">Root Node. We prefer to use <see cref="FrameUtil.GetPreferredRoot"/></param>
    /// <param name="prefab">The prefab of the child to be created</param>
    /// <typeparam name="N">Type of the root node</typeparam>
    /// 
    /// <returns>
    /// The parent and the newly created child node,
    /// so you don't need to declare a variable
    /// or call <see cref="FrameUtil.GetPreferredRoot"/> twice
    /// </returns>
    public static (N root, Node instance) BirthChild<N>(this N root, PackedScene prefab) where N: Node
    {
        return (root, root.SpawnChild(prefab));
    }
}
