using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.VirtualTexture.Atlas.Jobs;

[BurstCompile]
public struct EntryToAllocate
{
	public VirtualAtlasEntry Entry;

	public int4 Rect;

	public uint AllocId;

	public int IndexInAllocator;
}
