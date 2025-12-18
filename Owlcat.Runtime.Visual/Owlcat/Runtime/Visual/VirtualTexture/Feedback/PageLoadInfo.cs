using System;
using Unity.Burst;

namespace Owlcat.Runtime.Visual.VirtualTexture.Feedback;

[BurstCompile]
public struct PageLoadInfo : IComparable<PageLoadInfo>
{
	public int VirtualTileX;

	public int VirtualTileY;

	public int VirtualMipLevel;

	public PageLoadInfo(in int x, in int y, in int mipLevel)
	{
		VirtualTileX = x;
		VirtualTileY = y;
		VirtualMipLevel = mipLevel;
	}

	public bool Equals(in PageLoadInfo target)
	{
		if (target.VirtualTileX == VirtualTileX && target.VirtualTileY == VirtualTileY)
		{
			return target.VirtualMipLevel == VirtualMipLevel;
		}
		return false;
	}

	public bool NotEquals(PageLoadInfo target)
	{
		if (target.VirtualTileX == VirtualTileX && target.VirtualTileY == VirtualTileY)
		{
			return target.VirtualMipLevel != VirtualMipLevel;
		}
		return true;
	}

	public override bool Equals(object target)
	{
		PageLoadInfo target2 = (PageLoadInfo)target;
		return Equals(in target2);
	}

	public override int GetHashCode()
	{
		return ((((0x1A5D ^ VirtualTileX.GetHashCode()) * 397) ^ VirtualTileY.GetHashCode()) * 397) ^ VirtualMipLevel.GetHashCode();
	}

	public int CompareTo(PageLoadInfo target)
	{
		return VirtualMipLevel.CompareTo(target.VirtualMipLevel);
	}
}
