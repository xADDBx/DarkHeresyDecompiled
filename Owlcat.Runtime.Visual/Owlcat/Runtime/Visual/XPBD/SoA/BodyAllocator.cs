using System.Text;
using Owlcat.Runtime.Visual.XPBD.Authoring;
using Owlcat.Runtime.Visual.XPBD.Bodies;
using Owlcat.Runtime.Visual.XPBD.Constraints;
using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Owlcat.Runtime.Visual.XPBD.Particles;
using Owlcat.Runtime.Visual.XPBD.Stats;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.XPBD.SoA;

public class BodyAllocator : EntityAllocatorWithTransforms<AuthoringBase, BodyDescriptor>
{
	private bool m_AllocAfterGrow;

	public BodyDescriptorSoA BodyDescriptorSoA;

	public ParticleSoA ParticleSoA;

	public ConstraintSoA ConstraintSoA;

	public SingleArraySoA<int3> ConstraintBatchSoA;

	public SingleArraySoA<float4> ConstraintSettingsSoA;

	public SingleArraySoA<int> VertexToParticleMapSoA;

	public SingleArraySoA<int> TrianglesSoA;

	public SingleArraySoA<int> VertexTrianglesMapSoA;

	public SingleArraySoA<int2> VertexTrianglesMapRangesSoA;

	public BodyVertexSoA VerticesSoA;

	public MeshVertexSoA MeshLocalVerticesSoA;

	public SingleArraySoA<int> ParticleToVertexMapSoA;

	public BodyBoneSoA BonesSoA;

	public SingleArraySoA<int> BoneIndicesMapSoA;

	public SingleArraySoA<int2> BoneIndicesMapRangesSoA;

	public override StructureOfArrays<BodyDescriptor> Allocations => BodyDescriptorSoA;

	public BodyAllocator()
	{
		BodyDescriptorSoA = CreateSoA(128, (int size) => new BodyDescriptorSoA(size));
		ParticleSoA = CreateSoA(128, (int size) => new ParticleSoA(size));
		ConstraintSoA = CreateSoA(128, (int size) => new ConstraintSoA(size));
		ConstraintBatchSoA = CreateSingleArraySoA<int3>(128);
		ConstraintSettingsSoA = CreateSingleArraySoA<float4>(128);
		VertexToParticleMapSoA = CreateSingleArraySoA<int>(128);
		VerticesSoA = CreateSoA(128, (int size) => new BodyVertexSoA(size));
		TrianglesSoA = CreateSingleArraySoA<int>(128);
		VertexTrianglesMapSoA = CreateSingleArraySoA<int>(128);
		VertexTrianglesMapRangesSoA = CreateSingleArraySoA<int2>(128);
		VerticesSoA = CreateSoA(128, (int size) => new BodyVertexSoA(128));
		MeshLocalVerticesSoA = CreateSoA(128, (int size) => new MeshVertexSoA(size));
		ParticleToVertexMapSoA = CreateSingleArraySoA<int>(128);
		BonesSoA = CreateSoA(128, (int size) => new BodyBoneSoA(size));
		BoneIndicesMapSoA = CreateSingleArraySoA<int>(128);
		BoneIndicesMapRangesSoA = CreateSingleArraySoA<int2>(128);
	}

	internal void SyncAuthoringEnabledState(AuthoringBase authoringBase)
	{
		BodyDescriptorSoA.Enabled[authoringBase.IndexInSolver] = (authoringBase.isActiveAndEnabled ? 1 : 0);
	}

	protected override bool TryAlloc()
	{
		foreach (AuthoringBase addedEntity in m_AddedEntities)
		{
			if (BodyDescriptorSoA.TryAlloc(1, out var offset))
			{
				BodyDescriptor value = default(BodyDescriptor);
				if (ParticleSoA.TryAlloc(addedEntity.LayoutBase.BodyStructure.Particles.Count, out var offset2))
				{
					value.ParticlesRange = new int2(offset2, addedEntity.LayoutBase.BodyStructure.Particles.Count);
					if (ConstraintSoA.TryAlloc(addedEntity.LayoutBase.BodyStructure.Constraints.Count, out var offset3))
					{
						value.ConstraintsRange = new int2(offset3, addedEntity.LayoutBase.BodyStructure.Constraints.Count);
						if (ConstraintBatchSoA.TryAlloc(addedEntity.LayoutBase.BodyStructure.ConstraintBatches.Count, out var offset4))
						{
							value.ConstraintBatchesRange = new int2(offset4, addedEntity.LayoutBase.BodyStructure.ConstraintBatches.Count);
							if (addedEntity.LayoutBase.BodyStructure.SimplexOffsetCount.x > -1)
							{
								value.SimplexConstraintsRange = addedEntity.LayoutBase.BodyStructure.SimplexOffsetCount;
							}
							else
							{
								value.SimplexConstraintsRange = -1;
							}
							if (ConstraintSettingsSoA.TryAlloc(ConstraintSettingsCollection.ConstraintTypeCount, out var offset5))
							{
								value.ConstraintSettingsRange = new int2(offset5, ConstraintSettingsCollection.ConstraintTypeCount);
								if (addedEntity.LayoutBase.BodyStructure.VertexToParticlesMap != null && addedEntity.LayoutBase.BodyStructure.VertexToParticlesMap.Count > 0)
								{
									if (!VertexToParticleMapSoA.TryAlloc(addedEntity.LayoutBase.BodyStructure.VertexToParticlesMap.Count, out var offset6))
									{
										AllocAfterGrowAssert();
										return false;
									}
									value.VertexToParticleRange = new int2(offset6, addedEntity.LayoutBase.BodyStructure.VertexToParticlesMap.Count);
									if (!TrianglesSoA.TryAlloc(addedEntity.LayoutBase.BodyStructure.Triangles.Count, out var offset7))
									{
										AllocAfterGrowAssert();
										return false;
									}
									value.TrianglesRange = new int2(offset7, addedEntity.LayoutBase.BodyStructure.Triangles.Count);
									if (!VertexTrianglesMapSoA.TryAlloc(addedEntity.LayoutBase.BodyStructure.VertexTrianglesMap.Count, out var offset8))
									{
										AllocAfterGrowAssert();
										return false;
									}
									value.VertexTrianglesMapRange = new int2(offset8, addedEntity.LayoutBase.BodyStructure.VertexTrianglesMap.Count);
									if (VertexTrianglesMapRangesSoA.TryAlloc(addedEntity.LayoutBase.BodyStructure.VertexTrianglesMapRanges.Count, out var offset9))
									{
										value.VertexTrianglesMapRangesRange = new int2(offset9, addedEntity.LayoutBase.BodyStructure.VertexTrianglesMapRanges.Count);
									}
									if (!VerticesSoA.TryAlloc(addedEntity.LayoutBase.BodyStructure.RestNormals.Count, out var offset10))
									{
										AllocAfterGrowAssert();
										return false;
									}
									value.VerticesRange = new int2(offset10, addedEntity.LayoutBase.BodyStructure.RestNormals.Count);
									if (addedEntity.LayoutBase.BodyStructure.UseSkin)
									{
										if (!ParticleToVertexMapSoA.TryAlloc(addedEntity.LayoutBase.BodyStructure.ParticleToVertexMap.Count, out var offset11))
										{
											AllocAfterGrowAssert();
											return false;
										}
										value.ParticleToVertexRange = new int2(offset11, addedEntity.LayoutBase.BodyStructure.ParticleToVertexMap.Count);
										value.SkinBufferRange = addedEntity.LayoutBase.BodyStructure.SkinBufferRange;
									}
									else
									{
										value.SkinBufferRange = -1;
										value.ParticleToVertexRange.x = -1;
										if (!MeshLocalVerticesSoA.TryAlloc(addedEntity.LayoutBase.BodyStructure.Particles.Count, out var offset12))
										{
											AllocAfterGrowAssert();
											return false;
										}
										value.MeshLocalVerticesRange = new int2(offset12, addedEntity.LayoutBase.BodyStructure.Particles.Count);
									}
								}
								else
								{
									value.VertexToParticleRange = -1;
									value.ParticleToVertexRange = -1;
									value.VerticesRange = -1;
								}
								if (addedEntity.LayoutBase.BodyStructure.Bones != null && addedEntity.LayoutBase.BodyStructure.Bones.Count > 0)
								{
									if (!BonesSoA.TryAlloc(addedEntity.LayoutBase.BodyStructure.Bones.Count, out var offset13))
									{
										AllocAfterGrowAssert();
										return false;
									}
									value.BonesRange = new int2(offset13, addedEntity.LayoutBase.BodyStructure.Bones.Count);
									if (!BoneIndicesMapSoA.TryAlloc(addedEntity.LayoutBase.BodyStructure.MeshBonesMap.Count, out var offset14))
									{
										AllocAfterGrowAssert();
										return false;
									}
									value.BoneIndicesMapRange = new int2(offset14, addedEntity.LayoutBase.BodyStructure.MeshBonesMap.Count);
									if (!BoneIndicesMapRangesSoA.TryAlloc(addedEntity.LayoutBase.BodyStructure.MeshBonesMapRanges.Count, out var offset15))
									{
										AllocAfterGrowAssert();
										return false;
									}
									value.BoneIndicesMapRangesRange = new int2(offset15, addedEntity.LayoutBase.BodyStructure.MeshBonesMapRanges.Count);
								}
								else
								{
									value.BonesRange = -1;
								}
								value.EnabledConstraintTypeMask = addedEntity.GetEnabledConstraintTypeMask();
								Transform transform = addedEntity.GetTransform();
								value.WorldToLocal = transform.worldToLocalMatrix;
								value.LocalToWorld = transform.localToWorldMatrix;
								value.PrevWorldToLocal = value.WorldToLocal;
								value.BodySimulationParameters = addedEntity.BodySimulationParameters.GetPackedParameters();
								value.InertialFrame = InertialFrame.CreateFromTransform(transform);
								value.Enabled = (addedEntity.isActiveAndEnabled ? 1 : 0);
								BodyDescriptorSoA[offset] = value;
								m_EntityAllocationMap.Add(addedEntity, offset);
								continue;
							}
							AllocAfterGrowAssert();
							return false;
						}
						AllocAfterGrowAssert();
						return false;
					}
					AllocAfterGrowAssert();
					return false;
				}
				AllocAfterGrowAssert();
				return false;
			}
			AllocAfterGrowAssert();
			return false;
		}
		m_AllocAfterGrow = false;
		return true;
	}

	private void AllocAfterGrowAssert()
	{
		_ = m_AllocAfterGrow;
	}

	protected override void Grow()
	{
		m_AddedEntities.UnionWith(m_EntityAllocationMap.Keys);
		m_EntityAllocationMap.Clear();
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		int num8 = 0;
		int num9 = 0;
		int num10 = 0;
		int num11 = 0;
		int num12 = 0;
		int num13 = 0;
		int num14 = 0;
		int num15 = 0;
		foreach (AuthoringBase addedEntity in m_AddedEntities)
		{
			num += addedEntity.LayoutBase.BodyStructure.Particles.Count;
			num2 += addedEntity.LayoutBase.BodyStructure.Constraints.Count;
			num3 += addedEntity.LayoutBase.BodyStructure.ConstraintBatches.Count;
			num4 += ConstraintSettingsCollection.ConstraintTypeCount;
			num5 += addedEntity.LayoutBase.BodyStructure.VertexToParticlesMap?.Count ?? 0;
			num6 += addedEntity.LayoutBase.BodyStructure.Triangles?.Count ?? 0;
			num7 += addedEntity.LayoutBase.BodyStructure.VertexTrianglesMap?.Count ?? 0;
			num8 += addedEntity.LayoutBase.BodyStructure.VertexTrianglesMapRanges?.Count ?? 0;
			num9 += addedEntity.LayoutBase.BodyStructure.RestNormals?.Count ?? 0;
			if (addedEntity.LayoutBase.BodyStructure.UseSkin)
			{
				num10 += addedEntity.LayoutBase.BodyStructure.ParticleToVertexMap?.Count ?? 0;
				num12 += addedEntity.LayoutBase.BodyStructure.Particles.Count;
			}
			else
			{
				num11 += addedEntity.LayoutBase.BodyStructure.Particles.Count;
			}
			num13 += addedEntity.LayoutBase.BodyStructure.Bones?.Count ?? 0;
			num14 += addedEntity.LayoutBase.BodyStructure.MeshBonesMap?.Count ?? 0;
			num15 += addedEntity.LayoutBase.BodyStructure.MeshBonesMapRanges?.Count ?? 0;
		}
		if (m_AddedEntities.Count > BodyDescriptorSoA.Capacity)
		{
			BodyDescriptorSoA.Resize((int)((float)m_AddedEntities.Count * 1.5f));
		}
		else
		{
			BodyDescriptorSoA.Reset();
		}
		if (num > ParticleSoA.Capacity)
		{
			ParticleSoA.Resize((int)((float)num * 1.5f));
		}
		else
		{
			ParticleSoA.Reset();
		}
		if (num2 > ConstraintSoA.Capacity)
		{
			ConstraintSoA.Resize((int)((float)num2 * 1.5f));
		}
		else
		{
			ConstraintSoA.Reset();
		}
		if (num3 > ConstraintBatchSoA.Capacity)
		{
			ConstraintBatchSoA.Resize((int)((float)num3 * 1.5f));
		}
		else
		{
			ConstraintBatchSoA.Reset();
		}
		if (num4 > ConstraintSettingsSoA.Capacity)
		{
			ConstraintSettingsSoA.Resize((int)((float)num4 * 1.5f));
		}
		else
		{
			ConstraintSettingsSoA.Reset();
		}
		if (num5 > VertexToParticleMapSoA.Capacity)
		{
			VertexToParticleMapSoA.Resize((int)((float)num5 * 1.5f));
		}
		else
		{
			VertexToParticleMapSoA.Reset();
		}
		if (num6 > TrianglesSoA.Capacity)
		{
			TrianglesSoA.Resize((int)((float)num6 * 1.5f));
		}
		else
		{
			TrianglesSoA.Reset();
		}
		if (num7 > VertexTrianglesMapSoA.Capacity)
		{
			VertexTrianglesMapSoA.Resize((int)((float)num7 * 1.5f));
		}
		else
		{
			VertexTrianglesMapSoA.Reset();
		}
		if (num8 > VertexTrianglesMapRangesSoA.Capacity)
		{
			VertexTrianglesMapRangesSoA.Resize((int)((float)num8 * 1.5f));
		}
		else
		{
			VertexTrianglesMapRangesSoA.Reset();
		}
		if (num9 > VerticesSoA.Capacity)
		{
			VerticesSoA.Resize((int)((float)num9 * 1.5f));
		}
		else
		{
			VerticesSoA.Reset();
		}
		if (num11 > MeshLocalVerticesSoA.Capacity)
		{
			MeshLocalVerticesSoA.Resize((int)((float)num11 * 1.5f));
		}
		else
		{
			MeshLocalVerticesSoA.Reset();
		}
		if (num10 > ParticleToVertexMapSoA.Capacity)
		{
			ParticleToVertexMapSoA.Resize((int)((float)num10 * 1.5f));
		}
		else
		{
			ParticleToVertexMapSoA.Reset();
		}
		if (num13 > BonesSoA.Capacity)
		{
			BonesSoA.Resize((int)((float)num13 * 1.5f));
		}
		else
		{
			BonesSoA.Reset();
		}
		if (num14 > BoneIndicesMapSoA.Capacity)
		{
			BoneIndicesMapSoA.Resize((int)((float)num14 * 1.5f));
		}
		else
		{
			BoneIndicesMapSoA.Reset();
		}
		if (num15 > BoneIndicesMapRangesSoA.Capacity)
		{
			BoneIndicesMapRangesSoA.Resize((int)((float)num15 * 1.5f));
		}
		else
		{
			BoneIndicesMapRangesSoA.Reset();
		}
		m_AllocAfterGrow = true;
	}

	protected override void PushData()
	{
		foreach (AuthoringBase addedEntity in m_AddedEntities)
		{
			if (m_EntityAllocationMap.TryGetValue(addedEntity, out var value))
			{
				Matrix4x4 localToWorldMatrix = addedEntity.transform.localToWorldMatrix;
				_ = localToWorldMatrix.rotation;
				BodyDescriptor bodyDescriptor = BodyDescriptorSoA[value];
				for (int i = 0; i < bodyDescriptor.ParticlesRange.y; i++)
				{
					Particle value2 = addedEntity.LayoutBase.BodyStructure.Particles[i];
					value2.BasePosition = localToWorldMatrix.MultiplyPoint3x4(value2.BasePosition);
					value2.Position = value2.BasePosition;
					value2.PrevPosition = value2.Position;
					value2.JacobiPosDelta = int3.zero;
					value2.JacobiPosCount = 0;
					ParticleSoA[bodyDescriptor.ParticlesRange.x + i] = value2;
				}
				for (int j = 0; j < bodyDescriptor.ConstraintsRange.y; j++)
				{
					ConstraintSoA[bodyDescriptor.ConstraintsRange.x + j] = addedEntity.LayoutBase.BodyStructure.Constraints[j];
				}
				for (int k = 0; k < bodyDescriptor.ConstraintBatchesRange.y; k++)
				{
					ConstraintBatch constraintBatch = addedEntity.LayoutBase.BodyStructure.ConstraintBatches[k];
					int3 value3 = new int3(constraintBatch.OffsetCount.x + bodyDescriptor.ConstraintsRange.x, constraintBatch.OffsetCount.y, (int)constraintBatch.Type);
					ConstraintBatchSoA[bodyDescriptor.ConstraintBatchesRange.x + k] = value3;
				}
				for (int l = 0; l < ConstraintSettingsCollection.ConstraintTypeCount; l++)
				{
					ConstraintSettingsSoA[bodyDescriptor.ConstraintSettingsRange.x + l] = addedEntity.ConstraintSettings.GetPackedSettings((ConstraintType)l);
				}
				float3 min = addedEntity.LayoutBase.BodyStructure.Bounds.min;
				float3 max = addedEntity.LayoutBase.BodyStructure.Bounds.max;
				Aabb value4 = new Aabb(in min, in max);
				Matrix4x4 transform = addedEntity.transform.localToWorldMatrix;
				value4.Transform(in transform);
				BodyDescriptorSoA.Aabb[value] = value4;
				if (bodyDescriptor.HasVertices)
				{
					for (int m = 0; m < bodyDescriptor.VertexToParticleRange.y; m++)
					{
						VertexToParticleMapSoA[bodyDescriptor.VertexToParticleRange.x + m] = addedEntity.LayoutBase.BodyStructure.VertexToParticlesMap[m];
					}
					for (int n = 0; n < bodyDescriptor.TrianglesRange.y; n++)
					{
						TrianglesSoA[bodyDescriptor.TrianglesRange.x + n] = addedEntity.LayoutBase.BodyStructure.Triangles[n];
					}
					for (int num = 0; num < bodyDescriptor.VertexTrianglesMapRange.y; num++)
					{
						VertexTrianglesMapSoA[bodyDescriptor.VertexTrianglesMapRange.x + num] = addedEntity.LayoutBase.BodyStructure.VertexTrianglesMap[num];
					}
					for (int num2 = 0; num2 < bodyDescriptor.VertexTrianglesMapRangesRange.y; num2++)
					{
						VertexTrianglesMapRangesSoA[bodyDescriptor.VertexTrianglesMapRangesRange.x + num2] = addedEntity.LayoutBase.BodyStructure.VertexTrianglesMapRanges[num2];
					}
					for (int num3 = 0; num3 < bodyDescriptor.VerticesRange.y; num3++)
					{
						float3 @float = localToWorldMatrix.MultiplyVector(addedEntity.LayoutBase.BodyStructure.RestNormals[num3]);
						VerticesSoA[bodyDescriptor.VerticesRange.x + num3] = new BodyVertex
						{
							Normal = @float,
							RestNormal = @float,
							Position = addedEntity.LayoutBase.BodyStructure.Particles[num3].Position
						};
					}
					if (bodyDescriptor.HasSkinnedVertices)
					{
						for (int num4 = 0; num4 < bodyDescriptor.ParticleToVertexRange.y; num4++)
						{
							ParticleToVertexMapSoA[bodyDescriptor.ParticleToVertexRange.x + num4] = addedEntity.LayoutBase.BodyStructure.ParticleToVertexMap[num4];
						}
					}
					else
					{
						for (int num5 = 0; num5 < bodyDescriptor.MeshLocalVerticesRange.y; num5++)
						{
							MeshLocalVerticesSoA.Position[bodyDescriptor.MeshLocalVerticesRange.x + num5] = addedEntity.LayoutBase.BodyStructure.Particles[num5].BasePosition;
							MeshLocalVerticesSoA.Normal[bodyDescriptor.MeshLocalVerticesRange.x + num5] = addedEntity.LayoutBase.BodyStructure.RestNormals[num5];
						}
					}
				}
				if (bodyDescriptor.HasBones)
				{
					for (int num6 = 0; num6 < bodyDescriptor.BonesRange.y; num6++)
					{
						BonesSoA[bodyDescriptor.BonesRange.x + num6] = addedEntity.LayoutBase.BodyStructure.Bones[num6];
					}
					for (int num7 = 0; num7 < bodyDescriptor.BoneIndicesMapRange.y; num7++)
					{
						BoneIndicesMapSoA[bodyDescriptor.BoneIndicesMapRange.x + num7] = addedEntity.LayoutBase.BodyStructure.MeshBonesMap[num7];
					}
					for (int num8 = 0; num8 < bodyDescriptor.BoneIndicesMapRangesRange.y; num8++)
					{
						BoneIndicesMapRangesSoA[bodyDescriptor.BoneIndicesMapRangesRange.x + num8] = addedEntity.LayoutBase.BodyStructure.MeshBonesMapRanges[num8];
					}
				}
			}
			else
			{
				UnityEngine.Debug.LogError("Can't find body descriptor index: " + addedEntity.name, addedEntity);
			}
		}
	}

	public override void Free()
	{
		foreach (AuthoringBase removedEntity in m_RemovedEntities)
		{
			int num = m_EntityAllocationMap[removedEntity];
			m_EntityAllocationMap.Remove(removedEntity);
			BodyDescriptor bodyDescriptor = BodyDescriptorSoA[num];
			BodyDescriptorSoA.Free(num, 1);
			ParticleSoA.Free(bodyDescriptor.ParticlesRange.x, bodyDescriptor.ParticlesRange.y);
			ConstraintSoA.Free(bodyDescriptor.ConstraintsRange.x, bodyDescriptor.ConstraintsRange.y);
			ConstraintBatchSoA.Free(bodyDescriptor.ConstraintBatchesRange.x, bodyDescriptor.ConstraintBatchesRange.y);
			ConstraintSettingsSoA.Free(bodyDescriptor.ConstraintSettingsRange.x, bodyDescriptor.ConstraintSettingsRange.y);
			if (bodyDescriptor.HasVertices)
			{
				VertexToParticleMapSoA.Free(bodyDescriptor.VertexToParticleRange.x, bodyDescriptor.VertexToParticleRange.y);
				TrianglesSoA.Free(bodyDescriptor.TrianglesRange.x, bodyDescriptor.TrianglesRange.y);
				VertexTrianglesMapSoA.Free(bodyDescriptor.VertexTrianglesMapRange.x, bodyDescriptor.VertexTrianglesMapRange.y);
				VertexTrianglesMapRangesSoA.Free(bodyDescriptor.VertexTrianglesMapRangesRange.x, bodyDescriptor.VertexTrianglesMapRangesRange.y);
				VerticesSoA.Free(bodyDescriptor.VerticesRange.x, bodyDescriptor.VerticesRange.y);
				if (bodyDescriptor.HasSkinnedVertices)
				{
					ParticleToVertexMapSoA.Free(bodyDescriptor.ParticleToVertexRange.x, bodyDescriptor.ParticleToVertexRange.y);
				}
				else
				{
					MeshLocalVerticesSoA.Free(bodyDescriptor.MeshLocalVerticesRange.x, bodyDescriptor.MeshLocalVerticesRange.y);
				}
			}
			if (bodyDescriptor.HasBones)
			{
				BonesSoA.Free(bodyDescriptor.BonesRange.x, bodyDescriptor.BonesRange.y);
				BoneIndicesMapSoA.Free(bodyDescriptor.BoneIndicesMapRange.x, bodyDescriptor.BoneIndicesMapRange.y);
				BoneIndicesMapRangesSoA.Free(bodyDescriptor.BoneIndicesMapRangesRange.x, bodyDescriptor.BoneIndicesMapRangesRange.y);
			}
		}
	}

	protected override void BuildInternalData()
	{
		base.BuildInternalData();
		for (int i = 0; i < ParticleSoA.Capacity; i++)
		{
			ParticleSoA.JacobiPosDelta[i] = int3.zero;
			ParticleSoA.JacobiPosCount[i] = 0;
		}
	}

	public string GetDetailedMemoryStats()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine($"BodyAllocator memory stats: {m_AddedEntities.Count} bodies allocated");
		object arg = BodyDescriptorSoA.Capacity;
		object arg2 = BodyDescriptorSoA.Count;
		int memorySize = BodyDescriptorSoA.GetSizeInBytes();
		stringBuilder.AppendLine($"BodyDescriptorSoA: {arg} capacity, {arg2} count, {MemoryStats.MemoryToString(in memorySize)} size");
		object arg3 = ParticleSoA.Capacity;
		object arg4 = ParticleSoA.Count;
		memorySize = ParticleSoA.GetSizeInBytes();
		stringBuilder.AppendLine($"ParticleSoA: {arg3} capacity, {arg4} count, {MemoryStats.MemoryToString(in memorySize)} size");
		object arg5 = ConstraintSoA.Capacity;
		object arg6 = ConstraintSoA.Count;
		memorySize = ConstraintSoA.GetSizeInBytes();
		stringBuilder.AppendLine($"ConstraintSoA: {arg5} capacity, {arg6} count, {MemoryStats.MemoryToString(in memorySize)} size");
		object arg7 = ConstraintBatchSoA.Capacity;
		object arg8 = ConstraintBatchSoA.Count;
		memorySize = ConstraintBatchSoA.GetSizeInBytes();
		stringBuilder.AppendLine($"ConstraintBatchSoA: {arg7} capacity, {arg8} count, {MemoryStats.MemoryToString(in memorySize)} size");
		object arg9 = ConstraintSettingsSoA.Capacity;
		object arg10 = ConstraintSettingsSoA.Count;
		memorySize = ConstraintSettingsSoA.GetSizeInBytes();
		stringBuilder.AppendLine($"ConstraintSettingsSoA: {arg9} capacity, {arg10} count, {MemoryStats.MemoryToString(in memorySize)} size");
		object arg11 = VertexToParticleMapSoA.Capacity;
		object arg12 = VertexToParticleMapSoA.Count;
		memorySize = VertexToParticleMapSoA.GetSizeInBytes();
		stringBuilder.AppendLine($"VertexToParticleMapSoA: {arg11} capacity, {arg12} count, {MemoryStats.MemoryToString(in memorySize)} size");
		object arg13 = TrianglesSoA.Capacity;
		object arg14 = TrianglesSoA.Count;
		memorySize = TrianglesSoA.GetSizeInBytes();
		stringBuilder.AppendLine($"TrianglesSoA: {arg13} capacity, {arg14} count, {MemoryStats.MemoryToString(in memorySize)} size");
		object arg15 = VertexTrianglesMapSoA.Capacity;
		object arg16 = VertexTrianglesMapSoA.Count;
		memorySize = VertexTrianglesMapSoA.GetSizeInBytes();
		stringBuilder.AppendLine($"VertexTrianglesMapSoA: {arg15} capacity, {arg16} count, {MemoryStats.MemoryToString(in memorySize)} size");
		object arg17 = VertexTrianglesMapRangesSoA.Capacity;
		object arg18 = VertexTrianglesMapRangesSoA.Count;
		memorySize = VertexTrianglesMapRangesSoA.GetSizeInBytes();
		stringBuilder.AppendLine($"VertexTrianglesMapRangesSoA: {arg17} capacity, {arg18} count, {MemoryStats.MemoryToString(in memorySize)} size");
		object arg19 = VerticesSoA.Capacity;
		object arg20 = VerticesSoA.Count;
		memorySize = VerticesSoA.GetSizeInBytes();
		stringBuilder.AppendLine($"VerticesSoA: {arg19} capacity, {arg20} count, {MemoryStats.MemoryToString(in memorySize)} size");
		object arg21 = ParticleToVertexMapSoA.Capacity;
		object arg22 = ParticleToVertexMapSoA.Count;
		memorySize = ParticleToVertexMapSoA.GetSizeInBytes();
		stringBuilder.AppendLine($"ParticleToVertexMapSoA: {arg21} capacity, {arg22} count, {MemoryStats.MemoryToString(in memorySize)} size");
		object arg23 = BonesSoA.Capacity;
		object arg24 = BonesSoA.Count;
		memorySize = BonesSoA.GetSizeInBytes();
		stringBuilder.AppendLine($"BonesSoA: {arg23} capacity, {arg24} count, {MemoryStats.MemoryToString(in memorySize)} size");
		object arg25 = BoneIndicesMapSoA.Capacity;
		object arg26 = BoneIndicesMapSoA.Count;
		memorySize = BoneIndicesMapSoA.GetSizeInBytes();
		stringBuilder.AppendLine($"BoneIndicesMapSoA: {arg25} capacity, {arg26} count, {MemoryStats.MemoryToString(in memorySize)} size");
		object arg27 = BoneIndicesMapRangesSoA.Capacity;
		object arg28 = BoneIndicesMapRangesSoA.Count;
		memorySize = BoneIndicesMapRangesSoA.GetSizeInBytes();
		stringBuilder.AppendLine($"BoneIndicesMapRangesSoA: {arg27} capacity, {arg28} count, {MemoryStats.MemoryToString(in memorySize)} size");
		object arg29 = m_IndicesMap.Length;
		memorySize = m_IndicesMap.Length * UnsafeUtility.SizeOf<int>();
		stringBuilder.AppendLine($"IndicesMap: {arg29} count, {MemoryStats.MemoryToString(in memorySize)} size");
		object arg30 = m_Transforms.capacity;
		memorySize = m_Transforms.capacity * UnsafeUtility.SizeOf<Matrix4x4>();
		stringBuilder.AppendLine($"TransformMap: {arg30} count, {MemoryStats.MemoryToString(in memorySize)} size");
		return stringBuilder.ToString();
	}
}
