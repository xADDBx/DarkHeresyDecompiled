using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Profiling.Memory;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Scenes;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.BakedLighting;

public class GPUDrivenLightmapping : IDisposable, IGPUDrivenMemoryProfilingSource
{
	public struct SceneMaterialKey : IEquatable<SceneMaterialKey>
	{
		public BatchMaterialID OriginalMaterialID;

		public GPUDrivenSceneHandle Scene;

		public bool Equals(SceneMaterialKey other)
		{
			if (OriginalMaterialID.Equals(other.OriginalMaterialID))
			{
				return Scene.Equals(other.Scene);
			}
			return false;
		}

		[BurstDiscard]
		public override bool Equals(object obj)
		{
			if (obj is SceneMaterialKey other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (17 * 23 + OriginalMaterialID.GetHashCode()) * 23 + Scene.GetHashCode();
		}
	}

	private static class ShaderIDs
	{
		public static readonly int unity_Lightmaps = Shader.PropertyToID("unity_Lightmaps");

		public static readonly int unity_LightmapsInd = Shader.PropertyToID("unity_LightmapsInd");

		public static readonly int unity_ShadowMasks = Shader.PropertyToID("unity_ShadowMasks");
	}

	[Flags]
	private enum LightmappingFlags
	{
		None = 0,
		Lightmapped = 1,
		Directional = 2,
		ShadowMask = 4
	}

	private struct MaterialKey : IEquatable<MaterialKey>
	{
		public Material BaseMaterial;

		public BatchMaterialID BaseMaterialID;

		public GPUDrivenSceneHandle Scene;

		public LightmapArraysCollection LightMaps;

		public LightmappingFlags Flags;

		[BurstDiscard]
		public bool Equals(MaterialKey other)
		{
			if (object.Equals(BaseMaterial, other.BaseMaterial) && BaseMaterialID.Equals(other.BaseMaterialID) && Scene.Equals(other.Scene) && LightMaps.Equals(other.LightMaps))
			{
				return Flags == other.Flags;
			}
			return false;
		}

		[BurstDiscard]
		public override bool Equals(object obj)
		{
			if (obj is MaterialKey other)
			{
				return Equals(other);
			}
			return false;
		}

		[BurstDiscard]
		public override int GetHashCode()
		{
			return HashCode.Combine(BaseMaterial, BaseMaterialID, Scene, LightMaps, (int)Flags);
		}
	}

	public struct MaterialEntry : IEquatable<MaterialEntry>
	{
		public Material LightmappedMaterial;

		public BatchMaterialID BatchMaterialID;

		[BurstDiscard]
		public bool Equals(MaterialEntry other)
		{
			if (object.Equals(LightmappedMaterial, other.LightmappedMaterial))
			{
				return BatchMaterialID.Equals(other.BatchMaterialID);
			}
			return false;
		}

		[BurstDiscard]
		public override bool Equals(object obj)
		{
			if (obj is MaterialEntry other)
			{
				return Equals(other);
			}
			return false;
		}

		[BurstDiscard]
		public override int GetHashCode()
		{
			return HashCode.Combine(LightmappedMaterial, BatchMaterialID);
		}
	}

	private const int kAffectLightmapIndex = 65534;

	public const int kMaxLightmapIndex = 65533;

	public const int kNotLightmappedIndex = -1;

	private static readonly StringBuilder s_StringBuilder = new StringBuilder();

	private readonly List<GPUDrivenSceneHandle> m_AllMetScenes = new List<GPUDrivenSceneHandle>();

	private readonly BatchRendererGroup m_BRG;

	private readonly Dictionary<MaterialKey, MaterialEntry> m_LightmappedMaterialsCache = new Dictionary<MaterialKey, MaterialEntry>();

	private readonly Dictionary<Material, List<MaterialKey>> m_MaterialToMaterialKeys = new Dictionary<Material, List<MaterialKey>>();

	private readonly Dictionary<GPUDrivenSceneHandle, LightmapArraysCollection> m_SceneToLightmaps = new Dictionary<GPUDrivenSceneHandle, LightmapArraysCollection>();

	private NativeHashMap<SceneMaterialKey, BatchMaterialID> m_SceneLightmappedMaterials;

	public GPUDrivenLightmapping(BatchRendererGroup brg)
	{
		m_BRG = brg;
		m_SceneLightmappedMaterials = new NativeHashMap<SceneMaterialKey, BatchMaterialID>(16, Allocator.Persistent);
	}

	public void Dispose()
	{
		foreach (MaterialEntry value in m_LightmappedMaterialsCache.Values)
		{
			Release(value);
		}
		m_LightmappedMaterialsCache.Clear();
		foreach (List<MaterialKey> value2 in m_MaterialToMaterialKeys.Values)
		{
			ListPool<MaterialKey>.Release(value2);
		}
		m_MaterialToMaterialKeys.Clear();
		m_SceneLightmappedMaterials.Dispose();
	}

	public void FillMemoryCounters(Counters.CounterCollection counters)
	{
		counters.CollectBufferSize(counters.MiscCPU, m_AllMetScenes);
		counters.CollectBufferSize(counters.ResourceDataCPU, m_SceneLightmappedMaterials);
		counters.CollectBufferSize(counters.ResourceDataCPU, m_LightmappedMaterialsCache);
		counters.CollectBufferSize(counters.ResourceDataCPU, m_MaterialToMaterialKeys);
		counters.CollectBufferSize(counters.MiscCPU, m_SceneToLightmaps);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public NativeHashMap<SceneMaterialKey, BatchMaterialID>.ReadOnly GetSceneLightmappedMaterialsReadonly()
	{
		return m_SceneLightmappedMaterials.AsReadOnly();
	}

	public void RemoveAll(Material baseMaterial, BatchMaterialID batchMaterialID)
	{
		if (m_MaterialToMaterialKeys.Remove(baseMaterial, out var value))
		{
			foreach (MaterialKey item in value)
			{
				MaterialKey materialKey = item;
				Remove(in materialKey);
			}
			ListPool<MaterialKey>.Release(value);
		}
		foreach (GPUDrivenSceneHandle allMetScene in m_AllMetScenes)
		{
			SceneMaterialKey sceneMaterialKey = default(SceneMaterialKey);
			sceneMaterialKey.OriginalMaterialID = batchMaterialID;
			sceneMaterialKey.Scene = allMetScene;
			SceneMaterialKey key = sceneMaterialKey;
			m_SceneLightmappedMaterials.Remove(key);
		}
	}

	private void Remove(in MaterialKey materialKey)
	{
		if (m_LightmappedMaterialsCache.Remove(materialKey, out var value))
		{
			Release(value);
		}
	}

	private void Release(MaterialEntry materialEntry)
	{
		m_BRG.UnregisterMaterial(materialEntry.BatchMaterialID);
		CoreUtils.Destroy(materialEntry.LightmappedMaterial);
	}

	public bool AddLightmaps(GPUDrivenSceneHandle scene, in LightmapArraysCollection lightmaps, out int count)
	{
		if (lightmaps.Color == null || (lightmaps.Color == null && lightmaps.Dir == null && lightmaps.Shadowmask == null))
		{
			count = 0;
			return false;
		}
		m_SceneToLightmaps[scene] = lightmaps;
		count = lightmaps.Color.depth;
		return true;
	}

	public void RemoveLightmaps(GPUDrivenSceneHandle scene)
	{
		m_SceneToLightmaps.Remove(scene);
		m_AllMetScenes.Remove(scene);
		List<MaterialKey> value;
		using (ListPool<MaterialKey>.Get(out value))
		{
			foreach (MaterialKey key in m_LightmappedMaterialsCache.Keys)
			{
				GPUDrivenSceneHandle scene2 = key.Scene;
				if (scene2.Equals(scene))
				{
					value.Add(key);
				}
			}
			foreach (MaterialKey item in value)
			{
				MaterialKey materialKey = item;
				if (m_MaterialToMaterialKeys.TryGetValue(materialKey.BaseMaterial, out var value2))
				{
					value2.Remove(materialKey);
				}
				m_SceneLightmappedMaterials.Remove(new SceneMaterialKey
				{
					OriginalMaterialID = materialKey.BaseMaterialID,
					Scene = scene
				});
				Remove(in materialKey);
			}
		}
	}

	public void TryRegisterLightmappedMaterial(Material baseMaterial, BatchMaterialID baseMaterialID, GPUDrivenSceneHandle scene)
	{
		if (baseMaterial == null || !m_SceneToLightmaps.TryGetValue(scene, out var lightmaps))
		{
			return;
		}
		LightmappingFlags lightmappingFlags = LightmappingFlags.Lightmapped;
		if (lightmaps.Dir != null)
		{
			lightmappingFlags |= LightmappingFlags.Directional;
		}
		if (lightmaps.Shadowmask != null)
		{
			lightmappingFlags |= LightmappingFlags.ShadowMask;
		}
		MaterialKey materialKey = default(MaterialKey);
		materialKey.BaseMaterial = baseMaterial;
		materialKey.BaseMaterialID = baseMaterialID;
		materialKey.Scene = scene;
		materialKey.LightMaps = lightmaps;
		materialKey.Flags = lightmappingFlags;
		MaterialKey materialKey2 = materialKey;
		if (!m_LightmappedMaterialsCache.TryGetValue(materialKey2, out var value))
		{
			value = CreateMaterialEntry(baseMaterial, in lightmaps);
			m_SceneLightmappedMaterials[new SceneMaterialKey
			{
				OriginalMaterialID = baseMaterialID,
				Scene = scene
			}] = value.BatchMaterialID;
			m_AllMetScenes.Add(scene);
			if (!m_MaterialToMaterialKeys.TryGetValue(baseMaterial, out var value2))
			{
				value2 = ListPool<MaterialKey>.Get();
				m_MaterialToMaterialKeys.Add(baseMaterial, value2);
			}
			value2.Add(materialKey2);
			m_LightmappedMaterialsCache[materialKey2] = value;
		}
	}

	private MaterialEntry CreateMaterialEntry(Material baseMaterial, in LightmapArraysCollection lightmaps)
	{
		Material material = CreateLightMappedMaterial(baseMaterial, in lightmaps);
		MaterialEntry result = default(MaterialEntry);
		result.LightmappedMaterial = material;
		result.BatchMaterialID = m_BRG.RegisterMaterial(material);
		return result;
	}

	public void OnMaterialUpdated(Material material)
	{
		if (material == null || !m_MaterialToMaterialKeys.TryGetValue(material, out var value))
		{
			return;
		}
		foreach (MaterialKey item in value)
		{
			MaterialKey current = item;
			if (!m_LightmappedMaterialsCache.TryGetValue(current, out var value2) || !(value2.LightmappedMaterial != null))
			{
				continue;
			}
			ref LightmapArraysCollection lightMaps = ref current.LightMaps;
			if (value2.LightmappedMaterial.shader != material.shader)
			{
				Release(value2);
				value2 = CreateMaterialEntry(material, in lightMaps);
				m_LightmappedMaterialsCache[current] = value2;
				foreach (GPUDrivenSceneHandle allMetScene in m_AllMetScenes)
				{
					m_SceneLightmappedMaterials[new SceneMaterialKey
					{
						OriginalMaterialID = current.BaseMaterialID,
						Scene = allMetScene
					}] = value2.BatchMaterialID;
				}
			}
			else
			{
				value2.LightmappedMaterial.CopyMatchingPropertiesFromMaterial(material);
				SetLightmapsTextureArrays(value2.LightmappedMaterial, in lightMaps);
				EnableLightmappedKeywords(value2.LightmappedMaterial, in lightMaps);
			}
		}
	}

	private static Material CreateLightMappedMaterial(Material baseMaterial, in LightmapArraysCollection lightmaps)
	{
		Material obj = new Material(baseMaterial)
		{
			name = GetLightmappedMaterialName(baseMaterial, in lightmaps)
		};
		EnableLightmappedKeywords(obj, in lightmaps);
		SetLightmapsTextureArrays(obj, in lightmaps);
		return obj;
	}

	private static string GetLightmappedMaterialName(Material baseMaterial, in LightmapArraysCollection lightmaps)
	{
		s_StringBuilder.Clear().Append(baseMaterial.name).Append("_Lightmapped_");
		if (lightmaps.Dir != null)
		{
			s_StringBuilder.Append("_DIRLIGHTMAP");
		}
		if (lightmaps.Shadowmask != null)
		{
			s_StringBuilder.Append("_SHADOW_MASK");
		}
		return s_StringBuilder.ToString();
	}

	private static void SetLightmapsTextureArrays(Material material, in LightmapArraysCollection lightmaps)
	{
		material.SetTexture(ShaderIDs.unity_Lightmaps, lightmaps.Color);
		material.SetTexture(ShaderIDs.unity_LightmapsInd, lightmaps.Dir);
		material.SetTexture(ShaderIDs.unity_ShadowMasks, lightmaps.Shadowmask);
	}

	private static void EnableLightmappedKeywords(Material material, in LightmapArraysCollection lightmaps)
	{
		CoreUtils.SetKeyword(material, "LIGHTMAP_ON", state: true);
		CoreUtils.SetKeyword(material, "DIRLIGHTMAP_COMBINED", lightmaps.Dir != null);
	}

	internal static void Init()
	{
	}

	internal static void Cleanup()
	{
	}
}
