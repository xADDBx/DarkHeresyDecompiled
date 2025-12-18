using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.MaterialEffects.MaterialParametersOverride;

public readonly struct ParametersOverrideMaterial
{
	public struct Snapshot
	{
		public float Roughness;

		public float Metallic;

		public Texture BaseMap;

		public Texture BumpMap;

		public Vector4 BaseMap_ST;
	}

	private static readonly int _Emission = Shader.PropertyToID("_Emission");

	private static readonly int _Roughness = Shader.PropertyToID("_Roughness");

	private static readonly int _Metallic = Shader.PropertyToID("_Metallic");

	private static readonly int _BumpMap = Shader.PropertyToID("_BumpMap");

	private static readonly int _BaseMap = Shader.PropertyToID("_BaseMap");

	private static readonly int _BaseMap_ST = Shader.PropertyToID("_BaseMap_ST");

	private static readonly Dictionary<Shader, bool> s_ShadersCompatibilityCache = new Dictionary<Shader, bool>();

	private readonly Material m_Material;

	public float Roughness
	{
		get
		{
			return m_Material.GetFloat(_Roughness);
		}
		set
		{
			m_Material.SetFloat(_Roughness, value);
		}
	}

	public float Metallic
	{
		get
		{
			return m_Material.GetFloat(_Metallic);
		}
		set
		{
			m_Material.SetFloat(_Metallic, value);
		}
	}

	public Texture BaseMap
	{
		get
		{
			return m_Material.GetTexture(_BaseMap);
		}
		set
		{
			m_Material.SetTexture(_BaseMap, value);
		}
	}

	public Texture BumpMap
	{
		get
		{
			return m_Material.GetTexture(_BumpMap);
		}
		set
		{
			m_Material.SetTexture(_BumpMap, value);
		}
	}

	public Vector4 BaseMap_ST
	{
		get
		{
			return m_Material.GetVector(_BaseMap_ST);
		}
		set
		{
			m_Material.SetVector(_BaseMap_ST, value);
		}
	}

	public ParametersOverrideMaterial(Material material)
	{
		m_Material = material;
	}

	public Snapshot TakeSnapshot()
	{
		Snapshot result = default(Snapshot);
		result.Roughness = Roughness;
		result.Metallic = Metallic;
		result.BaseMap = BaseMap;
		result.BumpMap = BumpMap;
		result.BaseMap_ST = BaseMap_ST;
		return result;
	}

	public void ApplySnapshot(Snapshot snapshot)
	{
		Roughness = snapshot.Roughness;
		Metallic = snapshot.Metallic;
		BaseMap = snapshot.BaseMap;
		BumpMap = snapshot.BumpMap;
		BaseMap_ST = snapshot.BaseMap_ST;
	}

	public static bool IsMaterialCompatible(Material material)
	{
		Shader shader = material.shader;
		if (!s_ShadersCompatibilityCache.TryGetValue(shader, out var value))
		{
			value = material.HasFloat(_Emission) && material.HasFloat(_Roughness) && material.HasFloat(_Metallic) && material.HasTexture(_BaseMap) && material.HasTexture(_BumpMap) && material.HasVector(_BaseMap_ST);
			s_ShadersCompatibilityCache.Add(shader, value);
		}
		return value;
	}
}
