using System.Collections.Generic;
using Owlcat.Runtime.Visual.XPBD.Authoring;
using Owlcat.Runtime.Visual.XPBD.ParticleAttachments.Jobs;
using Owlcat.Runtime.Visual.XPBD.Particles;
using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace Owlcat.Runtime.Visual.XPBD.ParticleAttachments;

public class ParticleAttachmentAllocator : EntityAllocatorWithTransforms<ParticleAttachment, ParticleAttachmentDescriptor>
{
	private ParticleAttachmentDeactivator m_Deactivator;

	public ParticleAttachmentDescriptorSoA DescriptorSoA;

	public ParticleAttachmentDataSoA DataSoA;

	public override StructureOfArrays<ParticleAttachmentDescriptor> Allocations => DescriptorSoA;

	public ParticleAttachmentDeactivator Deactivator => m_Deactivator;

	public ParticleAttachmentAllocator()
	{
		m_Deactivator = new ParticleAttachmentDeactivator();
		DescriptorSoA = CreateSoA(64, (int size) => new ParticleAttachmentDescriptorSoA(size));
		DataSoA = CreateSoA(128, (int size) => new ParticleAttachmentDataSoA(size));
	}

	public override void Dispose()
	{
		base.Dispose();
		m_Deactivator.Dispose();
	}

	public JobHandle UpdateAttachmentTransforms(BodyAllocator bodyAllocator, JobHandle inputDep)
	{
		if (m_EntityAllocationMap.Count > 0)
		{
			foreach (KeyValuePair<ParticleAttachment, int> item in m_EntityAllocationMap)
			{
				int value = item.Value;
				if (bodyAllocator.EntityAllocationMap.TryGetValue(item.Key.Body, out var value2))
				{
					DescriptorSoA.BodyParticlesRange[value] = bodyAllocator.BodyDescriptorSoA.ParticlesRange[value2];
				}
				else
				{
					DescriptorSoA.BodyParticlesRange[value] = -1;
				}
			}
			UpdateTransformsJob updateTransformsJob = default(UpdateTransformsJob);
			updateTransformsJob.TransformAttachmentMap = m_IndicesMap;
			updateTransformsJob.LocalToWorld = DescriptorSoA.LocalToWorld;
			UpdateTransformsJob jobData = updateTransformsJob;
			inputDep = IJobParallelForTransformExtensions.ScheduleReadOnlyByRef(ref jobData, m_Transforms, 32, inputDep);
		}
		return inputDep;
	}

	internal JobHandle RestoreParticles(BodyAllocator bodyAllocator, JobHandle inputDep)
	{
		if (m_Deactivator.HasParticlesForRestore())
		{
			RestorePaticlesJob restorePaticlesJob = default(RestorePaticlesJob);
			restorePaticlesJob.ParticleIndexToRestore = m_Deactivator.ParticleIndices;
			restorePaticlesJob.ParticleInvMassToRestore = m_Deactivator.ParticleInvMass;
			restorePaticlesJob.ParticleInvMass = bodyAllocator.ParticleSoA.InvMass;
			RestorePaticlesJob jobData = restorePaticlesJob;
			inputDep = IJobParallelForExtensions.ScheduleByRef(ref jobData, m_Deactivator.ParticleIndices.Length, 32, inputDep);
		}
		return inputDep;
	}

	public JobHandle UpdateAttachments(BodyAllocator bodyAllocator, JobHandle inputDep)
	{
		if (m_EntityAllocationMap.Count > 0)
		{
			UpdateAttachmentsJob updateAttachmentsJob = default(UpdateAttachmentsJob);
			updateAttachmentsJob.AttachmentIndicesMap = m_IndicesMap;
			updateAttachmentsJob.AttachmentParticleDataRange = DescriptorSoA.ParticleDataRange;
			updateAttachmentsJob.AttachmentLocalToWorld = DescriptorSoA.LocalToWorld;
			updateAttachmentsJob.BodyParticleRange = DescriptorSoA.BodyParticlesRange;
			updateAttachmentsJob.AttachmentParticleIndices = DataSoA.IndexInBody;
			updateAttachmentsJob.AttachmentParticleOffsets = DataSoA.PositionOffset;
			updateAttachmentsJob.ParticlePosition = bodyAllocator.ParticleSoA.Position;
			updateAttachmentsJob.ParticleInvMass = bodyAllocator.ParticleSoA.InvMass;
			UpdateAttachmentsJob jobData = updateAttachmentsJob;
			inputDep = IJobParallelForExtensions.ScheduleByRef(ref jobData, m_EntityAllocationMap.Count, 16, inputDep);
		}
		return inputDep;
	}

	protected override bool TryAlloc()
	{
		foreach (ParticleAttachment addedEntity in m_AddedEntities)
		{
			if (DescriptorSoA.TryAlloc(1, out var offset))
			{
				ParticleAttachmentDescriptor value = default(ParticleAttachmentDescriptor);
				if (DataSoA.TryAlloc(addedEntity.ParticleGroup.Count, out var offset2))
				{
					value.ParticleDataRange = new int2(offset2, addedEntity.ParticleGroup.Count);
					DescriptorSoA[offset] = value;
					m_EntityAllocationMap.Add(addedEntity, offset);
					continue;
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
		foreach (ParticleAttachment addedEntity in m_AddedEntities)
		{
			num += addedEntity.ParticleGroup.Count;
		}
		if (m_AddedEntities.Count > DescriptorSoA.Capacity)
		{
			DescriptorSoA.Resize((int)((float)m_AddedEntities.Count * 1.5f));
		}
		else
		{
			DescriptorSoA.Reset();
		}
		if (num > DataSoA.Capacity)
		{
			DataSoA.Resize((int)((float)num * 1.5f));
		}
		else
		{
			DataSoA.Reset();
		}
	}

	protected override void PushData()
	{
		foreach (ParticleAttachment addedEntity in m_AddedEntities)
		{
			if (m_EntityAllocationMap.TryGetValue(addedEntity, out var value))
			{
				ParticleAttachmentDescriptor particleAttachmentDescriptor = DescriptorSoA[value];
				Matrix4x4 matrix4x = addedEntity.Target.worldToLocalMatrix * addedEntity.Body.transform.localToWorldMatrix;
				_ = matrix4x.rotation;
				int2 particleDataRange = particleAttachmentDescriptor.ParticleDataRange;
				for (int i = 0; i < particleDataRange.y; i++)
				{
					DataSoA.IndexInBody[particleDataRange.x + i] = addedEntity.ParticleGroup.ParticleIndices[i];
					Particle particle = addedEntity.Body.LayoutBase.BodyStructure.Particles[addedEntity.ParticleGroup.ParticleIndices[i]];
					DataSoA.PositionOffset[particleDataRange.x + i] = matrix4x.MultiplyPoint3x4((Vector3)particle.BasePosition);
				}
			}
		}
	}

	public override void Free()
	{
		foreach (ParticleAttachment removedEntity in m_RemovedEntities)
		{
			int num = m_EntityAllocationMap[removedEntity];
			ParticleAttachmentDescriptor descriptor = DescriptorSoA[num];
			DescriptorSoA.Free(num, 1);
			DataSoA.Free(descriptor.ParticleDataRange.x, descriptor.ParticleDataRange.y);
			m_EntityAllocationMap.Remove(removedEntity);
			m_Deactivator.AddParticlesForRestore(removedEntity, in descriptor);
		}
	}

	internal void EndStep()
	{
		if (m_Deactivator.HasParticlesForRestore())
		{
			m_Deactivator.ClearParticlesForRestore();
		}
	}
}
