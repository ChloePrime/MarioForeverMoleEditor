#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Godot;
using Godot.Collections;

namespace ChloePrime.MarioForever.Util;

/// <summary>
/// 对 <see cref="PhysicsDirectSpaceState3D.IntersectRay"/> 返回结果的强类型封装。
/// 在另一个 part 还有针对体素世界用的方法和属性。
/// </summary>
[StructLayout(LayoutKind.Auto)]
public struct ShapeHitResult
{
    public ShapeHitResult(Dictionary result) : this()
    {
        _data = result;
    }

    public GodotObject Collider => _collider ??= (GodotObject)_data["collider"];
    public int ColliderId => _colliderId ??= (int)_data["collider_id"];
    public Rid Rid => _rid ??= (Rid)_data["rid"];
    public int Shape => _shape ??= (int)_data["shape"];
    
    private readonly Dictionary _data;
    private GodotObject? _collider;
    private int? _colliderId;
    private Rid? _rid;
    private int? _shape;
}

public static class ShapeHitResultEx
{
    public static IEnumerable<ShapeHitResult> IntersectTyped(
        this CollisionShape2D shape,
        PhysicsShapeQueryParameters2D param,
        int maxResults = 32)
    {
        var state = shape.GetWorld2D().DirectSpaceState;
        param.Shape = shape.Shape;
        param.Transform = shape.GlobalTransform;
        return state.IntersectShapeTyped(param, maxResults);
    }
    
    public static IEnumerable<ShapeHitResult> IntersectShapeTyped(
        this PhysicsDirectSpaceState2D state,
        PhysicsShapeQueryParameters2D parameters,
        int maxResults = 32)
    {
        return state.IntersectShape(parameters, maxResults).Select(d => new ShapeHitResult(d));
    }
}