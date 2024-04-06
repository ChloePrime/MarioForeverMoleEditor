using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ChloePrime.Godot.Util;
using ChloePrime.MarioForever.Shared;
using ChloePrime.MarioForever.Util;
using Godot;
using Array = Godot.Collections.Array;

namespace ChloePrime.MarioForever.Level;

public partial class MaFoLevelArea
{
	private void LoadTilemaps()
	{
		var children = GetChildren();
		foreach (var tilemap in children.OfType<TileMap>())
		{
			LoadTilemapWithLayers(tilemap);
		}
		foreach (var tilemapRoot in children.Where(IsPossibleTilemapRoot))
		{
			LoadTilemapSeperated(tilemapRoot);
		}
	}

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

	private static bool IsPossibleTilemapRoot(Node node)
	{
		return node.GetType() == typeof(Node2D) && node.Children().OfType<TileMap>().Any();
	}

	private void LoadTilemapSeperated(Node root) => LoadTilemapCore(root, root, TilemapType.None);

	/// <summary>
	/// 针对未勾选 Use Tilemap Layers 的 tmx scene 的处理，
	/// 支持伤害层。
	/// </summary>
	private void LoadTilemapCore(Node root, Node parent, TilemapType baseType)
	{
		var objRoot = this.GetPreferredRoot();
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
				LoadTilemapCore(root, node, type);
			}
			else if (node is Sprite2D possibleObject)
			{
				LoadObject(objRoot, possibleObject);
			}
		}
	}

	/// <summary>
	/// 针对使用 Use Tilemap Layers 的 tmx scene 的处理，
	/// 不支持伤害层。
	/// </summary>
	private void LoadTilemapWithLayers(TileMap tilemap)
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
		var root = this.GetPreferredRoot();
		
		if (resPathLayer == -1) return;
		
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
			PostInstantiate(instance, coord);
			
			root.AddChild(instance);
			if (instance is Node2D node2D)
			{
				node2D.GlobalPosition = myTransform.TranslatedLocal(coord * tileSize).Origin;
			}
			if (instance is ICustomTileOffsetObject o)
			{
				o.CustomOffset();
			}
			

			if (presetLayer != -1)
			{
				var preset = tileData.GetCustomDataByLayerId(presetLayer).AsInt32();
				if (preset > 0)
				{
					LoadPreset(instance, preset);
				}
			}
			
			
			tilemap.EraseCell(layer, coord);
		}
	}

	private ConditionalWeakTable<Type, ObjectTilePreset> PresetCache { get; } = new();

	private void LoadPreset(GodotObject instance, int preset)
	{
		var typeInfo = PresetCache.GetValue(instance.GetType(), _ =>
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
			return Level.TileLoadingPreset.Presets.FirstOrDefault(ConfigFilter);
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
		if (!@object.HasMeta(ResPathName) ||
		    (resPath = @object.GetMeta(ResPathName).AsString()).Length == 0 ||
		    !resPath.StartsWith("res://"))
		{
			return;
		}
		if (GD.Load(resPath) is not PackedScene prefab)
		{
			return;
		}
		var instance = prefab.Instantiate();
		PostInstantiate(instance, @object.Position);
		
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

	private static void PostInstantiate(Node @object, Vector2 context)
	{
		if (@object is INodeNameIsImportant)
		{
			@object.Name = $"{@object.GetClass()}_{context.X}_{context.Y}";
		}
	}

	private static readonly StringName ResPathName = "res_path";
	private static readonly StringName PresetName = "preset";
}