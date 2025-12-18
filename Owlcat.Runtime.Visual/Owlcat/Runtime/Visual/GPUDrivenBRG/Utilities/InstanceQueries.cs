using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.Utilities;

internal static class InstanceQueries
{
	[BurstCompile]
	private struct CollectRendererInstanceIDsJob : IJobParallelFor
	{
		public const int kBatchSize = 32;

		[ReadOnly]
		[NativeDisableParallelForRestriction]
		public NativeArray<GPUDrivenBatchRendererGroup.InstanceMetadata>.ReadOnly InstanceMetadata;

		[ReadOnly]
		public NativeArray<int>.ReadOnly InstanceIndices;

		[WriteOnly]
		public NativeArray<int> RendererInstanceIDs;

		public void Execute(int index)
		{
			int index2 = InstanceIndices[index];
			GPUDrivenInstanceID rendererInstanceID = InstanceMetadata[index2].RendererInstanceID;
			int value = ((rendererInstanceID.Type == GPUDrivenInstanceID.InstanceIDType.UnityObject) ? rendererInstanceID.RawInstanceID : 0);
			RendererInstanceIDs[index] = value;
		}
	}

	[BurstCompile]
	private struct CollectGameObjectInstanceIDsJob : IJobParallelFor
	{
		public const int kBatchSize = 32;

		[ReadOnly]
		[NativeDisableParallelForRestriction]
		public NativeArray<GPUDrivenBatchRendererGroup.InstanceMetadata>.ReadOnly InstanceMetadata;

		[ReadOnly]
		public NativeArray<int>.ReadOnly InstanceIndices;

		[WriteOnly]
		public NativeArray<int> GameObjectInstanceIDs;

		public void Execute(int index)
		{
			int index2 = InstanceIndices[index];
			GameObjectInstanceIDs[index] = InstanceMetadata[index2].GameObjectInstanceID;
		}
	}

	public static NativeArray<int> CollectRendererInstanceIDs(NativeArray<int>.ReadOnly instanceIndices, GPUDrivenBatchRendererGroup brg, Allocator allocator)
	{
		NativeArray<int> nativeArray = new NativeArray<int>(instanceIndices.Length, allocator);
		CollectRendererInstanceIDsJob jobData = default(CollectRendererInstanceIDsJob);
		jobData.InstanceMetadata = brg.GetAllInstanceMetadataReadonly();
		jobData.RendererInstanceIDs = nativeArray;
		jobData.InstanceIndices = instanceIndices;
		IJobParallelForExtensions.Schedule(jobData, instanceIndices.Length, 32).Complete();
		return nativeArray;
	}

	public static NativeArray<int> CollectGameObjectInstanceIDs(NativeArray<int>.ReadOnly instanceIndices, GPUDrivenBatchRendererGroup brg, Allocator allocator)
	{
		NativeArray<int> nativeArray = new NativeArray<int>(instanceIndices.Length, allocator);
		CollectGameObjectInstanceIDsJob jobData = default(CollectGameObjectInstanceIDsJob);
		jobData.InstanceMetadata = brg.GetAllInstanceMetadataReadonly();
		jobData.GameObjectInstanceIDs = nativeArray;
		jobData.InstanceIndices = instanceIndices;
		IJobParallelForExtensions.Schedule(jobData, instanceIndices.Length, 32).Complete();
		return nativeArray;
	}
}
