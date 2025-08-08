using System.Collections.Concurrent;
using System.Collections.Generic;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.Godot.Util;

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

    public static dynamic GetNodeDynamic(this Node self, NodePath path)
    {
        return self.GetNode(path);
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

    /// <summary>
    /// 在指定的全局位置添加子节点。<br/>
    /// 使用这个方法可以避免在开启物理插值时导致的物体在奇怪的地方闪现。
    /// </summary>
    /// <param name="parent">父节点</param>
    /// <param name="child">待添加的子节点</param>
    /// <param name="globalPosition">子节点的全局位置</param>
    public static void AddChildAt(this Node parent, Node child, Vector2 globalPosition)
    {
        if (child is Node2D child2d)
        {
            parent.AddChildAt(child2d, globalPosition);
        }
        else
        {
            parent.AddChild(child);
        }
    }

    /// <summary>
    /// 在指定的全局位置添加子节点。<br/>
    /// 使用这个方法可以避免在开启物理插值时导致的物体在奇怪的地方闪现。
    /// </summary>
    /// <param name="parent">父节点</param>
    /// <param name="child">待添加的子节点</param>
    /// <param name="globalPosition">子节点的全局位置</param>
    public static void AddChildAt(this Node parent, Node2D child, Vector2 globalPosition)
    {
        var piMode = child.PhysicsInterpolationMode;
        child.PhysicsInterpolationMode = Node.PhysicsInterpolationModeEnum.Off;
        
        if (parent is Node2D parent2d)
        {
            child.Position = parent2d.ToLocal(globalPosition);
            parent.AddChild(child);
        }
        else
        {
            parent.AddChild(child);
            child.GlobalPosition = globalPosition;
        }

        child.PhysicsInterpolationMode = piMode;
    }

    public static void TeleportTo(this Node2D self, Vector2 globalPosition)
    {
        var piMode = self.PhysicsInterpolationMode;
        self.PhysicsInterpolationMode = Node.PhysicsInterpolationModeEnum.Off;
        self.GlobalPosition = globalPosition;
        self.PhysicsInterpolationMode = piMode;
    }

    private static readonly ConcurrentDictionary<ulong, bool> DisablingPhItNodes = [];

    public static async void DisablePhysicsInterpolationUntilNextFrame(this Node node)
    {
        if (!DisablingPhItNodes.TryAdd(node.GetInstanceId(), true))
        {
            return;
        }
        
        var piMode = node.PhysicsInterpolationMode;
        node.PhysicsInterpolationMode = Node.PhysicsInterpolationModeEnum.Off;
        await node.WaitForPhysicsProcess();
        
        if (DisablingPhItNodes.TryRemove(node.GetInstanceId(), out _))
        {
            node.PhysicsInterpolationMode = piMode;   
        }
    }

    public static void ReparentSafely(this Node self, Node newParent, bool keepGlobalTransform = true)
    {
        var piMode = self.PhysicsInterpolationMode;
        self.PhysicsInterpolationMode = Node.PhysicsInterpolationModeEnum.Off;

        if (!GodotObject.IsInstanceValid(self.GetParent()))
        {
            if (self is Node2D self2d)
            {
                newParent.AddChildAt(self2d, self2d.GlobalPosition);  
            }
            else
            {
                newParent.AddChild(self);
            }
        }
        else
        {
            self.Reparent(newParent, keepGlobalTransform);   
        }
        
        self.PhysicsInterpolationMode = piMode;
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
