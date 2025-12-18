using System.Runtime.CompilerServices;
using Owlcat.Runtime.Core.Collections;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Integrations.GPUDriven;
using Owlcat.Runtime.Visual.GPUDrivenBRG.LOD;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.Batching;

public static class GPUDrivenNativeDataUpdate
{
	public struct Instance
	{
		public GPUDrivenIndexAllocator.IndexAllocation IndexAllocation;

		public int NativeDataIndex;

		public int RendererGroupIndex;

		public unsafe GPUDrivenMetadataAuthoring.DefaultPerInstanceData* DefaultPerInstanceDataPtr;

		public unsafe GPUDrivenMetadataAuthoring.LightMapsPerInstanceData* LightMapsPerInstanceData;

		public unsafe GPUDrivenMetadataAuthoring.LightProbesPerInstanceData* LightProbesPerInstanceData;

		public GPUDrivenInstanceID RendererInstanceID;
	}

	[BurstCompile]
	public struct MarkInstancesDirtyJob : IJob
	{
		[ReadOnly]
		public NativeArray<Instance> Instances;

		[ReadOnly]
		public NativeArray<GPUDrivenAllocator.DataAllocation>.ReadOnly InstanceDataAllocations;

		[NativeDisableContainerSafetyRestriction]
		public NativeSparseSegmentList DirtyVisibilityInfoSegmentList;

		[NativeDisableContainerSafetyRestriction]
		[NativeDisableUnsafePtrRestriction]
		public GPUDrivenPropertyDataWriter PropertyDataWriter;

		public void Execute()
		{
			foreach (Instance instance in Instances)
			{
				DirtyVisibilityInfoSegmentList.AddItem(instance.IndexAllocation.Index, 1);
				ref readonly GPUDrivenAllocator.DataAllocation dataAllocation = ref UnsafeCollectionExtensions.ElementAsRef(in InstanceDataAllocations, instance.IndexAllocation.Index);
				PropertyDataWriter.MarkWholeAllocationDirty(in dataAllocation);
			}
		}
	}

	[BurstCompile]
	public struct UpdateMovingRendererIDsJob : IJob
	{
		[ReadOnly]
		public NativeArray<int> CurrentMovingRendererIDs;

		public NativeHashSet<int> MovingRendererIDs;

		public NativeHashSet<int> LastFrameMovingRendererIDs;

		public void Execute()
		{
			foreach (int currentMovingRendererID in CurrentMovingRendererIDs)
			{
				MovingRendererIDs.Add(currentMovingRendererID);
				LastFrameMovingRendererIDs.Remove(currentMovingRendererID);
			}
		}
	}

	[BurstCompile]
	public struct Job : IJobParallelFor
	{
		[ReadOnly]
		public LightProbesQuery LightProbesQuery;

		[ReadOnly]
		public NativeArray<Instance> UpdatedInstances;

		[ReadOnly]
		[NativeDisableParallelForRestriction]
		public NativeArray<GPUDrivenRendererGroupPool.RendererGroup>.ReadOnly RendererGroups;

		[ReadOnly]
		[NativeDisableParallelForRestriction]
		public NativeArray<GPUDrivenResourceRegistry.UnmanagedMeshInfo>.ReadOnly MeshInfos;

		[ReadOnly]
		[NativeDisableParallelForRestriction]
		public NativeArray<GPUDrivenResourceRegistry.UnmanagedMaterialInfo>.ReadOnly MaterialInfos;

		[ReadOnly]
		[NativeDisableParallelForRestriction]
		public NativeArray<GPUDrivenBatchRendererGroup.InstanceMetadata>.ReadOnly InstanceMetadata;

		public NativeArray<Owlcat.Runtime.Visual.GPUDrivenBRG.Scenes.GPUDrivenSceneData>.ReadOnly SceneData;

		[ReadOnly]
		[NativeDisableParallelForRestriction]
		public NativeArray<float4x4> LocalToWorldMatrices;

		[ReadOnly]
		[NativeDisableParallelForRestriction]
		public NativeArray<float4x4> PrevLocalToWorldMatrices;

		[ReadOnly]
		[NativeDisableParallelForRestriction]
		[NativeDisableContainerSafetyRestriction]
		public NativeArray<Bounds> LocalBounds;

		[ReadOnly]
		[NativeDisableParallelForRestriction]
		[NativeDisableContainerSafetyRestriction]
		public NativeArray<float4> LightmapSTs;

		[ReadOnly]
		[NativeDisableParallelForRestriction]
		[NativeDisableContainerSafetyRestriction]
		public NativeArray<int> LightmapIndices;

		[ReadOnly]
		[NativeDisableParallelForRestriction]
		[NativeDisableContainerSafetyRestriction]
		public NativeArray<int> LODGroupIDs;

		[ReadOnly]
		[NativeDisableParallelForRestriction]
		[NativeDisableContainerSafetyRestriction]
		public NativeArray<GPUDrivenProcessor.WrappedPackedRendererData> PackedRendererData;

		[NativeDisableParallelForRestriction]
		public NativeArray<GPUDrivenVisibilityInfo> VisibilityInfo;

		[NativeDisableParallelForRestriction]
		public NativeArray<int> TetrahedronCacheIndices;

		[ReadOnly]
		public NativeHashMap<int, GPUDrivenLODGroupRepository.LODGroupMetadata>.ReadOnly LODGroupMetadata;

		public NativeList<int>.ParallelWriter MovingRendererIDs;

		public unsafe void Execute(int index)
		{
			if (index < UpdatedInstances.Length)
			{
				Instance instance = UpdatedInstances[index];
				int nativeDataIndex = instance.NativeDataIndex;
				float4x4 float4x = LocalToWorldMatrices[nativeDataIndex];
				GPUDrivenRendererGroupPool.RendererGroup rendererGroup = RendererGroups[instance.RendererGroupIndex];
				GPUDrivenResourceRegistry.UnmanagedMeshInfo unmanagedMeshInfo = MeshInfos[rendererGroup.Key.MeshAllocation.Index];
				GPUDrivenResourceRegistry.UnmanagedMaterialInfo unmanagedMaterialInfo = MaterialInfos[rendererGroup.Key.MaterialAllocation.Index];
				ref GPUDrivenVisibilityInfo reference = ref UnsafeCollectionExtensions.ElementAsRefUnchecked(in VisibilityInfo, instance.IndexAllocation.Index);
				reference.DynamicFlags = GPUDrivenDynamicFlags.None;
				reference.GameObjectLayerMask = (uint)(1 << (int)rendererGroup.Key.RendererSettings.Layer);
				Bounds bounds = (LocalBounds.IsCreated ? LocalBounds[nativeDataIndex] : unmanagedMeshInfo.Bounds);
				reference.BoundingSphere = GPUDrivenMathUtils.LocalBoundsToBoundingSphere(bounds, float4x, out var aabbExtents);
				reference.AABBExtents = math.float4(aabbExtents, 1f);
				if (LODGroupIDs.IsCreated && PackedRendererData.IsCreated && LODGroupMetadata.TryGetValue(LODGroupIDs[nativeDataIndex], out var item))
				{
					byte lODMask = PackedRendererData[nativeDataIndex].LODMask;
					int index2 = item.IndexAllocation.Index;
					reference.PackedLODInstanceInfo = GPUDrivenInstanceLODInfo.Pack(index2, lODMask);
				}
				else
				{
					reference.PackedLODInstanceInfo = uint.MaxValue;
				}
				GPUDrivenMetadataAuthoring.DefaultPerInstanceData* defaultPerInstanceDataPtr = instance.DefaultPerInstanceDataPtr;
				defaultPerInstanceDataPtr->unity_ObjectToWorld = GPUDrivenMathUtils.PackTransformMatrix(float4x);
				float4x4 transformMatrix = GPUDrivenMathUtils.AffineInverse3D(float4x);
				defaultPerInstanceDataPtr->unity_WorldToObject = GPUDrivenMathUtils.PackTransformMatrix(transformMatrix);
				float4x4 float4x2 = PrevLocalToWorldMatrices[nativeDataIndex];
				defaultPerInstanceDataPtr->unity_MatrixPreviousM = GPUDrivenMathUtils.PackTransformMatrix(float4x2);
				float4x4 transformMatrix2 = GPUDrivenMathUtils.AffineInverse3D(float4x2);
				defaultPerInstanceDataPtr->unity_MatrixPreviousMI = GPUDrivenMathUtils.PackTransformMatrix(transformMatrix2);
				bool num = !float4x.Equals(float4x2);
				if (num || unmanagedMaterialInfo.GPUDrivenMaterialUniquenessKey.HasMotionVectorsPassEnabled() || rendererGroup.Key.RendererSettings.MotionVectorGenerationMode == MotionVectorGenerationMode.ForceNoMotion)
				{
					reference.DynamicFlags |= GPUDrivenDynamicFlags.DrawMotionVectors;
				}
				if (num && instance.RendererInstanceID.Type == GPUDrivenInstanceID.InstanceIDType.UnityObject)
				{
					MovingRendererIDs.AddNoResize(instance.RendererInstanceID.RawInstanceID);
				}
				if (instance.LightMapsPerInstanceData != null && LightmapSTs.IsCreated && LightmapIndices.IsCreated)
				{
					float4 lightmapST = LightmapSTs[nativeDataIndex];
					int num2 = LightmapIndices[nativeDataIndex];
					GPUDrivenBatchRendererGroup.InstanceMetadata instanceMetadata = InstanceMetadata[instance.IndexAllocation.Index];
					FillLightMapsData(lightmapIndex: (instanceMetadata.SceneIndex.Index != -1) ? (num2 - SceneData[instanceMetadata.SceneIndex.Index].LightmapIndexOffset) : 0, perInstanceData: ref *instance.LightMapsPerInstanceData, lightmapST: lightmapST);
				}
				if (instance.LightProbesPerInstanceData != null)
				{
					FillLightProbesData(ref *instance.LightProbesPerInstanceData, in reference, instance.IndexAllocation.Index);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void FillLightMapsData(ref GPUDrivenMetadataAuthoring.LightMapsPerInstanceData perInstanceData, float4 lightmapST, int lightmapIndex)
		{
			perInstanceData.unity_LightmapST = lightmapST;
			perInstanceData.unity_LightmapIndex = math.float4(lightmapIndex, 0f, 0f, 0f);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void FillLightProbesData(ref GPUDrivenMetadataAuthoring.LightProbesPerInstanceData perInstanceData, in GPUDrivenVisibilityInfo visibilityInfo, int instanceIndex)
		{
			float3 xyz = visibilityInfo.BoundingSphere.xyz;
			ref int tetrahedronIndex = ref UnsafeCollectionExtensions.ElementAsRefUnchecked(in TetrahedronCacheIndices, instanceIndex);
			LightProbesQuery.CalculateInterpolatedLightAndOcclusionProbe(xyz, ref tetrahedronIndex, out var lightProbe, out var occlusionProbe);
			perInstanceData.unity_SHCoefficients = new SHCoefficients(lightProbe, occlusionProbe);
		}
	}
}
