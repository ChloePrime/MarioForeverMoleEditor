#nullable enable
using System.Runtime.InteropServices;
using Godot;
using Godot.Collections;

namespace ChloePrime.MarioForever.Util;

/// <summary>
/// 对 <see cref="PhysicsDirectSpaceState3D.IntersectRay"/> 返回结果的强类型封装。
/// 在另一个 part 还有针对体素世界用的方法和属性。
/// </summary>
[StructLayout(LayoutKind.Auto)]
public struct ShapeHitResult3D
{
    public ShapeHitResult3D(Dictionary result) : this()
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
