using System;
using System.Collections.Generic;
using Owlcat.Runtime.Core.Allocators;
using Owlcat.Runtime.Visual.Waaagh;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Lighting;

internal struct ReflectionProbeManager : IDisposable
{
	private struct CachedProbe
	{
		public uint UpdateCount;

		public Hash128 ImageContentsHash;

		public int Size;

		public int MipCount;

		public unsafe fixed int DataIndices[7];

		public unsafe fixed int Levels[7];

		public Texture Texture;

		public int LastUsed;

		public Vector4 HDRData;
	}

	private static class ShaderProperties
	{
		public static readonly int BoxMin = Shader.PropertyToID("waaagh_ReflProbes_BoxMin");

		public static readonly int BoxMax = Shader.PropertyToID("waaagh_ReflProbes_BoxMax");

		public static readonly int ProbePosition = Shader.PropertyToID("waaagh_ReflProbes_ProbePosition");

		public static readonly int MipScaleOffset = Shader.PropertyToID("waaagh_ReflProbes_MipScaleOffset");

		public static readonly int Count = Shader.PropertyToID("waaagh_ReflProbes_Count");

		public static readonly int Atlas = Shader.PropertyToID("waaagh_ReflProbes_Atlas");
	}

	private static class Profiling
	{
		public static ProfilingSampler UpdateReflectionProbeAtlas = new ProfilingSampler("UpdateReflectionProbeAtlas");
	}

	private int2 m_Resolution;

	private RenderTexture m_AtlasTexture1;

	private BuddyAllocator m_AtlasAllocator;

	private Dictionary<int, CachedProbe> m_Cache;

	private Dictionary<int, int> m_WarningCache;

	private List<int> m_NeedsUpdate;

	private List<int> m_NeedsRemove;

	private Dictionary<int, int> m_VisibleProbeToGpuDataIndex;

	private Vector4[] m_BoxMax;

	private Vector4[] m_BoxMin;

	private Vector4[] m_ProbePosition;

	private Vector4[] m_MipScaleOffset;

	private const int kMaxMipCount = 7;

	private const string kReflectionProbeAtlasName = "Waaagh Reflection Probe Atlas";

	public RenderTexture atlasRT { get; private set; }

	public RTHandle atlasRTHandle { get; private set; }

	public static ReflectionProbeManager Create()
	{
		ReflectionProbeManager result = default(ReflectionProbeManager);
		result.Init();
		return result;
	}

	private void Init()
	{
		int maxVisibleReflectionProbes = WaaaghPipeline.MaxVisibleReflectionProbes;
		m_Resolution = 1;
		GraphicsFormat graphicsFormat = GraphicsFormat.B10G11R11_UFloatPack32;
		if (!SystemInfo.IsFormatSupported(graphicsFormat, GraphicsFormatUsage.Render))
		{
			graphicsFormat = GraphicsFormat.R16G16B16A16_SFloat;
		}
		atlasRT = new RenderTexture(new RenderTextureDescriptor
		{
			width = m_Resolution.x,
			height = m_Resolution.y,
			volumeDepth = 1,
			dimension = TextureDimension.Tex2D,
			graphicsFormat = graphicsFormat,
			useMipMap = false,
			msaaSamples = 1
		});
		atlasRT.name = "Waaagh Reflection Probe Atlas";
		atlasRT.filterMode = FilterMode.Bilinear;
		atlasRT.hideFlags = HideFlags.HideAndDontSave;
		atlasRT.Create();
		atlasRTHandle = RTHandles.Alloc(atlasRT, transferOwnership: true);
		m_AtlasTexture1 = new RenderTexture(atlasRT.descriptor);
		m_AtlasTexture1.name = "Waaagh Reflection Probe Atlas";
		m_AtlasTexture1.filterMode = FilterMode.Bilinear;
		m_AtlasTexture1.hideFlags = HideFlags.HideAndDontSave;
		m_AtlasAllocator = new BuddyAllocator(math.floorlog2(SystemInfo.maxTextureSize) - 2, 2);
		m_Cache = new Dictionary<int, CachedProbe>(maxVisibleReflectionProbes);
		m_WarningCache = new Dictionary<int, int>(maxVisibleReflectionProbes);
		m_NeedsUpdate = new List<int>(maxVisibleReflectionProbes);
		m_NeedsRemove = new List<int>(maxVisibleReflectionProbes);
		m_VisibleProbeToGpuDataIndex = new Dictionary<int, int>(maxVisibleReflectionProbes);
		m_BoxMax = new Vector4[maxVisibleReflectionProbes];
		m_BoxMin = new Vector4[maxVisibleReflectionProbes];
		m_ProbePosition = new Vector4[maxVisibleReflectionProbes];
		m_MipScaleOffset = new Vector4[maxVisibleReflectionProbes * 7];
	}

	public bool TryMapVisibleProbeToGpuDataIndex(int visibleProbeIndex, out int dataIndex)
	{
		return m_VisibleProbeToGpuDataIndex.TryGetValue(visibleProbeIndex, out dataIndex);
	}

	public unsafe void UpdateGpuData(CommandBuffer cmd, ref CullingResults cullResults)
	{
		NativeArray<VisibleReflectionProbe> visibleReflectionProbes = cullResults.visibleReflectionProbes;
		int num = math.min(visibleReflectionProbes.Length, WaaaghPipeline.MaxVisibleReflectionProbes);
		int renderedFrameCount = Time.renderedFrameCount;
		int key;
		foreach (KeyValuePair<int, CachedProbe> item3 in m_Cache)
		{
			item3.Deconstruct(out key, out var value);
			int item = key;
			CachedProbe cachedProbe = value;
			if (Math.Abs(cachedProbe.LastUsed - renderedFrameCount) <= 1 && (bool)cachedProbe.Texture && cachedProbe.Size == cachedProbe.Texture.width)
			{
				continue;
			}
			m_NeedsRemove.Add(item);
			for (int i = 0; i < 7; i++)
			{
				if (cachedProbe.DataIndices[i] != -1)
				{
					m_AtlasAllocator.Free(new BuddyAllocation(cachedProbe.Levels[i], cachedProbe.DataIndices[i]));
				}
			}
		}
		foreach (int item4 in m_NeedsRemove)
		{
			m_Cache.Remove(item4);
		}
		m_NeedsRemove.Clear();
		foreach (KeyValuePair<int, int> item5 in m_WarningCache)
		{
			item5.Deconstruct(out key, out var value2);
			int item2 = key;
			if (Math.Abs(value2 - renderedFrameCount) > 1)
			{
				m_NeedsRemove.Add(item2);
			}
		}
		foreach (int item6 in m_NeedsRemove)
		{
			m_WarningCache.Remove(item6);
		}
		m_NeedsRemove.Clear();
		bool flag = false;
		int2 @int = math.int2(0, 0);
		for (int j = 0; j < num; j++)
		{
			VisibleReflectionProbe visibleReflectionProbe = visibleReflectionProbes[j];
			ReflectionProbe reflectionProbe = visibleReflectionProbe.reflectionProbe;
			if (!reflectionProbe)
			{
				continue;
			}
			Texture texture = visibleReflectionProbe.texture;
			int instanceID = reflectionProbe.GetInstanceID();
			CachedProbe value3;
			bool flag2 = m_Cache.TryGetValue(instanceID, out value3);
			if (!texture)
			{
				continue;
			}
			if (!flag2)
			{
				value3.Size = texture.width;
				int num2 = math.ceillog2(value3.Size * 4) + 1;
				int num3 = m_AtlasAllocator.levelCount + 2 - num2;
				value3.MipCount = math.min(num2, 7);
				value3.Texture = texture;
				int k;
				for (k = 0; k < value3.MipCount; k++)
				{
					int num4 = math.min(num3 + k, m_AtlasAllocator.levelCount - 1);
					if (!m_AtlasAllocator.TryAllocate(num4, out var allocation))
					{
						break;
					}
					value3.Levels[k] = allocation.Level;
					value3.DataIndices[k] = allocation.Index;
					int4 int2 = (int4)(GetScaleOffset(num4, allocation.Index, includePadding: true, yflip: false) * m_Resolution.xyxy);
					@int = math.max(@int, int2.zw + int2.xy);
				}
				if (k < value3.MipCount)
				{
					if (!m_WarningCache.ContainsKey(instanceID))
					{
						flag = true;
					}
					m_WarningCache[instanceID] = renderedFrameCount;
					for (int l = 0; l < k; l++)
					{
						m_AtlasAllocator.Free(new BuddyAllocation(value3.Levels[l], value3.DataIndices[l]));
					}
					for (int m = 0; m < 7; m++)
					{
						value3.DataIndices[m] = -1;
					}
					continue;
				}
				for (; k < 7; k++)
				{
					value3.DataIndices[k] = -1;
				}
			}
			if ((!flag2 || value3.UpdateCount != texture.updateCount) | (value3.HDRData != visibleReflectionProbe.hdrData))
			{
				value3.UpdateCount = texture.updateCount;
				m_NeedsUpdate.Add(instanceID);
			}
			if (reflectionProbe.refreshMode == ReflectionProbeRefreshMode.EveryFrame)
			{
				value3.LastUsed = -1;
			}
			else
			{
				value3.LastUsed = renderedFrameCount;
			}
			value3.HDRData = visibleReflectionProbe.hdrData;
			m_Cache[instanceID] = value3;
		}
		if (math.any(m_Resolution < @int))
		{
			@int = math.max(m_Resolution, math.ceilpow2(@int));
			RenderTextureDescriptor descriptor = atlasRT.descriptor;
			descriptor.width = @int.x;
			descriptor.height = @int.y;
			m_AtlasTexture1.width = @int.x;
			m_AtlasTexture1.height = @int.y;
			m_AtlasTexture1.Create();
			if (atlasRT.width != 1)
			{
				if (SystemInfo.copyTextureSupport != 0)
				{
					Graphics.CopyTexture(atlasRT, 0, 0, 0, 0, m_Resolution.x, m_Resolution.y, m_AtlasTexture1, 0, 0, 0, 0);
				}
				else
				{
					Graphics.Blit(atlasRT, m_AtlasTexture1, (float2)m_Resolution / (float2)@int, Vector2.zero);
				}
			}
			atlasRT.Release();
			RenderTexture atlasTexture = m_AtlasTexture1;
			RenderTexture atlasTexture2 = atlasRT;
			RenderTexture renderTexture2 = (atlasRT = atlasTexture);
			m_AtlasTexture1 = atlasTexture2;
			m_Resolution = @int;
		}
		m_VisibleProbeToGpuDataIndex.Clear();
		int num5 = 0;
		for (int n = 0; n < num; n++)
		{
			VisibleReflectionProbe visibleReflectionProbe2 = visibleReflectionProbes[n];
			ReflectionProbe reflectionProbe2 = visibleReflectionProbe2.reflectionProbe;
			if (!reflectionProbe2)
			{
				num5++;
				continue;
			}
			int instanceID2 = reflectionProbe2.GetInstanceID();
			int num6 = n - num5;
			if (!m_Cache.TryGetValue(instanceID2, out var value4) || !visibleReflectionProbe2.texture)
			{
				num5++;
				continue;
			}
			m_BoxMax[num6] = new Vector4(visibleReflectionProbe2.bounds.max.x, visibleReflectionProbe2.bounds.max.y, visibleReflectionProbe2.bounds.max.z, visibleReflectionProbe2.blendDistance);
			m_BoxMin[num6] = new Vector4(visibleReflectionProbe2.bounds.min.x, visibleReflectionProbe2.bounds.min.y, visibleReflectionProbe2.bounds.min.z, visibleReflectionProbe2.importance);
			m_ProbePosition[num6] = new Vector4(visibleReflectionProbe2.localToWorldMatrix.m03, visibleReflectionProbe2.localToWorldMatrix.m13, visibleReflectionProbe2.localToWorldMatrix.m23, (visibleReflectionProbe2.isBoxProjection ? 1 : (-1)) * value4.MipCount);
			for (int num7 = 0; num7 < value4.MipCount; num7++)
			{
				m_MipScaleOffset[num6 * 7 + num7] = GetScaleOffset(value4.Levels[num7], value4.DataIndices[num7], includePadding: false, yflip: false);
			}
			m_VisibleProbeToGpuDataIndex.Add(n, num6);
		}
		if (flag)
		{
			Debug.LogWarning("A number of reflection probes have been skipped due to the reflection probe atlas being full.\nTo fix this, you can decrease the number or resolution of probes.");
		}
		using (new ProfilingScope(cmd, Profiling.UpdateReflectionProbeAtlas))
		{
			cmd.SetRenderTarget(atlasRT);
			foreach (int item7 in m_NeedsUpdate)
			{
				CachedProbe cachedProbe2 = m_Cache[item7];
				for (int num8 = 0; num8 < cachedProbe2.MipCount; num8++)
				{
					int num9 = cachedProbe2.Levels[num8];
					int dataIndex = cachedProbe2.DataIndices[num8];
					float4 scaleOffset = GetScaleOffset(num9, dataIndex, includePadding: true, !SystemInfo.graphicsUVStartsAtTop);
					int num10 = (1 << m_AtlasAllocator.levelCount + 1 - num9) - 2;
					Blitter.BlitCubeToOctahedral2DQuadWithPadding(cmd, cachedProbe2.Texture, new Vector2(num10, num10), scaleOffset, num8, bilinear: true, 2, cachedProbe2.HDRData);
				}
			}
			cmd.SetGlobalVectorArray(ShaderProperties.BoxMin, m_BoxMin);
			cmd.SetGlobalVectorArray(ShaderProperties.BoxMax, m_BoxMax);
			cmd.SetGlobalVectorArray(ShaderProperties.ProbePosition, m_ProbePosition);
			cmd.SetGlobalVectorArray(ShaderProperties.MipScaleOffset, m_MipScaleOffset);
			cmd.SetGlobalFloat(ShaderProperties.Count, num - num5);
			cmd.SetGlobalTexture(ShaderProperties.Atlas, atlasRT);
		}
		m_NeedsUpdate.Clear();
	}

	private float4 GetScaleOffset(int level, int dataIndex, bool includePadding, bool yflip)
	{
		int num = 1 << m_AtlasAllocator.levelCount + 1 - level;
		uint2 @uint = SpaceFillingCurves.DecodeMorton2D((uint)dataIndex);
		float2 xy = (float)(num - ((!includePadding) ? 2 : 0)) / (float2)m_Resolution;
		float2 zw = ((float2)@uint * (float)num + ((!includePadding) ? 1 : 0)) / m_Resolution;
		if (yflip)
		{
			zw.y = 1f - zw.y - xy.y;
		}
		return math.float4(xy, zw);
	}

	public void Dispose()
	{
		if ((bool)atlasRT)
		{
			atlasRT.Release();
			atlasRTHandle.Release();
		}
		UnityEngine.Object.DestroyImmediate(atlasRT);
		UnityEngine.Object.DestroyImmediate(m_AtlasTexture1);
		this = default(ReflectionProbeManager);
	}
}
