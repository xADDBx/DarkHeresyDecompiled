using Owlcat.Runtime.Core.Allocators.Guillotiere;
using Owlcat.Runtime.Core.Collections;
using Owlcat.Runtime.Visual.VirtualTexture.Materials;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.VirtualTexture.Atlas.Jobs;

[BurstCompile]
internal struct RemoveUnusedTextureStacksJob : IJob
{
	public NativeList<VirtualAtlasEntry> Entries;

	public NativeParallelHashMap<int, MaterialStackIndices> MaterialStackIndices;

	public NativeParallelHashMap<TextureStackId, int> TextureStackIndices;

	public NativeArray<Page> Pages;

	public NativeParallelHashMap<int3, int2> PhysicalToVirtualPageMap;

	[NativeDisableUnsafePtrRestriction]
	public unsafe NativeAtlasAllocator* Allocator;

	public unsafe void Execute()
	{
		NativeArray<int> nativeArray = new NativeArray<int>(Entries.Length, Unity.Collections.Allocator.Temp);
		foreach (KeyValue<int, MaterialStackIndices> materialStackIndex in MaterialStackIndices)
		{
			for (int i = 0; i < materialStackIndex.Value.Count; i++)
			{
				nativeArray[materialStackIndex.Value[i]]++;
			}
		}
		for (int j = 0; j < nativeArray.Length; j++)
		{
			if (nativeArray[j] == 0)
			{
				ref VirtualAtlasEntry reference = ref UnsafeCollectionExtensions.ElementAsRef(in Entries, j);
				if (reference.NodeKind == NodeKind.Alloc)
				{
					Allocator->Deallocate(reference.RectAllocId);
					TextureStackIndices.Remove(reference.StackId);
					VirtualAtlas.InvalidatePages(ref reference, Pages, PhysicalToVirtualPageMap, Allocator->Width);
					reference.NodeKind = NodeKind.Free;
				}
			}
		}
		nativeArray.Dispose();
	}
}
