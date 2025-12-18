using Owlcat.Runtime.Core.Collections;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Utilities;
using Unity.Collections;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

public struct GPUDrivenPerInstanceMetadataWriter
{
	private readonly NativeArray<GPUDrivenPerInstanceMetadata> m_PerInstanceMetadata;

	private NativeSparseSegmentList m_DirtyMetadataSegmentList;

	public GPUDrivenPerInstanceMetadataWriter(NativeArray<GPUDrivenPerInstanceMetadata> perInstanceMetadata, NativeSparseSegmentList dirtyMetadataSegmentList)
	{
		m_PerInstanceMetadata = perInstanceMetadata;
		m_DirtyMetadataSegmentList = dirtyMetadataSegmentList;
	}

	public void Write(int instanceIndex, GPUDrivenAllocator.DataAllocation instanceAllocation, GPUDrivenAllocator.DataAllocation materialAllocation)
	{
		ref GPUDrivenPerInstanceMetadata reference = ref UnsafeCollectionExtensions.ElementAsRef(in m_PerInstanceMetadata, instanceIndex);
		reference.InstanceDataOffset = instanceAllocation.TotalOffsetOrDefault();
		reference.MaterialDataOffset = materialAllocation.TotalOffsetOrDefault();
		m_DirtyMetadataSegmentList.AddItem(instanceIndex, 1);
	}
}
