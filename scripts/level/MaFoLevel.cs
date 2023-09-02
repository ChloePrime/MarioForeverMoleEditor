using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
	[Export] public AudioStream LevelMusic { get; private set; }
	[Export] public ObjectTilePresetList TileLoadingPreset { get; private set; }

	public override void _Ready()
	{
		TileLoadingPreset ??= GD.Load<ObjectTilePresetList>("res://tiles/R_object_tile_presets.tres");
		
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

	private void ProcessTilemapSeperated(Node root) => ProcessTilemapCore(root, root, TilemapType.None);

	/// <summary>
	/// 针对未勾选 Use Tilemap Layers 的 tmx scene 的处理，
	/// 支持伤害层。
	/// </summary>
	private void ProcessTilemapCore(Node root, Node parent, TilemapType baseType)
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
	private void ProcessTilemapWithLayers(TileMap tilemap)
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

	private void LoadObjectsFromTile(TileMap tilemap, int layer)
	{
		var all = tilemap.GetUsedCells(layer);
		using var _ = (Array)all;
		var tileSize = tilemap.TileSet.TileSize;
		var myTransform = tilemap.GlobalTransform.TranslatedLocal(tileSize / 2);
		var resPathLayer = tilemap.TileSet.GetCustomDataLayerByName("res_path");
		var presetLayer = tilemap.TileSet.GetCustomDataLayerByName("preset");
		
		foreach (var coord in all)
		{
			if (tilemap.GetCellTileData(layer, coord) is not { } tileData)
			{
				continue;
			}
			var resPath = tileData.GetCustomDataByLayerId(resPathLayer).AsString();
			if (resPath.Length == 0 ||
			    !resPath.StartsWith("res://") ||
			    GD.Load(resPath) is not PackedScene prefab)
			{
				continue;
			}
			var instance = prefab.Instantiate();
			
			tilemap.AddChild(instance);
			if (instance is Node2D node2D)
			{
				node2D.GlobalPosition = myTransform.TranslatedLocal(coord * tileSize).Origin;
			}

			var preset = tileData.GetCustomDataByLayerId(presetLayer).AsInt32();
			if (preset > 0)
			{
				LoadPreset(instance, preset);
			}
			
			tilemap.EraseCell(layer, coord);
		}
	}

	private ConditionalWeakTable<Type, ObjectTilePreset> PresetCache { get; } = new();

	private void LoadPreset(GodotObject instance, int preset)
	{
		var typeInfo = PresetCache.GetValue(instance.GetType(), type =>
		{
			bool ConfigFilter(ObjectTilePreset p)
			{
				var script0 = instance.GetScript();
				if (script0.VariantType != Variant.Type.Object || script0.AsGodotObject() is not Script script)
				{
					return false;
				}
				do
				{
					if (script == p.BaseClass)
					{
						return true;
					}
				} while ((script = script.GetBaseScript()) != null);

				return false;
			}
			return TileLoadingPreset.Presets.FirstOrDefault(ConfigFilter);
		});
		if (typeInfo is null)
		{
			return;
		}
		
		if (preset <= 0 || preset > typeInfo.PropertiesById.Count)
		{
			this.LogWarn($"Invalid preset: preset should in 1..{typeInfo.PropertiesById.Count}");
			return;
		}
		var presetData = typeInfo.PropertiesById[preset - 1];
		foreach (var (prop, value) in presetData)
		{
			instance.Set(prop, value);
		}
	}

	private void LoadObject(Node parent, Sprite2D @object)
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
		
		parent.AddChild(instance);
		if (instance is Node2D node2D)
		{
			node2D.GlobalPosition = GetTileObjectRealPosition(@object);
		}
		LoadProperties(@object, instance);
		
		@object.QueueFree();
	}

	private static Vector2 GetTileObjectRealPosition(Sprite2D tileObject)
	{
		Vector2 sprSize;
		if (tileObject.RegionEnabled)
		{
			sprSize = tileObject.RegionRect.Size * tileObject.GlobalScale;
		}
		else
		{
			sprSize = tileObject.Texture.GetSize() * tileObject.GlobalScale;
		}
		var offset = new Vector2(sprSize.X, -sprSize.Y) / 2;
		return tileObject.GlobalTransform.Orthonormalized().TranslatedLocal(offset).Origin;
	}

	private void LoadProperties(GodotObject dataSrc, GodotObject dataTarget)
	{
		if (dataSrc.HasMeta(PresetName))
		{
			LoadPreset(dataTarget, dataSrc.GetMeta(PresetName).AsInt32());
			return;
		}
		foreach (var name in dataSrc.GetMetaList())
		{
			if (name == ResPathName) continue;
			dataTarget.Set(name, dataSrc.GetMeta(name));
		}
	}

	private static readonly StringName ResPathName = "res_path";
	private static readonly StringName PresetName = "preset";
}
