using System;
using ChloePrime.MarioForever.RPG;
using Godot;

namespace ChloePrime.MarioForever.Player;

public interface IGrabbable
{
    public Node2D Grabber { get; set; }
    public bool IsGrabbed => Grabber is not null;

    public bool CanBeGrabbed()
    {
        return false;
    }

    public event Action<Mario.GrabEvent> Grabbed;
    public event Action<Mario.GrabReleaseEvent> GrabReleased;
    public sealed Node2D AsNode => (Node2D)this;

    public static bool IsGrabbedByPlayer(IGrabbable grabbable)
    {
        return grabbable.IsGrabbed && (grabbable.Grabber is not IMarioForeverNpc npc || npc.NpcData.Friendly);
    }

    /// <see cref="GrabNotifyImpl"/> Use this to implement in your class
    protected internal void GrabNotify(Mario.GrabEvent ge, Mario.GrabReleaseEvent? gre);

    protected internal static sealed void GrabNotifyImpl(
        Action<Mario.GrabEvent> grabbed,
        Action<Mario.GrabReleaseEvent> released,
        Mario.GrabEvent ge,
        Mario.GrabReleaseEvent? gre)
    {
        if (gre is {} e)
        {
            released?.Invoke(e);
        }
        else
        {
            grabbed.Invoke(ge);
        }
    }
}