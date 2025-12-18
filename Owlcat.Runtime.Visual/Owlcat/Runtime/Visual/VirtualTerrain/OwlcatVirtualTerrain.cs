using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Owlcat.Runtime.Visual.Terrain;
using Owlcat.Runtime.Visual.VirtualTerrain.Streaming;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.VirtualTerrain;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(UnityEngine.Terrain))]
public sealed class OwlcatVirtualTerrain : OwlcatTerrainBase, IFeedbackProvider, IStreamingListener
{
	public delegate void PropertyBlockUpdateEventHandler(OwlcatVirtualTerrain sender, MaterialPropertyBlock properties);

	private const int kLayerCountMax = 64;

	private static readonly Vector4[] s_RedirectionArray = new Vector4[384];

	private static readonly Vector4[] s_StampingWeightsArray = new Vector4[32];

	[SerializeField]
	internal TerrainData m_TerrainData;

	[SerializeField]
	internal Texture2DArray m_TerrainDataSplatMap;

	[SerializeField]
	internal TerrainLayerGuid[] m_LayerDataGuids = Array.Empty<TerrainLayerGuid>();

	[SerializeField]
	internal Vector4[] m_LayerDataStampingWeights = Array.Empty<Vector4>();

	[SerializeField]
	internal TerrainLayerPVS m_LayerPVS;

	private readonly List<int> m_PrimaryTerrainLayerIds = new List<int>();

	private readonly List<int> m_SecondaryTerrainLayerIds = new List<int>();

	private MaterialPropertyBlock m_MaterialPropertyBlock;

	private bool m_TerrainMaterialInvalid;

	private OwlcatTerrainPalette m_PrimaryPalette;

	private OwlcatTerrainPalette m_SecondaryPalette;

	public override Texture2DArray SplatArray => m_TerrainDataSplatMap;

	public OwlcatTerrainPalette PrimaryPalette
	{
		get
		{
			return m_PrimaryPalette;
		}
		set
		{
			if (m_PrimaryPalette != value)
			{
				m_PrimaryPalette = value;
				Invalidate();
			}
		}
	}

	public OwlcatTerrainPalette SecondaryPalette
	{
		get
		{
			return m_SecondaryPalette;
		}
		set
		{
			if (m_SecondaryPalette != value)
			{
				m_SecondaryPalette = value;
				Invalidate();
			}
		}
	}

	public event PropertyBlockUpdateEventHandler TerrainSplatMaterialPropertyBlockUpdating;

	public event PropertyBlockUpdateEventHandler TerrainSplatMaterialPropertyBlockUpdated;

	public event Action<Material> PopulatingMaterialProperties;

	[UsedImplicitly]
	private void OnEnable()
	{
		TerrainStreamingFeedbackFeature.RegisterFeedbackProvider(this);
		TerrainStreamingSystem.RegisterListener(this);
		Invalidate();
	}

	[UsedImplicitly]
	private void OnDisable()
	{
		TerrainStreamingFeedbackFeature.UnregisterFeedbackProvider(this);
		TerrainStreamingSystem.UnregisterListener(this);
	}

	[UsedImplicitly]
	private void LateUpdate()
	{
		if (m_TerrainMaterialInvalid)
		{
			m_TerrainMaterialInvalid = false;
			UpdateTerrainMaterial();
		}
	}

	public void Invalidate()
	{
		RefreshTerrainLayerIds();
		m_TerrainMaterialInvalid = true;
	}

	private void UpdateTerrainMaterial()
	{
		if (m_TerrainData == null || !TryGetComponent<UnityEngine.Terrain>(out var component))
		{
			return;
		}
		if (m_MaterialPropertyBlock == null)
		{
			m_MaterialPropertyBlock = new MaterialPropertyBlock();
		}
		try
		{
			PopulateMaterialProperties(m_MaterialPropertyBlock);
			this.TerrainSplatMaterialPropertyBlockUpdating?.Invoke(this, m_MaterialPropertyBlock);
			component.SetSplatMaterialPropertyBlock(m_MaterialPropertyBlock);
			this.TerrainSplatMaterialPropertyBlockUpdated?.Invoke(this, m_MaterialPropertyBlock);
		}
		finally
		{
			m_MaterialPropertyBlock.Clear();
		}
	}

	public void PopulateMaterialProperties(MaterialPropertyBlock properties)
	{
		PopulateRedirection(s_RedirectionArray);
		PopulateStampingWeights(s_StampingWeightsArray);
		properties.SetVector(VirtualTerrainShader.Properties.LayerParams, GetLayerParams());
		properties.SetVectorArray(VirtualTerrainShader.Properties.Redirection, s_RedirectionArray);
		properties.SetVectorArray(VirtualTerrainShader.Properties.StampingWeights, s_StampingWeightsArray);
		properties.SetFloat(VirtualTerrainShader.Properties.TerrainMaxHeight, (m_TerrainData != null) ? (1f / m_TerrainData.size.x) : 1f);
		Texture2DArray terrainDataSplatMap = m_TerrainDataSplatMap;
		if (terrainDataSplatMap != null)
		{
			properties.SetTexture(VirtualTerrainShader.Properties.SplatMap, terrainDataSplatMap);
		}
	}

	public void PopulateMaterialProperties(Material material)
	{
		PopulateRedirection(s_RedirectionArray);
		PopulateStampingWeights(s_StampingWeightsArray);
		material.SetVector(VirtualTerrainShader.Properties.LayerParams, GetLayerParams());
		material.SetVectorArray(VirtualTerrainShader.Properties.Redirection, s_RedirectionArray);
		material.SetVectorArray(VirtualTerrainShader.Properties.StampingWeights, s_StampingWeightsArray);
		material.SetFloat(VirtualTerrainShader.Properties.TerrainMaxHeight, (m_TerrainData != null) ? (1f / m_TerrainData.size.x) : 1f);
		if (m_TerrainDataSplatMap != null)
		{
			material.SetTexture(VirtualTerrainShader.Properties.SplatMap, m_TerrainDataSplatMap);
		}
		this.PopulatingMaterialProperties?.Invoke(material);
	}

	private Vector4 GetLayerParams()
	{
		int num = ((m_TerrainDataSplatMap != null) ? m_TerrainDataSplatMap.depth : 0);
		VirtualTerrainGlobalSettings settings;
		float y = (GraphicsSettings.TryGetRenderPipelineSettings<VirtualTerrainGlobalSettings>(out settings) ? settings.TerrainLayerTiling : VirtualTerrainGlobalSettings.Default.TerrainLayerTiling) * m_TerrainData.size.x * 0.25f;
		return new Vector4(num, y, 0f, 0f);
	}

	private void RefreshTerrainLayerIds()
	{
		m_PrimaryTerrainLayerIds.Clear();
		m_SecondaryTerrainLayerIds.Clear();
		if (m_PrimaryPalette != null)
		{
			m_PrimaryPalette.GetTerrainLayerIds(m_PrimaryTerrainLayerIds);
		}
		else
		{
			int num = Mathf.Min(64, m_LayerDataGuids.Length);
			for (int i = 0; i < num; i++)
			{
				m_PrimaryTerrainLayerIds.Add(TerrainLayerId.GetTerrainLayerId(m_LayerDataGuids[i]));
			}
		}
		if (m_SecondaryPalette != null)
		{
			m_SecondaryPalette.GetTerrainLayerIds(m_SecondaryTerrainLayerIds);
		}
	}

	private unsafe static void PopulateRedirection(List<int> layerIds, Span<Vector4> redirections)
	{
		if (layerIds.Count > 0)
		{
			TerrainStreamingSystem.PopulateRedirectionBuffer(layerIds, redirections, 64);
			int num = (64 - layerIds.Count) / layerIds.Count;
			if (num <= 0)
			{
				return;
			}
			int size = layerIds.Count * 16;
			fixed (Vector4* ptr = redirections)
			{
				for (int i = 0; i < 3; i++)
				{
					Vector4* ptr2 = ptr + i * 64;
					UnsafeUtility.MemCpyReplicate(ptr2 + layerIds.Count, ptr2, size, num);
				}
			}
		}
		else
		{
			redirections.Fill(default(Vector4));
		}
	}

	private void PopulateRedirection(Span<Vector4> redirectionBuffer)
	{
		Span<Vector4> redirections = redirectionBuffer.Slice(0, 192);
		Span<Vector4> redirections2 = redirectionBuffer.Slice(192, 192);
		PopulateRedirection(m_PrimaryTerrainLayerIds, redirections);
		PopulateRedirection(m_SecondaryTerrainLayerIds, redirections2);
	}

	private void PopulateStampingWeights(Span<Vector4> layerWeights)
	{
		if (m_PrimaryPalette != null)
		{
			m_PrimaryPalette.LayerDataStampingWeights.CopyTo(layerWeights.Slice(0, 16));
		}
		else
		{
			m_LayerDataStampingWeights.CopyTo(layerWeights.Slice(0, 16));
		}
		if (m_SecondaryPalette != null)
		{
			m_SecondaryPalette.LayerDataStampingWeights.CopyTo(layerWeights.Slice(16, 16));
		}
	}

	void IFeedbackProvider.GetFeedback(Vector3 probePosition, Span<int> layerLods)
	{
		m_LayerPVS.GetVisibleLayerMasks(new float2(probePosition.x, probePosition.z), out var mask, out var mask2);
		for (int i = 0; i < m_PrimaryTerrainLayerIds.Count; i++)
		{
			int num = (((mask & (ulong)(1L << i)) == 0L) ? (((mask2 & (ulong)(1L << i)) != 0L) ? 1 : 2) : 0);
			if (layerLods[m_PrimaryTerrainLayerIds[i]] > num)
			{
				layerLods[m_PrimaryTerrainLayerIds[i]] = num;
			}
		}
		for (int j = 0; j < m_SecondaryTerrainLayerIds.Count; j++)
		{
			int num2 = (((mask & (ulong)(1L << j)) == 0L) ? (((mask2 & (ulong)(1L << j)) != 0L) ? 1 : 2) : 0);
			if (layerLods[m_SecondaryTerrainLayerIds[j]] > num2)
			{
				layerLods[m_SecondaryTerrainLayerIds[j]] = num2;
			}
		}
	}

	void IStreamingListener.OnAtlasChanged()
	{
		Invalidate();
	}
}
