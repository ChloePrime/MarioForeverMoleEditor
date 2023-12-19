using Godot;
using Godot.Collections;

namespace ChloePrime.MarioForever.Util;

[GlobalClass]
public partial class AudioStreamGroup : Resource
{
    [Export] public Array<AudioStream> AudioStreamList;

    public void Play()
    {
        switch (AudioStreamList.Count)
        {
            case 0:
                return;
            case 1:
                AudioStreamList[0].Play();
                return;
            case 2:
                AudioStreamList.PickRandom().Play();
                return;
        }
        var i = GD.RandRange(0, AudioStreamList.Count - 2);
        var stream = AudioStreamList[i];
        var last = AudioStreamList[^1];
        AudioStreamList[^1] = stream;
        AudioStreamList[i] = last;
        
        stream.Play();
    }
}