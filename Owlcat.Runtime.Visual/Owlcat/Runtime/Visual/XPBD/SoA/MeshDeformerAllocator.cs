using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.XPBD.Authoring;
using Owlcat.Runtime.Visual.XPBD.Bodies;
using Owlcat.Runtime.Visual.XPBD.Layouts.MeshSkinning;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.SoA;

public class MeshDeformerAllocator : EntityAllocatorWithTransforms<MeshDeformer, MeshDeformerDescriptor>
{
	public MeshDeformerDescriptorSoA DescriptorsSoA;

	public MeshDeformerBindingDescriptorSoA BindingDescriptorsSoA;

	public SingleArraySoA<DeformableVertex> DeformableVerticesSoA;

	public SingleArraySoA<DeformableSkinnedVertex> DeformableSkinnedVerticesSoA;

	public SingleArraySoA<int> VertexToSkinnedVertexMapSoA;

	public Action<int2> BindingChanged;

	public override StructureOfArrays<MeshDeformerDescriptor> Allocations => DescriptorsSoA;

	public MeshDeformerAllocator()
	{
		DescriptorsSoA = CreateSoA(128, (int size) => new MeshDeformerDescriptorSoA(size));
		BindingDescriptorsSoA = CreateSoA(128, (int size) => new MeshDeformerBindingDescriptorSoA(size));
		DeformableVerticesSoA = CreateSingleArraySoA<DeformableVertex>(128);
		DeformableSkinnedVerticesSoA = CreateSingleArraySoA<DeformableSkinnedVertex>(128);
		VertexToSkinnedVertexMapSoA = CreateSingleArraySoA<int>(128);
		MeshDeformer.MasterIndexInSolverChanged = (Action<MeshDeformer>)Delegate.Combine(MeshDeformer.MasterIndexInSolverChanged, new Action<MeshDeformer>(OnMasterChanged));
	}

	public override void Dispose()
	{
		base.Dispose();
		MeshDeformer.MasterIndexInSolverChanged = (Action<MeshDeformer>)Delegate.Remove(MeshDeformer.MasterIndexInSolverChanged, new Action<MeshDeformer>(OnMasterChanged));
	}

	private void OnMasterChanged(MeshDeformer deformer)
	{
		int indexInSolver = deformer.IndexInSolver;
		int2 obj = DescriptorsSoA.BindingsRange[indexInSolver];
		for (int i = 0; i < obj.y; i++)
		{
			MeshDeformerBindingDescriptor value = BindingDescriptorsSoA[obj.x + i];
			int indexInSolver2 = deformer.Bindings[i].Master.IndexInSolver;
			int2 @int = XPBD.Solver.BodyAllocator.BodyDescriptorSoA.VerticesRange[indexInSolver2];
			value.Offsets.w = @int.x;
			value.BodyToDeformer = math.mul(DescriptorsSoA.WorldToLocal[indexInSolver], XPBD.Solver.BodyAllocator.BodyDescriptorSoA.LocalToWorld[indexInSolver2]);
			BindingDescriptorsSoA[obj.x + i] = value;
		}
		BindingChanged?.Invoke(obj);
	}

	protected override bool TryAlloc()
	{
		foreach (MeshDeformer addedEntity in m_AddedEntities)
		{
			if (DescriptorsSoA.TryAlloc(1, out var offset))
			{
				MeshDeformerDescriptor value = default(MeshDeformerDescriptor);
				value.WorldToLocal = addedEntity.transform.worldToLocalMatrix;
				value.LocalToWorld = addedEntity.transform.localToWorldMatrix;
				if (BindingDescriptorsSoA.TryAlloc(addedEntity.Bindings.Count, out var offset2))
				{
					value.BindingsRange = new int2(offset2, addedEntity.Bindings.Count);
				}
				if (DeformableVerticesSoA.TryAlloc(addedEntity.DeformableVerticesCount, out var offset3))
				{
					value.DeformableVerticesRange = new int2(offset3, addedEntity.DeformableVerticesCount);
					if (DeformableSkinnedVerticesSoA.TryAlloc(addedEntity.DeformableVerticesCount, out var offset4))
					{
						value.SkinnedVerticesRange = new int2(offset4, addedEntity.DeformableVerticesCount);
						if (VertexToSkinnedVertexMapSoA.TryAlloc(addedEntity.VertexToSkinnedVertexMap.Length, out var offset5))
						{
							value.VertexToSkinnedVertexMapRange = new int2(offset5, addedEntity.VertexToSkinnedVertexMap.Length);
							DescriptorsSoA[offset] = value;
							m_EntityAllocationMap.Add(addedEntity, offset);
							continue;
						}
						return false;
					}
					return false;
				}
				return false;
			}
			return false;
		}
		return true;
	}

	protected override void Grow()
	{
		m_AddedEntities.UnionWith(m_EntityAllocationMap.Keys);
		m_EntityAllocationMap.Clear();
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		foreach (MeshDeformer addedEntity in m_AddedEntities)
		{
			num += addedEntity.DeformableVerticesCount;
			num2 += addedEntity.VertexToSkinnedVertexMap.Length;
			num3 += addedEntity.Bindings.Count;
		}
		if (m_AddedEntities.Count > DescriptorsSoA.Capacity)
		{
			DescriptorsSoA.Resize((int)((float)m_AddedEntities.Count * 1.5f));
		}
		else
		{
			DescriptorsSoA.Reset();
		}
		if (num3 > BindingDescriptorsSoA.Capacity)
		{
			BindingDescriptorsSoA.Resize((int)((float)num3 * 1.5f));
		}
		else
		{
			BindingDescriptorsSoA.Reset();
		}
		if (num > DeformableVerticesSoA.Capacity)
		{
			DeformableVerticesSoA.Resize((int)((float)num * 1.5f));
		}
		else
		{
			DeformableVerticesSoA.Reset();
		}
		if (num > DeformableSkinnedVerticesSoA.Capacity)
		{
			DeformableSkinnedVerticesSoA.Resize((int)((float)num * 1.5f));
		}
		else
		{
			DeformableSkinnedVerticesSoA.Reset();
		}
		if (num2 > VertexToSkinnedVertexMapSoA.Capacity)
		{
			VertexToSkinnedVertexMapSoA.Resize((int)((float)num2 * 1.5f));
		}
		else
		{
			VertexToSkinnedVertexMapSoA.Reset();
		}
	}

	protected override void PushData()
	{
		foreach (MeshDeformer addedEntity in m_AddedEntities)
		{
			if (!m_EntityAllocationMap.TryGetValue(addedEntity, out var value))
			{
				continue;
			}
			MeshDeformerDescriptor meshDeformerDescriptor = DescriptorsSoA[value];
			int2 deformableVerticesRange = meshDeformerDescriptor.DeformableVerticesRange;
			int2 bindingsRange = meshDeformerDescriptor.BindingsRange;
			int num = 0;
			for (int i = 0; i < addedEntity.Bindings.Count; i++)
			{
				MeshDeformerBinding meshDeformerBinding = addedEntity.Bindings[i];
				MeshDeformerBindingDescriptor value2 = default(MeshDeformerBindingDescriptor);
				value2.Offsets = new int4(num + meshDeformerDescriptor.DeformableVerticesRange.x, num + meshDeformerDescriptor.SkinnedVerticesRange.x, meshDeformerBinding.Skinmap.SkinnedVertices.Count, -1);
				value2.BodyToDeformer = float4x4.identity;
				BindingDescriptorsSoA[bindingsRange.x + i] = value2;
				for (int j = 0; j < meshDeformerBinding.Skinmap.SkinnedVertices.Count; j++)
				{
					SlaveVertex slaveVertex = meshDeformerBinding.Skinmap.SkinnedVertices[j];
					DeformableVerticesSoA[deformableVerticesRange.x + num] = new DeformableVertex
					{
						MasterTriangleIndex = slaveVertex.MasterTriangleIndex,
						PositionBary = new float4(slaveVertex.Position.BarycentricCoords, slaveVertex.Position.Height),
						NormalBary = new float4(slaveVertex.Normal.BarycentricCoords, slaveVertex.Normal.Height),
						TangentBary = new float4(slaveVertex.Tangent.BarycentricCoords, slaveVertex.Tangent.Height),
						ParticleIndices = slaveVertex.MasterVertexIndices
					};
					num++;
				}
			}
			int2 vertexToSkinnedVertexMapRange = meshDeformerDescriptor.VertexToSkinnedVertexMapRange;
			for (int k = 0; k < vertexToSkinnedVertexMapRange.y; k++)
			{
				VertexToSkinnedVertexMapSoA[vertexToSkinnedVertexMapRange.x + k] = addedEntity.VertexToSkinnedVertexMap[k];
			}
		}
	}

	public override void Free()
	{
		foreach (MeshDeformer removedEntity in m_RemovedEntities)
		{
			int num = m_EntityAllocationMap[removedEntity];
			MeshDeformerDescriptor meshDeformerDescriptor = DescriptorsSoA[num];
			DescriptorsSoA.Free(num, 1);
			BindingDescriptorsSoA.Free(meshDeformerDescriptor.BindingsRange.x, meshDeformerDescriptor.BindingsRange.y);
			DeformableVerticesSoA.Free(meshDeformerDescriptor.DeformableVerticesRange.x, meshDeformerDescriptor.DeformableVerticesRange.y);
			DeformableSkinnedVerticesSoA.Free(meshDeformerDescriptor.SkinnedVerticesRange.x, meshDeformerDescriptor.SkinnedVerticesRange.y);
			VertexToSkinnedVertexMapSoA.Free(meshDeformerDescriptor.VertexToSkinnedVertexMapRange.x, meshDeformerDescriptor.VertexToSkinnedVertexMapRange.y);
			m_EntityAllocationMap.Remove(removedEntity);
		}
	}

	internal void UpdateMasterIndices()
	{
		foreach (KeyValuePair<MeshDeformer, int> item in m_EntityAllocationMap)
		{
			MeshDeformer key = item.Key;
			int value = item.Value;
			MeshDeformerDescriptor meshDeformerDescriptor = DescriptorsSoA[value];
			for (int i = 0; i < key.Bindings.Count; i++)
			{
				MeshDeformerBinding meshDeformerBinding = key.Bindings[i];
				if (meshDeformerBinding != null && meshDeformerBinding.Master != null)
				{
					BindingDescriptorsSoA.MasterIndexInSimulation[meshDeformerDescriptor.BindingsRange.x + i] = key.Bindings[i].Master.IndexInSolver;
					int4 value2 = BindingDescriptorsSoA.Offsets[meshDeformerDescriptor.BindingsRange.x + i];
					value2.w = XPBD.Solver.BodyAllocator.BodyDescriptorSoA.VerticesRange[meshDeformerBinding.Master.IndexInSolver].x;
					BindingDescriptorsSoA.Offsets[meshDeformerDescriptor.BindingsRange.x + i] = value2;
				}
				else
				{
					BindingDescriptorsSoA.MasterIndexInSimulation[meshDeformerDescriptor.BindingsRange.x + i] = -1;
				}
			}
		}
	}
}
