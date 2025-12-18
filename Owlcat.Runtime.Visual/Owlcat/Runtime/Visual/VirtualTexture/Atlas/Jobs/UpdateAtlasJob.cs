using System;
using Owlcat.Runtime.Core.Allocators.Guillotiere;
using Owlcat.Runtime.Core.Collections;
using Owlcat.Runtime.Visual.VirtualTexture.Materials;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Owlcat.Runtime.Visual.VirtualTexture.Atlas.Jobs;

[BurstCompile]
internal struct UpdateAtlasJob : IJob
{
	public NativeList<VirtualAtlasEntry> AtlasEntries;

	[ReadOnly]
	public NativeParallelHashSet<MaterialStackIdMapping> InvalidMaterialStackIdMappings;

	public NativeList<EntryToAllocate> DataToAllocate;

	public NativeParallelHashMap<int, MaterialStackIndices> MaterialStackIndices;

	public NativeParallelHashMap<TextureStackId, int> TextureStackIndices;

	[NativeDisableUnsafePtrRestriction]
	public unsafe NativeAtlasAllocator* Allocator;

	public unsafe void Execute()
	{
		while (AtlasEntries.Length < Allocator->Nodes.Length)
		{
			ref NativeList<VirtualAtlasEntry> atlasEntries = ref AtlasEntries;
			VirtualAtlasEntry value = default(VirtualAtlasEntry);
			atlasEntries.Add(in value);
		}
		Span<EntryToAllocate> span = DataToAllocate.AsArray().AsSpan();
		for (int i = 0; i < span.Length; i++)
		{
			ref EntryToAllocate reference = ref span[i];
			int indexInAllocator = reference.IndexInAllocator;
			reference.Entry.RectAllocId = reference.AllocId;
			reference.Entry.RectInTiles = reference.Rect;
			AtlasEntries[indexInAllocator] = reference.Entry;
			TextureStackIndices[reference.Entry.StackId] = indexInAllocator;
		}
		for (int j = 0; j < AtlasEntries.Length; j++)
		{
			ref VirtualAtlasEntry reference2 = ref UnsafeCollectionExtensions.ElementAsRef(in AtlasEntries, j);
			NativeList<Node> nativeArray = Allocator->Nodes;
			reference2.NodeKind = UnsafeCollectionExtensions.ElementAsRef(in nativeArray, j).Kind;
		}
		foreach (MaterialStackIdMapping invalidMaterialStackIdMapping in InvalidMaterialStackIdMappings)
		{
			int value2 = TextureStackIndices[invalidMaterialStackIdMapping.StackId];
			MaterialStackIndices value3 = MaterialStackIndices[invalidMaterialStackIdMapping.MaterialId];
			value3[invalidMaterialStackIdMapping.IndexInMaterial] = value2;
			MaterialStackIndices[invalidMaterialStackIdMapping.MaterialId] = value3;
		}
	}
}
