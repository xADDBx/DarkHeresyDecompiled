using System.Collections.Generic;
using System.Runtime.InteropServices;
using Owlcat.Runtime.Visual.VirtualTexture.Atlas.Jobs;

namespace Owlcat.Runtime.Visual.VirtualTexture.Atlas;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct AllocationDataComparer : IComparer<EntryToAllocate>
{
	public int Compare(EntryToAllocate x, EntryToAllocate y)
	{
		return y.Rect.z * y.Rect.w - x.Rect.z * x.Rect.w;
	}
}
