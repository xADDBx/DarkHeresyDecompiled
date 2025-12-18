using System;
using System.Collections.Generic;
using System.Linq;
using Owlcat.Runtime.Visual.Utilities;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.GlobalEffects.SnapMap;

public class VfxGlobalSnapMapTexture : IDisposable
{
	private int m_Hash;

	private Texture2D m_Texture;

	private Bounds m_Bounds;

	private float2 m_TexelSize;

	public float2 TexelSize => m_TexelSize;

	public Bounds Bounds => m_Bounds;

	public Texture2D Texture => m_Texture;

	public void Dispose()
	{
		CleanUp();
	}

	private void CleanUp()
	{
		if (m_Texture != null)
		{
			UnityEngine.Object.DestroyImmediate(m_Texture);
			m_Texture = null;
		}
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
			CreateTexture(snapMaps);
			m_Hash = num;
		}
	}

	private void CreateTexture(IEnumerable<VfxGlobalSnapMap> snapMaps)
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
		float num2 = snapMaps.Max((VfxGlobalSnapMap sm) => sm.DensityPerMeter);
		int2 @int = new int2((int)(m_Bounds.size.x * num2), (int)(m_Bounds.size.z * num2));
		if (math.any(@int > 2048))
		{
			@int = math.min(@int, 2048);
		}
		m_Texture = new Texture2D(@int.x, @int.y, TextureFormat.RGBA32, mipChain: false, linear: true)
		{
			filterMode = FilterMode.Point,
			wrapMode = TextureWrapMode.Clamp
		};
		m_Texture.name = "VfxGlobalSnapMapTexture";
		NativeArray<Color32> rawTextureData = m_Texture.GetRawTextureData<Color32>();
		float3 @float = m_Bounds.size;
		float3 float2 = m_Bounds.min;
		m_TexelSize = @float.xz / @int;
		for (int i = 0; i < rawTextureData.Length; i++)
		{
			rawTextureData[i] = new Color32(0, 0, 0, 0);
		}
		foreach (VfxGlobalSnapMap snapMap2 in snapMaps)
		{
			foreach (float3 snapPoint in snapMap2.SnapPoints)
			{
				float2 float3 = snapPoint.xz - float2.xz;
				int2 valueToClamp = (int2)(float3 / @float.xz * @int);
				valueToClamp = math.clamp(valueToClamp, 0, @int - 1);
				float2 float4 = math.saturate((float3 - valueToClamp * m_TexelSize) / m_TexelSize);
				float f = math.saturate((snapPoint.y - float2.y) / @float.y);
				uint num3 = Packing.UnpackByte(float4.x);
				uint num4 = Packing.UnpackByte(float4.y);
				float2 float5 = Packing.PackFloatToR8G8(f);
				uint num5 = Packing.UnpackByte(float5.x);
				uint num6 = Packing.UnpackByte(float5.y);
				rawTextureData[valueToClamp.y * @int.x + valueToClamp.x] = new Color32((byte)num3, (byte)num5, (byte)num6, (byte)num4);
			}
		}
		bool makeNoLongerReadable = !Application.isEditor;
		m_Texture.Apply(updateMipmaps: false, makeNoLongerReadable);
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
