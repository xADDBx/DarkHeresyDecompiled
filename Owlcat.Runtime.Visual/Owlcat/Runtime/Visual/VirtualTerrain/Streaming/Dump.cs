using System.Collections.Generic;
using UnityEngine;

namespace Owlcat.Runtime.Visual.VirtualTerrain.Streaming;

internal sealed class Dump
{
	public readonly List<int> LayerLods = new List<int>();

	public readonly List<int>[] LayerAtlasSlotIds = new List<int>[3];

	public readonly List<int>[] AtlasSlotUploadedLayerIds = new List<int>[3];

	public readonly List<Texture2D> AtlasTextures = new List<Texture2D>();

	public Texture2D UploadTexture;

	public Dump()
	{
		for (int i = 0; i < 3; i++)
		{
			LayerAtlasSlotIds[i] = new List<int>();
			AtlasSlotUploadedLayerIds[i] = new List<int>();
		}
	}

	public void Clear()
	{
		LayerLods.Clear();
		List<int>[] layerAtlasSlotIds = LayerAtlasSlotIds;
		for (int i = 0; i < layerAtlasSlotIds.Length; i++)
		{
			layerAtlasSlotIds[i].Clear();
		}
		layerAtlasSlotIds = AtlasSlotUploadedLayerIds;
		for (int i = 0; i < layerAtlasSlotIds.Length; i++)
		{
			layerAtlasSlotIds[i].Clear();
		}
		AtlasTextures.Clear();
		UploadTexture = null;
	}
}
