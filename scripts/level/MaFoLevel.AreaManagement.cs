using System;
using System.Linq;
using ChloePrime.Godot.Util;

namespace ChloePrime.MarioForever.Level;

public partial class MaFoLevel
{
    public MaFoLevelArea[] Areas { get; private set; }
    public int CurrentAreaId { get; private set; }

    public MaFoLevelArea GetAreaById(int id)
    {
        EnsureAreaInitialized();
        if (id < 0 || id >= Areas.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(id), id, null);
        }
        return Areas[id];
    }

    public void SetArea(int id)
    {
        EnsureAreaInitialized();
        SetArea(Areas[id]);
    }

    public void SetArea(MaFoLevelArea area)
    {
        EnsureAreaInitialized();
        for (var i = 0; i < Areas.Length; i++)
        {
            var a = Areas[i];
            if (a.SetActivated(a == area))
            {
                CurrentAreaId = i;
                a._AreaActivated();
            }
        }
    }

    private void AreasReady()
    {
        EnsureAreaInitialized();
        foreach (var area in Areas)
        {
            _ = area.Level;
            area.Visible = true;
        }
        SetArea(DefaultArea);
    }

    private void EnsureAreaInitialized()
    {
        Areas ??= this.Children().OfType<MaFoLevelArea>().ToArray();
    }
}