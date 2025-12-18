using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.MaterialEffects.BloodMask;

public readonly struct BloodMaskMaterial
{
	public struct Snapshot
	{
		public float BloodMaskEdge;

		public Color BloodMaskColor;

		public Texture BloodMask;

		public Vector4 BloodMask_ST;
	}

	private static readonly int _BloodMaskColor = Shader.PropertyToID("_BloodMaskColor");

	private static readonly int _BloodMask = Shader.PropertyToID("_BloodMask");

	private static readonly int _BloodMask_ST = Shader.PropertyToID("_BloodMask_ST");

	private static readonly int _BloodMaskEdge = Shader.PropertyToID("_BloodMaskEdge");

	private static readonly Dictionary<Shader, bool> s_ShadersCompatibilityCache = new Dictionary<Shader, bool>();

	private readonly Material m_Material;

	public Texture BloodMask
	{
		get
		{
			return m_Material.GetTexture(_BloodMask);
		}
		set
		{
			m_Material.SetTexture(_BloodMask, value);
		}
	}

	public Vector4 BloodMask_ST
	{
		get
		{
			return m_Material.GetVector(_BloodMask_ST);
		}
		set
		{
			m_Material.SetVector(_BloodMask_ST, value);
		}
	}

	public float BloodMaskEdge
	{
		get
		{
			return m_Material.GetFloat(_BloodMaskEdge);
		}
		set
		{
			m_Material.SetFloat(_BloodMaskEdge, value);
		}
	}

	public Color BloodMaskColor
	{
		get
		{
			return m_Material.GetColor(_BloodMaskColor);
		}
		set
		{
			m_Material.SetColor(_BloodMaskColor, value);
		}
	}

	public BloodMaskMaterial(Material material)
	{
		this = default(BloodMaskMaterial);
		m_Material = material;
	}

	public static bool IsMaterialCompatible(Material material)
	{
		Shader shader = material.shader;
		if (!s_ShadersCompatibilityCache.TryGetValue(shader, out var value))
		{
			value = material.HasFloat(_BloodMaskEdge) && material.HasColor(_BloodMaskColor) && material.HasTexture(_BloodMask);
			s_ShadersCompatibilityCache.Add(shader, value);
		}
		return value;
	}

	public Snapshot TakeSnapshot()
	{
		Snapshot result = default(Snapshot);
		result.BloodMaskEdge = BloodMaskEdge;
		result.BloodMaskColor = BloodMaskColor;
		result.BloodMask = BloodMask;
		result.BloodMask_ST = BloodMask_ST;
		return result;
	}

	public void ApplySnapshot(in Snapshot snapshot)
	{
		BloodMaskEdge = snapshot.BloodMaskEdge;
		BloodMaskColor = snapshot.BloodMaskColor;
		BloodMask = snapshot.BloodMask;
		BloodMask_ST = snapshot.BloodMask_ST;
	}
}
