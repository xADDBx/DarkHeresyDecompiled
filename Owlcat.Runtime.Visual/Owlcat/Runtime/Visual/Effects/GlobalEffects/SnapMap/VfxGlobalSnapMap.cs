using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.Waaagh;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.GlobalEffects.SnapMap;

public class VfxGlobalSnapMap : MonoBehaviour
{
	[Serializable]
	public class AdditionalFilters
	{
		public bool IgnoreParticlesShader = true;

		public bool IgnoreWaterShader = true;

		public bool IgnoreUnlitShader = true;

		public bool IgnoreXPBDObjects = true;
	}

	private static HashSet<VfxGlobalSnapMap> s_All;

	private static VfxGlobalSnapMapTexture s_Texture;

	private static VfxGlobalSnapMapMorton s_Morton;

	public List<float3> SnapPoints = new List<float3>();

	public Bounds Bounds;

	public int ContentHash;

	public LayerMask LayerMask = -1;

	public AdditionalFilters RaycastFilters;

	[Range(0.1f, 10f)]
	public float DensityPerMeter = 1f;

	public static IEnumerable<VfxGlobalSnapMap> All => s_All;

	static VfxGlobalSnapMap()
	{
		s_All = new HashSet<VfxGlobalSnapMap>();
		s_Texture = null;
		s_Morton = null;
		WaaaghPipeline.Created += OnWaaaghPipelineCreated;
		WaaaghPipeline.Destroying += OnWaaaghPipelineDestroying;
	}

	private static void OnWaaaghPipelineCreated(WaaaghPipeline pipeline)
	{
		if (s_Texture != null)
		{
			s_Texture.Dispose();
		}
		s_Texture = new VfxGlobalSnapMapTexture();
		s_Morton?.Dispose();
		s_Morton = new VfxGlobalSnapMapMorton();
	}

	private static void OnWaaaghPipelineDestroying(WaaaghPipeline pipeline)
	{
		if (s_Texture != null)
		{
			s_Texture.Dispose();
			s_Texture = null;
		}
		if (s_Morton != null)
		{
			s_Morton.Dispose();
			s_Morton = null;
		}
	}

	private void OnEnable()
	{
		s_All.Add(this);
	}

	private void OnDisable()
	{
		s_All.Remove(this);
	}

	public static Texture2D GetTexture()
	{
		if (s_Texture != null)
		{
			s_Texture.Update(s_All);
			return s_Texture.Texture;
		}
		return null;
	}

	public static GraphicsBuffer GetMortonBuffer()
	{
		if (s_Morton != null)
		{
			s_Morton.Update(s_All);
			return s_Morton.Buffer;
		}
		return null;
	}

	public static Bounds GetTextureBounds()
	{
		if (s_Texture != null)
		{
			return s_Texture.Bounds;
		}
		return default(Bounds);
	}

	public static Vector2 GetTexelSize()
	{
		if (s_Texture != null)
		{
			return s_Texture.TexelSize;
		}
		return Vector2.zero;
	}

	private void OnDrawGizmosSelected()
	{
		if (Application.isPlaying)
		{
			GraphicsBuffer mortonBuffer = GetMortonBuffer();
			if (mortonBuffer == null)
			{
				return;
			}
			Bounds bounds = s_Morton.Bounds;
			VfxGlobalSnapMapMorton.MortonPoint[] array = new VfxGlobalSnapMapMorton.MortonPoint[mortonBuffer.count];
			mortonBuffer.GetData(array);
			Color color = Gizmos.color;
			for (int i = 0; i < array.Length; i++)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawWireCube(array[i].Position, Vector3.one * 0.1f);
				if (i > 0)
				{
					Gizmos.color = Color.yellow;
					Gizmos.DrawLine(array[i - 1].Position, array[i].Position);
				}
			}
			Gizmos.color = Color.green;
			Gizmos.DrawWireCube(bounds.center, bounds.size);
			Gizmos.color = color;
			return;
		}
		Color color2 = Gizmos.color;
		foreach (float3 snapPoint in SnapPoints)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireCube(snapPoint.xyz, Vector3.one * 0.1f);
		}
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(Bounds.center, Bounds.size);
		Gizmos.color = color2;
	}
}
