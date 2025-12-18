using System.Collections.Generic;
using Owlcat.Runtime.Core.Collections;
using Owlcat.Runtime.Visual.XPBD.Collisions.Jobs;
using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Jobs;
using UnityEngine.Jobs;

namespace Owlcat.Runtime.Visual.XPBD.Collisions;

public class ColliderWorld : EntityAllocatorWithTransforms<XpbdCollider, ColliderDescriptor>
{
	private ColliderDescriptorSoA m_Allocations;

	public override StructureOfArrays<ColliderDescriptor> Allocations => m_Allocations;

	public ColliderDescriptorSoA ColliderDescriptorSoA => m_Allocations;

	public ColliderWorld()
	{
		int size2 = 64;
		m_Allocations = CreateSoA(size2, (int size) => new ColliderDescriptorSoA(size));
	}

	protected override bool TryAlloc()
	{
		foreach (XpbdCollider addedEntity in m_AddedEntities)
		{
			if (m_Allocations.TryAlloc(1, out var offset))
			{
				ColliderDescriptor value = default(ColliderDescriptor);
				addedEntity.UpdateShape(ref value.Shape);
				value.Transform.FromTransform(addedEntity.transform);
				value.PrevTransform = value.Transform;
				value.Aabb = UpdateCollidersJob.CalculateColliderAabb(in value.Transform, in value.Shape);
				value.PrevAabb = value.Aabb;
				m_Allocations[offset] = value;
				m_EntityAllocationMap.Add(addedEntity, offset);
				continue;
			}
			return false;
		}
		return true;
	}

	protected override void Grow()
	{
		m_AddedEntities.UnionWith(m_EntityAllocationMap.Keys);
		m_EntityAllocationMap.Clear();
		if (m_AddedEntities.Count > m_Allocations.Capacity)
		{
			int newSize = (int)((float)m_AddedEntities.Count * 1.5f);
			m_Allocations.Resize(newSize);
		}
		else
		{
			m_Allocations.Reset();
		}
	}

	protected override void PushData()
	{
	}

	public override void Free()
	{
		foreach (XpbdCollider removedEntity in m_RemovedEntities)
		{
			int indexInSolver = removedEntity.IndexInSolver;
			m_Allocations.Free(indexInSolver, 1);
			m_EntityAllocationMap.Remove(removedEntity);
		}
	}

	public JobHandle UpdateColliders(JobHandle inputDep, float colliderCCD)
	{
		if (m_Allocations.Count > 0)
		{
			UpdateColliderParameters();
			UpdateCollidersJob updateCollidersJob = default(UpdateCollidersJob);
			updateCollidersJob.TransformColliderMap = m_IndicesMap;
			updateCollidersJob.ColliderShape = m_Allocations.Shape;
			updateCollidersJob.ColliderTransform = m_Allocations.Transform;
			updateCollidersJob.ColliderPrevTransform = m_Allocations.PrevTransform;
			updateCollidersJob.ColliderAabb = m_Allocations.Aabb;
			updateCollidersJob.ColliderPrevAabb = m_Allocations.PrevAabb;
			updateCollidersJob.ColliderCCD = colliderCCD;
			UpdateCollidersJob jobData = updateCollidersJob;
			inputDep = IJobParallelForTransformExtensions.ScheduleReadOnlyByRef(ref jobData, m_Transforms, 32, inputDep);
		}
		return inputDep;
	}

	private void UpdateColliderParameters()
	{
		foreach (KeyValuePair<XpbdCollider, int> item in m_EntityAllocationMap)
		{
			item.Key.UpdateShape(ref UnsafeCollectionExtensions.ElementAsRef(in m_Allocations.Shape, item.Value));
			m_Allocations.Layer[item.Value] = item.Key.gameObject.layer;
		}
	}
}
