using System;
using System.Collections.Generic;
using System.IO;
using Owlcat.Runtime.Visual.Waaagh;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.ShaderMetadata;

public class GPUDrivenShaderMetadataRepository : IDisposable
{
	private struct BakedCacheKey : IEquatable<BakedCacheKey>
	{
		public string ShaderName;

		public string ShaderGraphGuid;

		public bool Equals(BakedCacheKey other)
		{
			if (ShaderName == other.ShaderName)
			{
				return ShaderGraphGuid == other.ShaderGraphGuid;
			}
			return false;
		}

		[BurstDiscard]
		public override bool Equals(object obj)
		{
			if (obj is BakedCacheKey other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (((ShaderName != null) ? ShaderName.GetHashCode() : 0) * 397) ^ ((ShaderGraphGuid != null) ? ShaderGraphGuid.GetHashCode() : 0);
		}

		public override string ToString()
		{
			return ShaderName + " + (" + (string.IsNullOrWhiteSpace(ShaderGraphGuid) ? "GUID N\\A" : ShaderGraphGuid) + ")";
		}
	}

	public struct ShaderPropertyData
	{
		public int PropertyIndex;

		public GPUDrivenRenderer.PropertyData PropertyData;

		public bool PerInstance;

		public int? TilingOffsetTextureNameID;

		public bool BreaksBatching;
	}

	public const string kBakedMetadataLocalPath = "GPUDrivenBRG/BakedShaderMetadata.shadermetadata";

	private static readonly HashSet<int> s_KnownPerInstanceProperties = new HashSet<int>
	{
		ShaderPropertyId._OccluderObjectOpacity,
		ShaderPropertyId._OccluderObjectFullOpacity
	};

	private readonly Dictionary<BakedCacheKey, GPUDrivenShaderMetadata> m_BakedShaderMetadataCache = new Dictionary<BakedCacheKey, GPUDrivenShaderMetadata>();

	private readonly Dictionary<Shader, GPUDrivenShaderMetadata> m_MetadataCache = new Dictionary<Shader, GPUDrivenShaderMetadata>();

	private readonly Dictionary<Shader, NativeList<ShaderPropertyData>> m_PropertyLayouts = new Dictionary<Shader, NativeList<ShaderPropertyData>>();

	public GPUDrivenShaderMetadataRepository()
	{
		if (!Application.isEditor)
		{
			WarmCacheUpFromBakedMetadata();
		}
	}

	public void Dispose()
	{
		m_MetadataCache.Clear();
		foreach (NativeList<ShaderPropertyData> value in m_PropertyLayouts.Values)
		{
			if (value.IsCreated)
			{
				value.Dispose();
			}
		}
		m_PropertyLayouts.Clear();
	}

	~GPUDrivenShaderMetadataRepository()
	{
		Dispose();
	}

	private static string GetBuildBakedMetadataPath()
	{
		return Path.Combine(Application.streamingAssetsPath, "GPUDrivenBRG/BakedShaderMetadata.shadermetadata");
	}

	public static string GetEditorBakedMetadataPath()
	{
		return Path.Combine("Assets/StreamingAssets", "GPUDrivenBRG/BakedShaderMetadata.shadermetadata");
	}

	private void WarmCacheUpFromBakedMetadata()
	{
		string buildBakedMetadataPath = GetBuildBakedMetadataPath();
		GPUDrivenBakedShaderMetadata gPUDrivenBakedShaderMetadata = GPUDrivenShaderMetadataUtils.ReadFromFile(buildBakedMetadataPath);
		if (gPUDrivenBakedShaderMetadata == null)
		{
			Debug.LogError("Baked Shader Metadata: could not find metadata in Resources at " + buildBakedMetadataPath + ".");
			return;
		}
		GPUDrivenBakedShaderMetadata gPUDrivenBakedShaderMetadata2 = gPUDrivenBakedShaderMetadata;
		if (gPUDrivenBakedShaderMetadata2.ShaderMetadata == null)
		{
			gPUDrivenBakedShaderMetadata2.ShaderMetadata = Array.Empty<GPUDrivenShaderMetadata>();
		}
		GPUDrivenShaderMetadata[] shaderMetadata = gPUDrivenBakedShaderMetadata.ShaderMetadata;
		for (int i = 0; i < shaderMetadata.Length; i++)
		{
			GPUDrivenShaderMetadata value = shaderMetadata[i];
			BakedCacheKey bakedCacheKey = default(BakedCacheKey);
			bakedCacheKey.ShaderName = value.ShaderName;
			bakedCacheKey.ShaderGraphGuid = value.ShaderGraphGuid;
			BakedCacheKey bakedCacheKey2 = bakedCacheKey;
			if (!m_BakedShaderMetadataCache.TryAdd(bakedCacheKey2, value))
			{
				Debug.LogError($"Baked Shader Metadata: duplicate shader found among baked shader metadata: {bakedCacheKey2}. This might be due multiple different shaders having the same name.");
			}
		}
		Debug.Log($"Baked Shader Metadata: created baked cache with {m_BakedShaderMetadataCache.Count}/{gPUDrivenBakedShaderMetadata.ShaderMetadata.Length} entries.");
	}

	public bool IsMaterialSupported(Material material)
	{
		if (material == null)
		{
			return true;
		}
		Shader shader = material.shader;
		if (!shader.isSupported)
		{
			return false;
		}
		return Get(shader).Support == GPUDrivenShaderMetadata.SupportFlags.Everything;
	}

	public void InvalidateCache(Shader shader)
	{
	}

	public bool TryGet(Shader shader, out GPUDrivenShaderMetadata metadata)
	{
		return m_MetadataCache.TryGetValue(shader, out metadata);
	}

	public GPUDrivenShaderMetadata Get(Shader shader)
	{
		if (m_MetadataCache.TryGetValue(shader, out var value))
		{
			return value;
		}
		return m_MetadataCache[shader] = Load(shader);
	}

	private GPUDrivenShaderMetadata Load(Shader shader)
	{
		BakedCacheKey bakedCacheKey = default(BakedCacheKey);
		bakedCacheKey.ShaderName = shader.name;
		bakedCacheKey.ShaderGraphGuid = GPUDrivenShaderGraph.GetGuidOrDefault(shader);
		BakedCacheKey key = bakedCacheKey;
		GPUDrivenShaderMetadata metadata;
		if (m_BakedShaderMetadataCache.TryGetValue(key, out var value))
		{
			metadata = value;
			FillAndValidate(shader, ref metadata);
			m_MetadataCache.Add(shader, metadata);
		}
		else
		{
			metadata = default(GPUDrivenShaderMetadata);
			Debug.LogError("Unbaked shader metadata requested: " + shader.name + ".", shader);
		}
		return metadata;
	}

	private static void FillAndValidate(Shader shader, ref GPUDrivenShaderMetadata metadata)
	{
		ref string shaderName = ref metadata.ShaderName;
		if (shaderName == null)
		{
			shaderName = shader.name;
		}
		ref string[] localKeywordNames = ref metadata.LocalKeywordNames;
		if (localKeywordNames == null)
		{
			localKeywordNames = Array.Empty<string>();
		}
		ref LocalKeyword[] localKeywords = ref metadata.LocalKeywords;
		if (localKeywords == null)
		{
			localKeywords = Array.Empty<LocalKeyword>();
		}
		ref GPUDrivenShaderMetadata.PassMetadata[] passes = ref metadata.Passes;
		if (passes == null)
		{
			passes = Array.Empty<GPUDrivenShaderMetadata.PassMetadata>();
		}
		if (metadata.Support == GPUDrivenShaderMetadata.SupportFlags.Everything)
		{
			string[] localKeywordNames2 = metadata.LocalKeywordNames;
			if (metadata.LocalKeywordNames.Length != shader.keywordSpace.keywordCount)
			{
				metadata.LocalKeywordNames = shader.keywordSpace.keywordNames;
			}
			if (metadata.LocalKeywords.Length != metadata.LocalKeywordNames.Length)
			{
				metadata.LocalKeywords = GPUDrivenShaderMetadataUtils.CreateLocalKeywords(shader, metadata.LocalKeywordNames);
			}
			if (metadata.Passes.Length != shader.passCount)
			{
				metadata.Passes = GPUDrivenShaderMetadataUtils.MigratePasses(shader, metadata.Passes, localKeywordNames2, metadata.LocalKeywordNames);
			}
			metadata.ShadowCasterPassIndex = GPUDrivenShaderMetadataUtils.FindShaderCasterPassIndex(shader);
			metadata.DepthOnlyPassIndex = GPUDrivenShaderMetadataUtils.FindDepthOnlyPassIndex(shader);
			metadata.MotionVectorsPassIndex = GPUDrivenShaderMetadataUtils.FindMotionVectorPassIndex(shader);
		}
	}

	public NativeArray<ShaderPropertyData>.ReadOnly GetPropertyLayout(Shader shader)
	{
		if (m_PropertyLayouts.TryGetValue(shader, out var value))
		{
			return value.AsReadOnly();
		}
		NativeList<ShaderPropertyData> value2 = CollectPropertyLayout(shader, Allocator.Persistent);
		m_PropertyLayouts[shader] = value2;
		return value2.AsReadOnly();
	}

	private static NativeList<ShaderPropertyData> CollectPropertyLayout(Shader shader, Allocator allocator)
	{
		int propertyCount = shader.GetPropertyCount();
		NativeList<ShaderPropertyData> result = new NativeList<ShaderPropertyData>(propertyCount, allocator);
		ShaderPropertyData value;
		for (int i = 0; i < propertyCount; result.Add(in value), i++)
		{
			ShaderPropertyFlags propertyFlags = shader.GetPropertyFlags(i);
			string propertyName = shader.GetPropertyName(i);
			int propertyNameId = shader.GetPropertyNameId(i);
			bool perInstance = (((propertyFlags & ShaderPropertyFlags.PerRendererData) != 0 || propertyName.StartsWith("_PerInstance_") || s_KnownPerInstanceProperties.Contains(propertyNameId)) ? true : false);
			GPUDrivenRenderer.PropertyDataType propertyDataType;
			ShaderPropertyData shaderPropertyData;
			switch (shader.GetPropertyType(i))
			{
			case ShaderPropertyType.Texture:
				shaderPropertyData = default(ShaderPropertyData);
				shaderPropertyData.PropertyIndex = i;
				shaderPropertyData.PerInstance = perInstance;
				shaderPropertyData.TilingOffsetTextureNameID = Shader.PropertyToID(propertyName);
				shaderPropertyData.PropertyData = GPUDrivenRenderer.PropertyData.Vector(Shader.PropertyToID(propertyName + "_ST"), new Vector4(1f, 1f, 0f, 0f));
				value = shaderPropertyData;
				continue;
			case ShaderPropertyType.Color:
				propertyDataType = GPUDrivenRenderer.PropertyDataType.Color;
				break;
			case ShaderPropertyType.Vector:
				propertyDataType = GPUDrivenRenderer.PropertyDataType.Vector;
				break;
			case ShaderPropertyType.Float:
				propertyDataType = GPUDrivenRenderer.PropertyDataType.Float;
				break;
			case ShaderPropertyType.Range:
				propertyDataType = GPUDrivenRenderer.PropertyDataType.Float;
				break;
			case ShaderPropertyType.Int:
				propertyDataType = GPUDrivenRenderer.PropertyDataType.Int;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			GPUDrivenRenderer.PropertyDataType propertyDataType2 = propertyDataType;
			if (propertyDataType2 == GPUDrivenRenderer.PropertyDataType.Color && (propertyFlags & ShaderPropertyFlags.HDR) != 0)
			{
				propertyDataType2 = GPUDrivenRenderer.PropertyDataType.Vector;
			}
			GPUDrivenRenderer.PropertyData propertyData = default(GPUDrivenRenderer.PropertyData);
			propertyData.NameID = Shader.PropertyToID(propertyName);
			propertyData.Type = propertyDataType2;
			GPUDrivenRenderer.PropertyData propertyData2 = propertyData;
			switch (propertyData2.Type)
			{
			case GPUDrivenRenderer.PropertyDataType.Float:
				propertyData2.Value.Float = shader.GetPropertyDefaultFloatValue(i);
				break;
			case GPUDrivenRenderer.PropertyDataType.Int:
				propertyData2.Value.Int = shader.GetPropertyDefaultIntValue(i);
				break;
			case GPUDrivenRenderer.PropertyDataType.Vector:
				propertyData2.Value.Vector = shader.GetPropertyDefaultVectorValue(i);
				break;
			case GPUDrivenRenderer.PropertyDataType.Color:
				propertyData2.Value.Color = shader.GetPropertyDefaultVectorValue(i);
				propertyData2.Value.Color = propertyData2.Value.Color.linear;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			shaderPropertyData = default(ShaderPropertyData);
			shaderPropertyData.PropertyIndex = i;
			shaderPropertyData.PerInstance = perInstance;
			shaderPropertyData.PropertyData = propertyData2;
			value = shaderPropertyData;
			if (value.PropertyIndex >= 0 && Array.IndexOf(shader.GetPropertyAttributes(value.PropertyIndex), "BreaksBatching") != -1)
			{
				value.BreaksBatching = true;
			}
		}
		return result;
	}
}
