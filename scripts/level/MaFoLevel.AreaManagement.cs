using System.Linq;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever.Level;

public partial class MaFoLevel
{
    public MaFoLevelArea[] Areas { get; private set; }

    public void SetArea(MaFoLevelArea area)
    {
        foreach (var a in Areas)
        {
            var activate = a == area;
            a.SetActivated(activate);
            if (activate) a._AreaActivated();
        }
    }

    private void AreasReady()
    {
        Areas = this.Children().OfType<MaFoLevelArea>().ToArray();
        foreach (var area in Areas)
        {
            _ = area.Level;
        }
        SetArea(DefaultArea);
    }
}