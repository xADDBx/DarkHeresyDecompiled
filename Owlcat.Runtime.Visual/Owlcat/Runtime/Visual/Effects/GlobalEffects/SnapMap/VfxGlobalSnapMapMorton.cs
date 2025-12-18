using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.GlobalEffects.SnapMap;

public class VfxGlobalSnapMapMorton : IDisposable
{
	public struct MortonPoint
	{
		public float3 Position;

		public uint MortonCode;
	}

	private const int kMortonBits2d = 16;

	private const uint kMortonMask2d = 65535u;

	private int m_Hash;

	private Bounds m_Bounds;

	private GraphicsBuffer m_PointsBuffer;

	public GraphicsBuffer Buffer => m_PointsBuffer;

	public Bounds Bounds => m_Bounds;

	public void Dispose()
	{
		CleanUp();
	}

	private void CleanUp()
	{
		m_PointsBuffer?.Dispose();
	}

	public void Update(IEnumerable<VfxGlobalSnapMap> snapMaps)
	{
		if (snapMaps.Count() == 0)
		{
			CleanUp();
			m_Hash = 0;
			return;
		}
		int num = CalculateHash(snapMaps);
		if (m_Hash != num)
		{
			CleanUp();
			CreatePointBuffer(snapMaps);
			m_Hash = num;
		}
	}

	private void CreatePointBuffer(IEnumerable<VfxGlobalSnapMap> snapMaps)
	{
		int num = 0;
		m_Bounds = default(Bounds);
		foreach (VfxGlobalSnapMap snapMap in snapMaps)
		{
			if (num == 0)
			{
				m_Bounds = snapMap.Bounds;
			}
			else
			{
				m_Bounds.Encapsulate(snapMap.Bounds);
			}
			num++;
		}
		float3 @float = m_Bounds.min;
		float3 float2 = m_Bounds.size;
		int num2 = snapMaps.Sum((VfxGlobalSnapMap sm) => sm.SnapPoints.Count);
		List<MortonPoint> list = new List<MortonPoint>(num2);
		num = 0;
		foreach (VfxGlobalSnapMap snapMap2 in snapMaps)
		{
			foreach (float3 snapPoint in snapMap2.SnapPoints)
			{
				uint mortonCode = Morton2D(math.saturate((snapPoint.xz - @float.xz) / float2.xz));
				list.Add(new MortonPoint
				{
					Position = snapPoint,
					MortonCode = mortonCode
				});
			}
		}
		list.Sort((MortonPoint a, MortonPoint b) => a.MortonCode.CompareTo(b.MortonCode));
		m_PointsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, num2, Marshal.SizeOf<MortonPoint>());
		m_PointsBuffer.SetData(list);
	}

	private static uint Morton2D(float2 normalizedCoords)
	{
		uint2 @uint = Quantize2D(normalizedCoords);
		return ExpandBits2D(@uint.x) | (ExpandBits2D(@uint.y) << 1);
	}

	private static uint2 Quantize2D(float2 pointNorm)
	{
		return math.clamp((uint2)math.round(pointNorm * 65535f), 0u, 65535u);
	}

	private static uint ExpandBits2D(uint v)
	{
		v &= 0xFFFFu;
		v = (v | (v << 8)) & 0xFF00FFu;
		v = (v | (v << 4)) & 0xF0F0F0Fu;
		v = (v | (v << 2)) & 0x33333333u;
		v = (v | (v << 1)) & 0x55555555u;
		return v;
	}

	private int CalculateHash(IEnumerable<VfxGlobalSnapMap> snapMaps)
	{
		int num = 0;
		foreach (VfxGlobalSnapMap snapMap in snapMaps)
		{
			num = HashCode.Combine(num, snapMap.ContentHash);
		}
		return num;
	}
}
