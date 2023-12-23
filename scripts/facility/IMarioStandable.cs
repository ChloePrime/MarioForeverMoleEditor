using Godot;

namespace ChloePrime.MarioForever.Facility;

/// <summary>
/// 马里奥站上去以后调用虚方法的接口
/// </summary>
public interface IMarioStandable
{
    public void ProcessMarioStandOn(Node2D mario);
}