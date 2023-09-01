using System;
using System.Collections.Generic;
using System.Linq;
using ChloePrime.MarioForever.Util;
using Godot;
using MixelTools.Util.Extensions;
using Array = Godot.Collections.Array;

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
		foreach (var tilemapRoot in children.Where(IsPossibleTilemapRoot))
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

	private static readonly System.Collections.Generic.Dictionary<StringName, TilemapType> TypeLookup = new()
	{
		{ "Collision", TilemapType.SolidOnly },
	};

	private static bool IsPossibleTilemapRoot(Node node)
	{
		return node.GetType() == typeof(Node2D) && node.Children().OfType<TileMap>().Any();
	}

	private static void ProcessTilemapSeperated(Node root) => ProcessTilemapCore(root, root, TilemapType.None);

	/// <summary>
	/// 针对未勾选 Use Tilemap Layers 的 tmx scene 的处理，
	/// 支持伤害层。
	/// </summary>
	private static void ProcessTilemapCore(Node root, Node parent, TilemapType baseType)
	{
		foreach (var node in parent.Children())
		{
			var typeByName = TypeLookup.GetValueOrDefault(node.Name, TilemapType.None);
			var type = typeByName == TilemapType.None ? baseType : typeByName;
			if (node is TileMap tilemap)
			{
				LoadObjectsFromTile(tilemap, 0);
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
				ProcessTilemapCore(root, node, type);
			}
			else if (node is Sprite2D possibleObject)
			{
				LoadObject(root, possibleObject);
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
			else
			{
				LoadObjectsFromTile(tilemap, i);
			}
		}
	}

	private static void LoadObjectsFromTile(TileMap tilemap, int layer)
	{
		var all = tilemap.GetUsedCells(layer);
		using var _ = (Array)all;
		var tileSize = tilemap.TileSet.TileSize;
		var myTransform = tilemap.GlobalTransform.TranslatedLocal(tileSize / 2);
		var resPathLayer = tilemap.TileSet.GetCustomDataLayerByName("res_path");
		
		foreach (var coord in all)
		{
			var customData = tilemap.GetCellTileData(layer, coord)?.GetCustomDataByLayerId(resPathLayer);
			if (customData is not {} data || data.VariantType == Variant.Type.Nil)
			{
				continue;
			}
			var resPath = data.AsString();
			if (resPath.Length == 0 ||
			    !resPath.StartsWith("res://") ||
			    GD.Load(data.AsString()) is not PackedScene prefab)
			{
				continue;
			}
			var instance = prefab.Instantiate();
			tilemap.AddChild(instance);
			if (instance is Node2D node2D)
			{
				node2D.GlobalPosition = myTransform.TranslatedLocal(coord * tileSize).Origin;
			}
			tilemap.EraseCell(layer, coord);
		}
	}

	private static void LoadObject(Node parent, Sprite2D @object)
	{
		string resPath;
		if (!@object.HasMeta("res_path") ||
		    (resPath = @object.GetMeta("res_path").AsString()).Length == 0 ||
		    !resPath.StartsWith("res://"))
		{
			return;
		}
		if (GD.Load(resPath) is not PackedScene prefab)
		{
			return;
		}
		var instance = prefab.Instantiate();
		var sprSize = @object.Texture.GetSize() * @object.Scale;
		var offset = new Vector2(sprSize.X, -sprSize.Y) / 2;
		
		parent.AddChild(instance);
		if (instance is Node2D node2D)
		{
			node2D.GlobalPosition = @object.GlobalTransform.Orthonormalized().TranslatedLocal(offset).Origin;
		}
		LoadProperties(@object, instance);
		
		@object.QueueFree();
	}

	private static void LoadProperties(GodotObject dataSrc, GodotObject dataTarget)
	{
		foreach (var name in dataSrc.GetMetaList())
		{
			if (name == ResPathName) continue;
			dataTarget.Set(name, dataSrc.GetMeta(name));
		}
	}

	private static readonly StringName ResPathName = "res_path";
}
