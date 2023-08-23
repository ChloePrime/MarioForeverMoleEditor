using System;
using System.Collections.Generic;
using System.Linq;
using ChloePrime.MarioForever.Util;
using Godot;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever.Level;

/// <summary>
/// 初始化关卡：
/// 1. 让作为本节点直接子节点的 Tilemap 的碰撞层（名字为 Collision 的层）变为透明
/// </summary>
[GlobalClass]
public partial class MaFoLevel : Node
{
	[Export]
	public AudioStream LevelMusic { get; private set; }
	
	public override void _Ready()
	{
		if (LevelMusic is { } bgm)
		{
			BackgroundMusic.Music = bgm;
		}
		
		if (this.GetLevelManager() is null)
		{
			GetTree().Root.ContentScaleAspect = Window.ContentScaleAspectEnum.Keep;
		}
		
		ProcessTilemaps();
	}

	private void ProcessTilemaps()
	{
		var children = GetChildren();
		foreach (var tilemap in children.OfType<TileMap>())
		{
			ProcessTilemapWithLayers(tilemap);
		}
		foreach (var tilemapRoot in children.Where(it => it.GetType() == typeof(Node2D)))
		{
			ProcessTilemapSeperated(tilemapRoot);
		}
	}

	private AudioStreamPlayer _musicPlayer;
	private AudioStream _levelMusic;
	private static readonly StringName MusicPlayerName = "Music Player";

	private enum TilemapType
	{
		None,
		SolidOnly,
		DamageSource,
		DeathSource,
	}

	private static readonly Dictionary<StringName, TilemapType> TypeLookup = new()
	{
		{ "Collision", TilemapType.SolidOnly },
	};

	private static void ProcessTilemapSeperated(Node tilemapRoot) => ProcessTilemapCore(tilemapRoot, TilemapType.None);

	/// <summary>
	/// 针对未勾选 Use Tilemap Layers 的 tmx scene 的处理，
	/// 支持伤害层。
	/// </summary>
	private static void ProcessTilemapCore(Node tilemapRoot, TilemapType baseType)
	{
		foreach (var node in tilemapRoot.Children())
		{
			var typeByName = TypeLookup.GetValueOrDefault(node.Name, TilemapType.None);
			var type = typeByName == TilemapType.None ? baseType : typeByName;
			if (node is TileMap tilemap)
			{
				switch (type)
				{
					case TilemapType.SolidOnly:
						tilemap.Visible = false;
						break;
					case TilemapType.None:
					default:
						break;
				}
			}
			else if (node.GetType() == typeof(Node2D))
			{
				ProcessTilemapCore(node, type);
			}
		}
	}

	/// <summary>
	/// 针对使用 Use Tilemap Layers 的 tmx scene 的处理，
	/// 不支持伤害层。
	/// </summary>
	private static void ProcessTilemapWithLayers(TileMap tilemap)
	{
		var count = tilemap.GetLayersCount();
		for (var i = 0; i < count; i++)
		{
			if (tilemap.GetLayerName(i).Contains("Collision", StringComparison.OrdinalIgnoreCase))
			{
				tilemap.SetLayerModulate(i, Colors.Transparent);
			}
		}
	}
}
