using System;
using Owlcat.Runtime.Visual.VirtualTerrain;
using Owlcat.Runtime.Visual.VirtualTerrain.Streaming;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Debugging;

[Serializable]
internal sealed class TerrainDebug
{
	public enum MaskType
	{
		None,
		PvsCascadeLod0,
		PvsCascadeLod1,
		Custom
	}

	public enum MaskPositionType
	{
		Manual,
		Auto
	}

	public Color GradientMinColor = Color.red;

	public Color GradientMaxColor = Color.green;

	public Color BackColor = new Color(0f, 0f, 0f, 0.75f);

	public float Opacity = 1f;

	public int LayerCount = 16;

	public float LayerWeightMin;

	public float LayerWeightMax = 1f;

	public int PvsHeatmapCountThreshold = 8;

	public MaskType SplatMapMaskType;

	public MaskPositionType SplatMaskPositionType = MaskPositionType.Auto;

	public float SplatMapMaskCustomRadius = 10f;

	public Vector3 SplatMapMaskCustomPosition;

	[NonSerialized]
	public bool Enabled;

	[NonSerialized]
	public TerrainDebugMode DebugMode;

	[NonSerialized]
	public int LayerIndex = -1;

	[NonSerialized]
	public bool HeatmapRefreshRequested;

	public Vector3 ResolveSplatMapMaskPosition()
	{
		return SplatMaskPositionType switch
		{
			MaskPositionType.Manual => SplatMapMaskCustomPosition, 
			MaskPositionType.Auto => TerrainStreamingFeedback.FeedbackPosition, 
			_ => Vector3.zero, 
		};
	}

	public float ResolveSplatMapMaskRadius()
	{
		switch (SplatMapMaskType)
		{
		case MaskType.PvsCascadeLod0:
		case MaskType.PvsCascadeLod1:
		{
			if (GraphicsSettings.TryGetRenderPipelineSettings<VirtualTerrainEditorGlobalSettings>(out var settings))
			{
				if (SplatMapMaskType != MaskType.PvsCascadeLod1)
				{
					return (float)settings.TerrainLayerPvsCellSize / 2f + (float)settings.TerrainLayerPvsExtentRadiusLod0 + 1f;
				}
				return (float)settings.TerrainLayerPvsCellSize / 2f + (float)settings.TerrainLayerPvsExtentRadiusLod1 + 1f;
			}
			return 0f;
		}
		case MaskType.Custom:
			return SplatMapMaskCustomRadius;
		default:
			return 0f;
		}
	}
}
