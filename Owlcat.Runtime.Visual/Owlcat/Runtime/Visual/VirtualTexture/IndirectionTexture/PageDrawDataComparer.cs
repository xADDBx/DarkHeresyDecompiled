using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Burst;

namespace Owlcat.Runtime.Visual.VirtualTexture.IndirectionTexture;

[StructLayout(LayoutKind.Sequential, Size = 1)]
[BurstCompile]
internal struct PageDrawDataComparer : IComparer<PageDrawData>
{
	public int Compare(PageDrawData x, PageDrawData y)
	{
		return -x.MipLevel.CompareTo(y.MipLevel);
	}
}
