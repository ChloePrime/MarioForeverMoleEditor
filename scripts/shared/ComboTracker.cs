using System;
using System.Diagnostics.CodeAnalysis;
using Godot;

namespace ChloePrime.MarioForever.Shared;

[GlobalClass]
public partial class ComboTracker : Node
{
    [Export, MaybeNull] public ComboRule RuleOverride { get; set; }
    
    public ComboRule Rule => RuleOverride ?? this.GetRule().DefaultComboRule;

    public void MoveNext()
    {
        _rule = Rule;
        _position = (_position + 1) % _rule.ScoreList.Count;
    }

    public void Reset() => _position = -1;

    [return: NotNull]
    public Node2D CreateScore()
    {
        CheckMoveNextCalled();
        var rule = _rule ??= Rule;
        var scores = rule.ScoreList;
        return scores[Math.Clamp(_position, 0, scores.Count - 1)].Instantiate<Node2D>();
    }

    [return: MaybeNull]
    public AudioStream GetSound()
    {
        CheckMoveNextCalled();
        var rule = _rule ??= Rule;
        var sounds = rule.SoundList;
        return sounds[Math.Clamp(_position, 0, sounds.Count - 1)];
    }

    private void CheckMoveNextCalled()
    {
        if (_position < 0)
        {
            throw new IndexOutOfRangeException("Call MoveNext() before getting result from ComboTracker!");
        }
    }

    private ComboRule _rule;
    private int _position = -1;
}