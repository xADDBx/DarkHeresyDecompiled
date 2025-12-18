using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.VirtualTerrain.Streaming;
using UnityEngine;
using UnityEngine.Serialization;

namespace Owlcat.Runtime.Visual.Terrain;

[CreateAssetMenu(menuName = "Owlcat/Terrain Palette")]
public sealed class OwlcatTerrainPalette : ScriptableObject
{
	public TerrainLayerGuid[] LayerDataGuids = Array.Empty<TerrainLayerGuid>();

	public Vector4[] LayerDataStampingWeights = Array.Empty<Vector4>();

	public OwlcatTerrainPalette[] ChildrenPalettes = Array.Empty<OwlcatTerrainPalette>();

	[FormerlySerializedAs("LayerDataMasksScale")]
	public Vector4[] OBSOLETE_LayerDataMasksScale;

	[FormerlySerializedAs("LayerDataUvMatrix")]
	public Vector4[] OBSOLETE_LayerDataUvMatrix;

	[FormerlySerializedAs("LayerDataParams")]
	public Vector4[] OBSOLETE_LayerDataParams;

	[FormerlySerializedAs("TerrainLayerGuids")]
	public string[] OBSOLETE_TerrainLayerGuids;

	[FormerlySerializedAs("BakedDiffuse")]
	public Texture2DArray OBSOLETE_BakedDiffuse;

	[FormerlySerializedAs("BakedNormalMap")]
	public Texture2DArray OBSOLETE_BakedNormalMap;

	[FormerlySerializedAs("BakedMaskMap")]
	public Texture2DArray OBSOLETE_BakedMaskMap;

	public void GetTerrainLayerGuids(List<TerrainLayerGuid> results)
	{
		if (ChildrenPalettes != null && ChildrenPalettes.Length != 0)
		{
			OwlcatTerrainPalette[] childrenPalettes = ChildrenPalettes;
			for (int i = 0; i < childrenPalettes.Length; i++)
			{
				childrenPalettes[i].GetTerrainLayerGuids(results);
			}
		}
		else if (LayerDataGuids != null)
		{
			results.AddRange(LayerDataGuids);
		}
	}

	public void GetTerrainLayerIds(List<int> results)
	{
		if (ChildrenPalettes != null && ChildrenPalettes.Length != 0)
		{
			OwlcatTerrainPalette[] childrenPalettes = ChildrenPalettes;
			for (int i = 0; i < childrenPalettes.Length; i++)
			{
				childrenPalettes[i].GetTerrainLayerIds(results);
			}
		}
		else if (LayerDataGuids != null)
		{
			TerrainLayerGuid[] layerDataGuids = LayerDataGuids;
			foreach (TerrainLayerGuid guid in layerDataGuids)
			{
				results.Add(TerrainLayerId.GetTerrainLayerId(guid));
			}
		}
	}
}
