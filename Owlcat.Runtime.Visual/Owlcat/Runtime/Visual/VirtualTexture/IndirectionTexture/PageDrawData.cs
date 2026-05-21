using System;
using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.VirtualTexture.IndirectionTexture;

[BurstCompile]
internal struct PageDrawData : IComparable<PageDrawData>
{
	public int MipLevel;

	public float4 Rect;

	public float2 DrawPos;

	public int SliceIndex;

	public int CompareTo(PageDrawData target)
	{
		float value = Rect.z * Rect.w;
		int num = (target.Rect.z * target.Rect.w).CompareTo(value);
		if (num != 0)
		{
			return num;
		}
		return target.MipLevel.CompareTo(MipLevel);
	}
}
