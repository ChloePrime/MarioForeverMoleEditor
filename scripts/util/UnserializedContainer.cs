namespace ChloePrime.MarioForever.Util;

/// <summary>
/// 用这个包装一些对象，那么 Godot 就不会序列化它们
/// </summary>
/// <typeparam name="T"></typeparam>
public struct UnserializedContainer<T>
{
    public T Value;
}