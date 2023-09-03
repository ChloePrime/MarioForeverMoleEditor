﻿using ChloePrime.MarioForever.Player;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Bonus;

public partial class Coin : Area2D
{
    [Export] public int Value { get; set; } = 1;
    [Export] public AudioStream Sound { get; set; } = GD.Load<AudioStream>("res://resources/bonus/SE_coin.ogg");
    
    public override void _Ready()
    {
        base._Ready();
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D other)
    {
        if (other is not Mario) return;
        GlobalData.Coins += Value;
        Sound?.Play();
        QueueFree();
    }
}