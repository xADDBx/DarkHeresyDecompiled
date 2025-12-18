using System;
using Owlcat.Runtime.Core.Collections.Extensions;
using Owlcat.Runtime.Visual;
using Owlcat.Runtime.Visual.Effects.LineRenderer;
using Owlcat.Runtime.Visual.Effects.RayView;
using Owlcat.Runtime.Visual.GPUDrivenBRG;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Batching;
using Owlcat.Runtime.Visual.GPUDrivenBRG.LOD;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Utilities;
using Owlcat.Runtime.Visual.Lighting;
using Owlcat.Runtime.Visual.OcclusionClipping;
using Owlcat.Runtime.Visual.OcclusionGeometryClip;
using Owlcat.Runtime.Visual.VirtualTerrain.Streaming;
using Owlcat.Runtime.Visual.VirtualTexture;
using Owlcat.Runtime.Visual.VirtualTexture.Atlas.Jobs;
using Owlcat.Runtime.Visual.VirtualTexture.Feedback;
using Owlcat.Runtime.Visual.VirtualTexture.IndirectionTexture;
using Owlcat.Runtime.Visual.VirtualTexture.IndirectionTexture.Jobs;
using Owlcat.Runtime.Visual.VirtualTexture.PostRender.Jobs;
using Owlcat.Runtime.Visual.Waaagh.Lighting;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.Highlighting;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.TerrainBlending;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.TerrainStamping.Passes;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting.Jobs;
using Owlcat.Runtime.Visual.Waaagh.Shadows;
using Owlcat.Runtime.Visual.XPBD.Collisions.Broadphase.CPU;
using Owlcat.Runtime.Visual.XPBD.Collisions.Broadphase.CPU.Jobs;
using Owlcat.Runtime.Visual.XPBD.Collisions.Jobs;
using Owlcat.Runtime.Visual.XPBD.Culling.Jobs;
using Owlcat.Runtime.Visual.XPBD.Debug.Jobs;
using Owlcat.Runtime.Visual.XPBD.ParticleAttachments.Jobs;
using Owlcat.Runtime.Visual.XPBD.Solvers.CPU.Jobs;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

[Unity.Jobs.DOTSCompilerGenerated]
internal class __JobReflectionRegistrationOutput__4975605086208355381
{
	public static void CreateJobReflectionData()
	{
		try
		{
			IJobParallelForTransformExtensions.EarlyJobInit<UpdateMeshDeformerTransformsJob>();
			IJobParallelForExtensions.EarlyJobInit<RestorePaticlesJob>();
			IJobParallelForExtensions.EarlyJobInit<UpdateAttachmentsJob>();
			IJobParallelForTransformExtensions.EarlyJobInit<UpdateTransformsJob>();
			IJobParallelForExtensions.EarlyJobInit<GizmosTransformDeformedVerticesToWorldJob>();
			IJobParallelForExtensions.EarlyJobInit<GizmosTransformVerticesToWorldJob>();
			IJobParallelForExtensions.EarlyJobInit<ClearCullingDataJob>();
			IJobParallelForExtensions.EarlyJobInit<CullBodiesJob>();
			IJobParallelForExtensions.EarlyJobInit<GenerateParticleContactsGlobalJob>();
			IJobParallelForExtensions.EarlyJobInit<ApplyExternalForcesJob>();
			IJobParallelForTransformExtensions.EarlyJobInit<CopyBonesJob>();
			IJobParallelForExtensions.EarlyJobInit<CopySkinnedVerticesJob>();
			IJobParallelForExtensions.EarlyJobInit<DeformMeshJob>();
			IJobParallelForExtensions.EarlyJobInit<RecalculateSkinNormalsJob>();
			IJobParallelForDeferExtensions.EarlyJobInit<SolveContactsJob>();
			IJobParallelForExtensions.EarlyJobInit<ApplyContactsDeltasJob>();
			IJobParallelForExtensions.EarlyJobInit<SolveJob>();
			IJobParallelForExtensions.EarlyJobInit<TransformBodyJob>();
			IJobParallelForExtensions.EarlyJobInit<UpdateBodyAabbJob>();
			IJobParallelForTransformExtensions.EarlyJobInit<UpdateBodyTransformsJob>();
			IJobParallelForExtensions.EarlyJobInit<UpdateBonesJob>();
			IJobParallelForExtensions.EarlyJobInit<UpdateMeshBasePositionsJob>();
			IJobParallelForExtensions.EarlyJobInit<UpdateVelocitiesJob>();
			IJobParallelForExtensions.EarlyJobInit<UpdateVerticesJob>();
			IJobParallelForTransformExtensions.EarlyJobInit<UpdateCollidersJob>();
			IJobParallelForExtensions.EarlyJobInit<CalculateParticleGridSpacingJob>();
			IJobParallelForExtensions.EarlyJobInit<BuildGridJob>();
			IJobParallelForExtensions.EarlyJobInit<BuildParticlesGridJob>();
			IJobParallelForExtensions.EarlyJobInit<BuildSimplexAabbJob>();
			IJobParallelForExtensions.EarlyJobInit<CalculateGridSpacingJob>();
			IJobParallelForExtensions.EarlyJobInit<CalculateHashmapLoadFactorJob>();
			IJobParallelForExtensions.EarlyJobInit<ClearGridJob>();
			IJobParallelForExtensions.EarlyJobInit<GenerateColliderContactsGlobalJob>();
			IJobExtensions.EarlyJobInit<Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting.Jobs.CullingJob>();
			IJobParallelForExtensions.EarlyJobInit<ExtractLocalFogDataJob>();
			IJobParallelForExtensions.EarlyJobInit<Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting.Jobs.MinMaxZJob>();
			IJobForExtensions.EarlyJobInit<Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting.Jobs.ZBinningJob>();
			IJobParallelForExtensions.EarlyJobInit<TerrainStampingBrushPass.BrushCullingJob>();
			IJobExtensions.EarlyJobInit<TerrainStampingBrushPass.MergeBrushCullingResultsJob>();
			IJobExtensions.EarlyJobInit<TerrainCullingJob>();
			IJobExtensions.EarlyJobInit<CountJob>();
			IJobParallelForExtensions.EarlyJobInit<Owlcat.Runtime.Visual.Waaagh.RendererFeatures.Highlighting.CullingJob>();
			IJobExtensions.EarlyJobInit<ShadowJob>();
			IJobForExtensions.EarlyJobInit<ExtractLightDataJob>();
			IJobForExtensions.EarlyJobInit<Owlcat.Runtime.Visual.Waaagh.Lighting.MinMaxZJob>();
			IJobExtensions.EarlyJobInit<RadixSortJob>();
			IJobForExtensions.EarlyJobInit<Owlcat.Runtime.Visual.Waaagh.Lighting.ZBinningJob>();
			IJobParallelForExtensions.EarlyJobInit<CollectFeedbackTilesJob>();
			IJobParallelForExtensions.EarlyJobInit<CollectResidentTilesJob>();
			IJobExtensions.EarlyJobInit<RequestedTilesAnalysisJob>();
			IJobExtensions.EarlyJobInit<PageDrawDataBuildJob>();
			IJobExtensions.EarlyJobInit<PageDrawDataGPUBuildJob>();
			IJobExtensions.EarlyJobInit<EntryAllocationJob>();
			IJobExtensions.EarlyJobInit<FindMaxMipCountJob>();
			IJobExtensions.EarlyJobInit<InvalidatePagesJob>();
			IJobExtensions.EarlyJobInit<RemoveUnusedTextureStacksJob>();
			IJobExtensions.EarlyJobInit<UpdateAtlasJob>();
			IJobParallelForBatchExtensions.EarlyJobInit<TerrainLayerPVSFactory.BuildMasksJob>();
			IJobParallelForBatchExtensions.EarlyJobInit<TerrainLayerPVSFactory.BuildPvsNodesJob>();
			IJobExtensions.EarlyJobInit<Service.BuildCastGeometryJob>();
			IJobParallelForExtensions.EarlyJobInit<Service.AnimationJob>();
			IJobExtensions.EarlyJobInit<ReplayActionsJob>();
			IJobExtensions.EarlyJobInit<UpdateRendererLinksJob>();
			IJobExtensions.EarlyJobInit<UpdateRendererOpacityJob>();
			IJobExtensions.EarlyJobInit<UpdateTriggersJob>();
			IJobExtensions.EarlyJobInit<UpdateVolumeLinksJob>();
			IJobExtensions.EarlyJobInit<LightCookieManager.Job>();
			IJobParallelForExtensions.EarlyJobInit<GPUDrivenBatchDataUploaderExtensions.ApplyBufferDataSegmentUnitStride>();
			IJobParallelForBatchExtensions.EarlyJobInit<GPUDrivenBatchDataUploaderExtensions.ComputeTotalSegmentByteSizeJob>();
			IJobParallelForBatchExtensions.EarlyJobInit<GPUDrivenBatchDataUploaderExtensions.ScheduleUploadSegmentsJob>();
			IJobParallelForBatchExtensions.EarlyJobInit<GPUDrivenBatchRendererGroup.UpdateTransformJobs.CollectRendererStatesJob>();
			IJobParallelForExtensions.EarlyJobInit<GPUDrivenBatchRendererGroup.UpdateTransformJobs.CheckChangedGroups>();
			IJobParallelForBatchExtensions.EarlyJobInit<GPUDrivenBatchRendererGroup.CollectInstancesForNativeDataUpdateJob>();
			IJobExtensions.EarlyJobInit<GPUDrivenBRGInstanceCuller.PrepareCullingDataJob>();
			IJobExtensions.EarlyJobInit<GPUDrivenBRGInstanceCuller.SkipSubmittingDrawCommandsJob>();
			IJobParallelForBatchExtensions.EarlyJobInit<GPUDrivenBRGInstanceCuller.CoarseInstanceCullingJob>();
			IJobParallelForBatchExtensions.EarlyJobInit<GPUDrivenBRGInstanceCuller.DrawCommandMaskingJob>();
			IJobExtensions.EarlyJobInit<GPUDrivenBRGInstanceCuller.FixupBatchDrawRangesJob>();
			IJobParallelForExtensions.EarlyJobInit<GPUDrivenBRGInstanceCuller.MarkSingleSplitDrawRangesForMergeJob>();
			IJobExtensions.EarlyJobInit<GPUDrivenBRGInstanceCuller.CreateSingleSplitDrawRangesJob>();
			IJobParallelForExtensions.EarlyJobInit<GPUDrivenBRGInstanceCuller.FillSingleSplitBatchDrawRangesJob>();
			IJobExtensions.EarlyJobInit<GPUDrivenBRGInstanceCuller.FreeUnsafePtrJob>();
			IJobParallelForExtensions.EarlyJobInit<GPUDrivenBRGInstanceCuller.BuildSingleSplitDrawCommandsJob>();
			IJobForExtensions.EarlyJobInit<GPUDrivenRendererGroupPool.SortGroupInstanceIndicesJob>();
			IJobForExtensions.EarlyJobInit<GPUDrivenRendererGroupPool.FillGroupInfoJob>();
			IJobExtensions.EarlyJobInit<GPUDrivenRendererGroupPool.CreateGPUCullingJobsJob>();
			IJobParallelForBatchExtensions.EarlyJobInit<GPUDrivenRendererGroupPool.RendererGroupMergeabilityCheckJob>();
			IJobParallelForBatchExtensions.EarlyJobInit<GPUDrivenRendererGroupPool.PopulateRendererGroupSlicesJob>();
			IJobExtensions.EarlyJobInit<GPUDrivenRendererGroupPool.GroupSliceCustomSortingJob>();
			IJobParallelForExtensions.EarlyJobInit<GPUDrivenSortableGroupDescriptionManager.StripRedundantDataFromSortableGroupsJob>();
			IJobParallelForBatchExtensions.EarlyJobInit<GPUDrivenSortableGroupDescriptionManager.CollectSortableGroupDescriptionsJob>();
			IJobParallelForExtensions.EarlyJobInit<InstanceQueries.CollectRendererInstanceIDsJob>();
			IJobParallelForExtensions.EarlyJobInit<InstanceQueries.CollectGameObjectInstanceIDsJob>();
			IJobExtensions.EarlyJobInit<NativeSparseSegmentList.MergeJob>();
			IJobParallelForBatchExtensions.EarlyJobInit<GPUDrivenLODGroupRepository.CollectExistingIndexAllocationsJob>();
			IJobParallelForExtensions.EarlyJobInit<GPUDrivenLODGroupRepository.UpdateLODGroupDataJob>();
			IJobParallelForExtensions.EarlyJobInit<GPUDrivenLODGroupRepository.UpdateAnimatedCrossFadeValuesJob>();
			IJobFilterExtensions.EarlyJobInit<GPUDrivenLODViewCollection.FilterAnimatedGroupsJob>();
			IJobParallelForExtensions.EarlyJobInit<GPUDrivenLODViewCollection.TickJob>();
			IJobExtensions.EarlyJobInit<GPUDrivenNativeDataUpdate.MarkInstancesDirtyJob>();
			IJobExtensions.EarlyJobInit<GPUDrivenNativeDataUpdate.UpdateMovingRendererIDsJob>();
			IJobParallelForExtensions.EarlyJobInit<GPUDrivenNativeDataUpdate.Job>();
			IJobParallelForExtensions.EarlyJobInit<RayViewUpdateJob>();
			IJobParallelForExtensions.EarlyJobInit<CompositeLineUpdateJob>();
			IJobParallelForExtensions.EarlyJobInit<UpdateGeometryJob>();
			IJobExtensions.EarlyJobInit<SortListJob<PageLoadInfo>>();
			IJobExtensions.EarlyJobInit<SortListJob<PageDrawData>>();
			IJobExtensions.EarlyJobInit<UnsafeHashMapExtensions.GetHashMapValueArray<GPUDrivenRendererGroupPool.RendererGroupKey, GPUDrivenIndexAllocator.IndexAllocation>.Job>();
		}
		catch (Exception ex)
		{
			EarlyInitHelpers.JobReflectionDataCreationFailed(ex);
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	public static void EarlyInit()
	{
		CreateJobReflectionData();
	}
}
