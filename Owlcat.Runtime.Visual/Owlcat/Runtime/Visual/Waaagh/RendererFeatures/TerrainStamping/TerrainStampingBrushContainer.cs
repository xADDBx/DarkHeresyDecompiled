using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.TerrainStamping;

public static class TerrainStampingBrushContainer
{
	public struct BrushData
	{
		public float2 Position;

		public float Radius;

		public float RelativeRadius;

		public float Strength;

		public int Index;
	}

	private static readonly List<TerrainStampingBrush> s_Brushes = new List<TerrainStampingBrush>();

	public static NativeArray<BrushData> GetData(float chunkSize, Allocator allocator)
	{
		NativeArray<BrushData> result = new NativeArray<BrushData>(s_Brushes.Count, allocator, NativeArrayOptions.UninitializedMemory);
		for (int i = 0; i < s_Brushes.Count; i++)
		{
			TerrainStampingBrush terrainStampingBrush = s_Brushes[i];
			result[i] = new BrushData
			{
				Position = ((float3)terrainStampingBrush.transform.position).xz,
				Radius = terrainStampingBrush.Radius,
				RelativeRadius = 2f * terrainStampingBrush.Radius / chunkSize,
				Index = i,
				Strength = terrainStampingBrush.Strength
			};
		}
		return result;
	}

	public static Texture2D GetTexture(int brushIndex)
	{
		Texture2D texture2D = s_Brushes[brushIndex].Texture;
		if (texture2D == null)
		{
			texture2D = Texture2D.whiteTexture;
		}
		return texture2D;
	}

	public static void Add(TerrainStampingBrush zone)
	{
		s_Brushes.Add(zone);
	}

	public static void Remove(TerrainStampingBrush zone)
	{
		s_Brushes.Remove(zone);
	}
}
