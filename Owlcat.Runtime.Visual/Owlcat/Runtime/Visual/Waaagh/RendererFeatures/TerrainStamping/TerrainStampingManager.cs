using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.GPUDrivenBRG;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.TerrainStamping;

public class TerrainStampingManager : IDisposable
{
	public enum BrushMode
	{
		Accumulate,
		Render
	}

	public struct ChunkData
	{
		public int2 Index;

		public GPUDrivenIndexAllocator.IndexAllocation PoolAllocation;

		public float MaxValue;

		public float AccumulatedValue;

		public float ComputeTotalFade()
		{
			return math.saturate(MaxValue + AccumulatedValue);
		}
	}

	private static class ShaderIDs
	{
		public static readonly int _BlendOp = Shader.PropertyToID("_BlendOp");
	}

	private static TerrainStampingManager s_Instance;

	private readonly TerrainStampingManagerParameters m_Parameters;

	private readonly GPUDrivenIndexAllocator m_TexturePoolAllocator;

	private NativeHashMap<int2, ChunkData> m_ActiveChunkData;

	private NativeHashSet<int2> m_ChunksInZones;

	private double? m_LastUpdateTime;

	private bool m_ZonesAreDirty;

	public float ChunkSize => m_Parameters.ChunkSize;

	public int2 ChunksMinIndex { get; private set; }

	public int2 ChunksMaxIndex { get; private set; }

	public static bool IsInitialized => s_Instance != null;

	public Material BrushMaterial { get; private set; }

	public Material FadeMaterial { get; private set; }

	public Material BakeNormalsMaterial { get; private set; }

	public RenderTexture StampingTexturePool { get; }

	public RenderTexture BakedNormalsPool { get; }

	private TerrainStampingManager(TerrainStampingManagerParameters parameters)
	{
		m_Parameters = parameters;
		BrushMaterial = CoreUtils.CreateEngineMaterial(parameters.BrushShader);
		BrushMaterial.SetFloat(ShaderIDs._BlendOp, 4f);
		FadeMaterial = CoreUtils.CreateEngineMaterial(parameters.BrushShader);
		FadeMaterial.SetFloat(ShaderIDs._BlendOp, 2f);
		BakeNormalsMaterial = CoreUtils.CreateEngineMaterial(parameters.BakeNormalsShader);
		m_ActiveChunkData = new NativeHashMap<int2, ChunkData>(parameters.VisibleRegionMaxLengthInChunks * parameters.VisibleRegionMaxLengthInChunks, Allocator.Persistent);
		m_ChunksInZones = new NativeHashSet<int2>(parameters.VisibleRegionMaxLengthInChunks * parameters.VisibleRegionMaxLengthInChunks, Allocator.Persistent);
		int resolution = (int)parameters.Resolution;
		RenderTextureDescriptor desc = new RenderTextureDescriptor(resolution, resolution, GraphicsFormat.R16_UNorm, GraphicsFormat.None, 1)
		{
			dimension = TextureDimension.Tex2DArray,
			volumeDepth = parameters.ChunkTexturesCapacity
		};
		StampingTexturePool = new RenderTexture(desc)
		{
			filterMode = FilterMode.Bilinear,
			hideFlags = HideFlags.HideAndDontSave,
			name = "Terrain Stamping Texture Pool"
		};
		int bakedNormalsResolution = (int)parameters.BakedNormalsResolution;
		RenderTextureDescriptor desc2 = new RenderTextureDescriptor(bakedNormalsResolution, bakedNormalsResolution, GraphicsFormat.R8G8_UNorm, GraphicsFormat.None, 1)
		{
			dimension = TextureDimension.Tex2DArray,
			volumeDepth = parameters.ChunkTexturesCapacity
		};
		BakedNormalsPool = new RenderTexture(desc2)
		{
			filterMode = FilterMode.Bilinear,
			hideFlags = HideFlags.HideAndDontSave,
			name = "Terrain Stamping Baked Normals Pool"
		};
		m_TexturePoolAllocator = new GPUDrivenIndexAllocator(parameters.ChunkTexturesCapacity);
		TerrainStampingZoneContainer.Changed += OnZonesChanged;
		m_ZonesAreDirty = true;
	}

	public void Dispose()
	{
		TerrainStampingZoneContainer.Changed -= OnZonesChanged;
		CoreUtils.Destroy(BrushMaterial);
		BrushMaterial = null;
		CoreUtils.Destroy(FadeMaterial);
		FadeMaterial = null;
		CoreUtils.Destroy(StampingTexturePool);
		CoreUtils.Destroy(BakedNormalsPool);
		m_TexturePoolAllocator.Dispose();
		m_ActiveChunkData.Dispose();
		m_ChunksInZones.Dispose();
	}

	public void OnValidate()
	{
	}

	private void OnZonesChanged()
	{
		m_ZonesAreDirty = true;
	}

	public NativeArray<ChunkData> GetActiveChunks(Allocator allocator)
	{
		return m_ActiveChunkData.GetValueArray(allocator);
	}

	public float GetNextDeltaTime()
	{
		double timeAsDouble = Time.timeAsDouble;
		float result = (m_LastUpdateTime.HasValue ? ((float)(timeAsDouble - m_LastUpdateTime.Value)) : 0f);
		m_LastUpdateTime = timeAsDouble;
		return result;
	}

	public void OnSetup()
	{
		if (m_ZonesAreDirty)
		{
			m_ChunksInZones.Clear();
			List<OwlcatTerrainStampingZone> value;
			using (ListPool<OwlcatTerrainStampingZone>.Get(out value))
			{
				TerrainStampingZoneContainer.Get(value);
				foreach (OwlcatTerrainStampingZone item in value)
				{
					item.GetWorldAABB(out var center, out var extents);
					float2 boundsMin = center.xz - extents.xz;
					float2 boundsMax = center.xz + extents.xz;
					TerrainStampingUtils.ComputeMinMaxChunkRange(boundsMin, boundsMax, m_Parameters.ChunkSize, out var chunksMin, out var chunksMax);
					for (int i = chunksMin.x; i < chunksMax.x; i++)
					{
						for (int j = chunksMin.y; j < chunksMax.y; j++)
						{
							m_ChunksInZones.Add(new int2(i, j));
						}
					}
				}
			}
			foreach (int2 item2 in m_ActiveChunkData.GetKeyArray(Allocator.Temp))
			{
				if (!m_ChunksInZones.Contains(item2))
				{
					FreeChunkData(item2);
				}
			}
			m_ZonesAreDirty = false;
		}
		foreach (int2 item3 in m_ActiveChunkData.GetKeyArray(Allocator.Temp))
		{
			if (m_ActiveChunkData[item3].ComputeTotalFade() == 0f)
			{
				FreeChunkData(item3);
			}
		}
	}

	public ChunkData GetOrAllocateChunkData(int2 chunkIndex, out bool isNew)
	{
		ChunkData result;
		if (!m_ChunksInZones.Contains(chunkIndex))
		{
			isNew = false;
			result = default(ChunkData);
			result.PoolAllocation = GPUDrivenIndexAllocator.IndexAllocation.Invalid;
			return result;
		}
		if (m_ActiveChunkData.TryGetValue(chunkIndex, out var item))
		{
			isNew = false;
			return item;
		}
		GPUDrivenIndexAllocator.IndexAllocation poolAllocation = m_TexturePoolAllocator.Allocate();
		if (poolAllocation.Index == -1)
		{
			isNew = false;
			Debug.LogError("Terrain Stamping pool allocation failure. Out of memory.");
			result = default(ChunkData);
			result.PoolAllocation = GPUDrivenIndexAllocator.IndexAllocation.Invalid;
			return result;
		}
		result = default(ChunkData);
		result.Index = chunkIndex;
		result.PoolAllocation = poolAllocation;
		ChunkData chunkData = result;
		m_ActiveChunkData[chunkIndex] = chunkData;
		isNew = true;
		return chunkData;
	}

	private void FreeChunkData(int2 chunkIndex)
	{
		m_ActiveChunkData.TryGetValue(chunkIndex, out var item);
		m_TexturePoolAllocator.Free(item.PoolAllocation);
		m_ActiveChunkData.Remove(chunkIndex);
	}

	public static bool TryGetInstance(out TerrainStampingManager terrainStampingManager)
	{
		terrainStampingManager = s_Instance;
		return s_Instance != null;
	}

	public static void Init(WaaaghPipeline pipeline, TerrainStampingManagerParameters parameters)
	{
		s_Instance = new TerrainStampingManager(parameters);
	}

	public static void Cleanup()
	{
		if (s_Instance != null)
		{
			s_Instance.Dispose();
			s_Instance = null;
		}
	}

	public void UpdateChunkExtents(float2 chunksMin, float2 chunksMax)
	{
		float chunkSize = m_Parameters.ChunkSize;
		TerrainStampingUtils.ComputeMinMaxChunkRange(chunksMin, chunksMax, chunkSize, out var chunksMin2, out var chunksMax2);
		int2 @int = (int2)math.floor((float2)(chunksMax2 + chunksMin2) * 0.5f);
		int2 y = chunksMax2 - chunksMin2;
		y = math.min(m_Parameters.VisibleRegionMaxLengthInChunks, y);
		ChunksMinIndex = @int - y / 2;
		ChunksMaxIndex = @int + y / 2 + y % 2;
	}

	public bool IsChunkActive(int2 chunkIndex)
	{
		return m_ActiveChunkData.ContainsKey(chunkIndex);
	}

	public bool TryGetChunkData(int2 chunkIndex, out ChunkData chunkData)
	{
		return m_ActiveChunkData.TryGetValue(chunkIndex, out chunkData);
	}

	public bool IsInZone(int2 chunkIndex)
	{
		return m_ChunksInZones.Contains(chunkIndex);
	}

	public void ApplyBrushVirtually(int2 chunkIndex, float strength, out float physicalBrushStrength, BrushMode brushMode)
	{
		physicalBrushStrength = 0f;
		if (m_ActiveChunkData.TryGetValue(chunkIndex, out var item))
		{
			float num = item.AccumulatedValue + strength;
			if (brushMode == BrushMode.Render)
			{
				item.AccumulatedValue = 0f;
				item.MaxValue += num;
				item.MaxValue = math.saturate(item.MaxValue);
				physicalBrushStrength = num;
			}
			else
			{
				item.AccumulatedValue = num;
			}
			m_ActiveChunkData[chunkIndex] = item;
		}
	}
}
