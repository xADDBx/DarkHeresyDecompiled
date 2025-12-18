using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.XPBD.Authoring;
using Owlcat.Runtime.Visual.XPBD.Bodies;
using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.GPU.Replicators;

public class MeshDeformerReplicator : ReplicatorBase<MeshDeformerAllocator>
{
	private bool m_NeedReplicate;

	private bool m_BindingsChanged;

	private BodyAllocator m_BodyAllocator;

	private int m_MeshDeformerBindingsCount;

	private GraphicsBufferWrapper<int> m_MeshDeformerIndicesBuffer;

	private GpuMeshDeformerDesctiptorSoA m_MeshDeformerDesctiptorSoA;

	private GpuMeshDeformerBindingDescriptorSoA m_MeshDeformerBindingDescriptorSoA;

	private GpuSingleArraySoA<DeformableVertex> m_MeshDeformerDeformableVerticesSoA;

	private GpuSingleArraySoA<DeformableSkinnedVertex> m_MeshDeformerDeformableSkinnedVerticesSoA;

	private GpuSingleArraySoA<int> m_MeshDeformerVertexToSkinnedVertexMapSoA;

	public int MeshDeformerBindingsCount => m_MeshDeformerBindingsCount;

	public GraphicsBufferWrapper<int> MeshDeformerIndicesBuffer => m_MeshDeformerIndicesBuffer;

	public GpuMeshDeformerBindingDescriptorSoA MeshDeformerBindingDescriptorSoA => m_MeshDeformerBindingDescriptorSoA;

	public GpuSingleArraySoA<DeformableVertex> MeshDeformerDeformableVerticesSoA => m_MeshDeformerDeformableVerticesSoA;

	public GpuSingleArraySoA<DeformableSkinnedVertex> MeshDeformerDeformableSkinnedVerticesSoA => m_MeshDeformerDeformableSkinnedVerticesSoA;

	public GpuSingleArraySoA<int> MeshDeformerVertexToSkinnedVertexMapSoA => m_MeshDeformerVertexToSkinnedVertexMapSoA;

	public GpuMeshDeformerDesctiptorSoA GpuMeshDeformerDesctiptorSoA => m_MeshDeformerDesctiptorSoA;

	public override bool IsEmpty => base.Allocator.IsEmpty;

	public MeshDeformerReplicator(MeshDeformerAllocator allocator, BodyAllocator bodyAllocator)
		: base(allocator)
	{
		m_BodyAllocator = bodyAllocator;
		m_MeshDeformerIndicesBuffer = new GraphicsBufferWrapper<int>("_XpbdMeshDeformerIndicesBuffer", 128);
		m_MeshDeformerDesctiptorSoA = CreateSoA(base.Allocator.Allocations.Capacity, (int size) => new GpuMeshDeformerDesctiptorSoA(size));
		m_MeshDeformerBindingDescriptorSoA = CreateSoA(128, (int size) => new GpuMeshDeformerBindingDescriptorSoA(size));
		m_MeshDeformerDeformableVerticesSoA = CreateSingleArraySoA<DeformableVertex>("_XpbdMeshDeformerDeformableVerticesBuffer", 128);
		m_MeshDeformerDeformableSkinnedVerticesSoA = CreateSingleArraySoA<DeformableSkinnedVertex>("_XpbdDeformableSkinnedVerticesBuffer", 128);
		m_MeshDeformerVertexToSkinnedVertexMapSoA = CreateSingleArraySoA<int>("_XpbdVertexToDeformableVertexMapBuffer", 128);
		MeshDeformerAllocator allocator2 = base.Allocator;
		allocator2.BindingChanged = (Action<int2>)Delegate.Combine(allocator2.BindingChanged, new Action<int2>(OnBindingChanged));
	}

	public override void Dispose()
	{
		base.Dispose();
		m_MeshDeformerIndicesBuffer?.Dispose();
		MeshDeformerAllocator allocator = base.Allocator;
		allocator.BindingChanged = (Action<int2>)Delegate.Remove(allocator.BindingChanged, new Action<int2>(OnBindingChanged));
	}

	public override bool Replicate(CommandBuffer cmd)
	{
		bool result = false;
		if (m_NeedReplicate)
		{
			cmd.SetBufferData(m_MeshDeformerIndicesBuffer.Buffer, base.Allocator.IndicesMap);
			m_MeshDeformerDesctiptorSoA.SetData(cmd, base.Allocator.DescriptorsSoA);
			m_MeshDeformerDeformableVerticesSoA.SetData(cmd, base.Allocator.DeformableVerticesSoA);
			m_MeshDeformerDeformableSkinnedVerticesSoA.SetData(cmd, base.Allocator.DeformableSkinnedVerticesSoA);
			m_MeshDeformerVertexToSkinnedVertexMapSoA.SetData(cmd, base.Allocator.VertexToSkinnedVertexMapSoA);
			UpdateBodyVerticesRange(cmd);
			result = true;
		}
		else if (m_BindingsChanged)
		{
			UpdateBodyVerticesRange(cmd);
			result = true;
		}
		m_NeedReplicate = false;
		m_BindingsChanged = false;
		return result;
	}

	private void UpdateBodyVerticesRange(CommandBuffer cmd)
	{
		foreach (KeyValuePair<MeshDeformer, int> item in base.Allocator.EntityAllocationMap)
		{
			int2 @int = base.Allocator.DescriptorsSoA.BindingsRange[item.Value];
			List<MeshDeformerBinding> bindings = item.Key.Bindings;
			for (int i = 0; i < bindings.Count; i++)
			{
				int indexInSolver = bindings[i].Master.IndexInSolver;
				int2 int2 = m_BodyAllocator.BodyDescriptorSoA.VerticesRange[indexInSolver];
				int4 value = base.Allocator.BindingDescriptorsSoA.Offsets[@int.x + i];
				value.w = int2.x;
				base.Allocator.BindingDescriptorsSoA.Offsets[@int.x + i] = value;
			}
		}
		cmd.SetBufferData(m_MeshDeformerBindingDescriptorSoA.Offsets.Buffer, base.Allocator.BindingDescriptorsSoA.Offsets);
	}

	protected override void OnAfterAlloc()
	{
		m_MeshDeformerIndicesBuffer.Resize(base.Allocator.IndicesMap.Length);
		m_MeshDeformerDesctiptorSoA.Resize(base.Allocator.Allocations.Capacity);
		m_MeshDeformerBindingDescriptorSoA.Resize(base.Allocator.BindingDescriptorsSoA.Capacity);
		m_MeshDeformerDeformableVerticesSoA.Resize(base.Allocator.DeformableVerticesSoA.Capacity);
		m_MeshDeformerDeformableSkinnedVerticesSoA.Resize(base.Allocator.DeformableSkinnedVerticesSoA.Capacity);
		m_MeshDeformerVertexToSkinnedVertexMapSoA.Resize(base.Allocator.VertexToSkinnedVertexMapSoA.Capacity);
		m_NeedReplicate = base.Allocator.AddedEntities.Count > 0 || base.Allocator.RemovedEntities.Count > 0;
	}

	private void OnBindingChanged(int2 bindingsRange)
	{
		m_BindingsChanged = true;
	}
}
