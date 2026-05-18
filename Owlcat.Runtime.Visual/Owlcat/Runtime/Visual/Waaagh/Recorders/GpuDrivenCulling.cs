using System;
using System.Collections.Generic;
using Owlcat.Runtime.Core.Collections;
using Owlcat.Runtime.Visual.GPUDrivenBRG;
using Owlcat.Runtime.Visual.GPUDrivenBRG.LOD;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders;

internal static class GpuDrivenCulling
{
	private sealed class PassData
	{
		public class UsedBuffers
		{
			public readonly List<BufferHandle> CPUInstanceVisibilityMasks = new List<BufferHandle>();

			public readonly List<BufferHandle> IndirectArgs = new List<BufferHandle>();

			public readonly List<BufferHandle> VisibleIndices = new List<BufferHandle>();

			public BufferHandle CullingJobs;

			public BufferHandle EmptyViewDependentLODData;

			public BufferHandle GroupInfo;

			public BufferHandle LODGroupData;

			public BufferHandle PersistentIndices;

			public BufferHandle VisibilityInfo;

			public void Clear()
			{
				IndirectArgs.Clear();
				VisibleIndices.Clear();
				CPUInstanceVisibilityMasks.Clear();
			}
		}

		public OcclusionCullingPassType PassType;

		public CullingViewFilter CullingViewFilter;

		public readonly UsedBuffers Buffers = new UsedBuffers();

		public GPUDrivenBatchRendererGroup BRG;

		public CameraType CameraType;

		public TextureHandle CameraDepthTexture;
	}

	public enum OcclusionCullingPassType
	{
		None,
		First,
		FalseNegative
	}

	public delegate bool CullingViewFilter(BatchCullingViewType batchCullingViewType, GPUDrivenRendererGroupPool.ViewType groupViewType);

	private struct CullingDispatchInfo
	{
		public int CullingResourcesIndex;

		public int? InstanceVisibilityMaskIndex;

		public GPUDrivenRendererGroupPool.ViewType ViewType;

		public BatchPackedCullingViewID ViewID;

		public NativeList<int> CullingContextIndices;

		public bool CanBeBatchedWith(in CullingDispatchInfo source)
		{
			if (CullingContextIndices.Length >= 16)
			{
				return false;
			}
			if (CullingResourcesIndex == source.CullingResourcesIndex && InstanceVisibilityMaskIndex == source.InstanceVisibilityMaskIndex && ViewType == source.ViewType)
			{
				return ViewID == source.ViewID;
			}
			return false;
		}
	}

	private struct FixupIndirectArgsDispatchInfo
	{
		public int CullingResourcesIndex;

		public NativeList<int> CullingContextIndices;

		public bool CanBeBatchedWith(in FixupIndirectArgsDispatchInfo source)
		{
			if (CullingContextIndices.Length >= 16)
			{
				return false;
			}
			return CullingResourcesIndex == source.CullingResourcesIndex;
		}
	}

	private static class Profiling
	{
		public static readonly ProfilingSampler UploadLODBuffers = new ProfilingSampler("Upload LOD Buffers");

		public static readonly ProfilingSampler ClearBatchCountersSampler = new ProfilingSampler("Clear Batch Counters");

		public static readonly ProfilingSampler BindSharedBuffers = new ProfilingSampler("Bind Shared Buffers");

		public static readonly ProfilingSampler Culling = new ProfilingSampler("Culling");

		public static readonly ProfilingSampler FixupIndirectArgsSampler = new ProfilingSampler("Fixup Indirect Args");
	}

	private static class ShaderIDs
	{
		public static class Culling
		{
			public const int KernelIndex = 0;

			public static readonly int _PersistentIndices = Shader.PropertyToID("_PersistentIndices");

			public static readonly int _VisibilityInfo = Shader.PropertyToID("_VisibilityInfo");

			public static readonly int _LODGroupData = Shader.PropertyToID("_LODGroupData");

			public static readonly int _ViewDependentLODGroupData = Shader.PropertyToID("_ViewDependentLODGroupData");

			public static readonly int _BRGFrustumPlanes = Shader.PropertyToID("_BRGFrustumPlanes");

			public static readonly int _VisibleIndices = Shader.PropertyToID("_VisibleIndices");

			public static readonly int _InstanceVisibilityMask = Shader.PropertyToID("_InstanceVisibilityMask");

			public static readonly int _InstanceVisibilityMaskPrev = Shader.PropertyToID("_InstanceVisibilityMaskPrev");

			public static readonly int _CPUInstanceVisibilityMask = Shader.PropertyToID("_CPUInstanceVisibilityMask");

			public static readonly int _CullingJobs = Shader.PropertyToID("_CullingJobs");

			public static readonly int _CullingContextIndices = Shader.PropertyToID("_CullingContextIndices");

			public static readonly int _ApplySceneVisibility = Shader.PropertyToID("_ApplySceneVisibility");
		}

		public static class FixupIndirectArgs
		{
			public static readonly int _CullingContextIndices = Shader.PropertyToID("_CullingContextIndices");

			public static readonly int _IndirectArgs = Shader.PropertyToID("_IndirectArgs");
		}

		public static readonly int _GroupInfo = Shader.PropertyToID("_GroupInfo");

		public static readonly int _CullingContexts = Shader.PropertyToID("_CullingContexts");

		public static readonly int _GroupCounters = Shader.PropertyToID("_GroupCounters");
	}

	private static readonly Vector4[] s_SharedCullingContextIndices = new Vector4[4];

	public static void Record(RenderGraph renderGraph, WaaaghRenderingData renderingData, WaaaghCameraData cameraData, TextureHandle cameraDepthTexture, string passName, OcclusionCullingPassType passType, CullingViewFilter cullingViewFilter)
	{
		PassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = renderGraph.AddUnsafePass<PassData>(passName, out passData, ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\GpuDriven\\GpuDrivenCulling.cs", 31);
		GPUDrivenBatchRendererGroup gPUDrivenBatchRendererGroup = renderingData.GPUDrivenBatchRendererGroup;
		GPUDrivenCullingPassSharedData sharedPassData = gPUDrivenBatchRendererGroup.SharedPassData;
		passData.BRG = gPUDrivenBatchRendererGroup;
		passData.PassType = passType;
		passData.CullingViewFilter = cullingViewFilter;
		passData.CameraType = cameraData.cameraType;
		passData.Buffers.Clear();
		GPUDrivenCullingResourcesPool cullingResourcesPool = gPUDrivenBatchRendererGroup.CullingResourcesPool;
		for (int i = 0; i < cullingResourcesPool.UsedCount; i++)
		{
			GPUDrivenCullingResourcesPool.CullingResourceSet cullingResourceSet = cullingResourcesPool.Sets[i];
			passData.Buffers.VisibleIndices.Add(renderGraph.ImportBuffer(cullingResourceSet.VisibleIndices.InternalBuffer));
			passData.Buffers.IndirectArgs.Add(renderGraph.ImportBuffer(cullingResourceSet.IndirectArgs.InternalBuffer));
			passData.Buffers.CPUInstanceVisibilityMasks.Add(cullingResourceSet.CPUInstanceVisibilityMask.IsCreated ? renderGraph.ImportBuffer(cullingResourceSet.CPUInstanceVisibilityMask.InternalBuffer) : default(BufferHandle));
		}
		passData.Buffers.PersistentIndices = renderGraph.ImportBuffer(gPUDrivenBatchRendererGroup.PersistentIndicesBuffer);
		passData.Buffers.VisibilityInfo = renderGraph.ImportBuffer(gPUDrivenBatchRendererGroup.VisibilityInfoBuffer);
		passData.Buffers.LODGroupData = renderGraph.ImportBuffer(gPUDrivenBatchRendererGroup.LODGroupRepository.GPUDataBuffer);
		PassData.UsedBuffers buffers = passData.Buffers;
		BufferDesc desc = new BufferDesc(1, 4, GraphicsBuffer.Target.Raw)
		{
			name = "EmptyViewDependentLODData"
		};
		buffers.EmptyViewDependentLODData = unsafeRenderGraphBuilder.CreateTransientBuffer(in desc);
		passData.Buffers.GroupInfo = renderGraph.ImportBuffer(gPUDrivenBatchRendererGroup.GroupInfoBuffer);
		passData.Buffers.CullingJobs = renderGraph.ImportBuffer(gPUDrivenBatchRendererGroup.GPUCullingJobsBuffer);
		if (passType == OcclusionCullingPassType.FalseNegative && sharedPassData.Buffers.MainViewOcclusionTestDebug.IsValid())
		{
			unsafeRenderGraphBuilder.UseBuffer(in sharedPassData.Buffers.MainViewOcclusionTestDebug, AccessFlags.Write);
		}
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder.UseBuffer(in passData.Buffers.PersistentIndices);
		unsafeRenderGraphBuilder.UseBuffer(in passData.Buffers.VisibilityInfo);
		unsafeRenderGraphBuilder.UseBuffer(in passData.Buffers.LODGroupData);
		unsafeRenderGraphBuilder.UseBuffer(in sharedPassData.Buffers.FrustumPlanes);
		unsafeRenderGraphBuilder.UseBuffer(in sharedPassData.Buffers.CullingContexts);
		unsafeRenderGraphBuilder.UseBuffer(in passData.Buffers.GroupInfo, AccessFlags.Write);
		unsafeRenderGraphBuilder.UseBuffer(in passData.Buffers.CullingJobs, AccessFlags.Write);
		unsafeRenderGraphBuilder.UseBuffer(in sharedPassData.Buffers.GroupCounters, AccessFlags.Write);
		foreach (BufferHandle indirectArg in passData.Buffers.IndirectArgs)
		{
			BufferHandle input = indirectArg;
			unsafeRenderGraphBuilder.UseBuffer(in input, AccessFlags.Write);
		}
		foreach (BufferHandle visibleIndex in passData.Buffers.VisibleIndices)
		{
			BufferHandle input2 = visibleIndex;
			unsafeRenderGraphBuilder.UseBuffer(in input2, AccessFlags.Write);
		}
		foreach (BufferHandle cPUInstanceVisibilityMask in passData.Buffers.CPUInstanceVisibilityMasks)
		{
			BufferHandle input3 = cPUInstanceVisibilityMask;
			if (input3.IsValid())
			{
				unsafeRenderGraphBuilder.UseBuffer(in input3, AccessFlags.Write);
			}
		}
		if (passType == OcclusionCullingPassType.FalseNegative)
		{
			passData.CameraDepthTexture = cameraDepthTexture;
			unsafeRenderGraphBuilder.UseTexture(in passData.CameraDepthTexture);
			unsafeRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraDepthPyramidRT);
		}
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData data, UnsafeGraphContext context)
		{
			Render(data, context);
		});
	}

	private static void Render(PassData data, UnsafeGraphContext context)
	{
		GPUDrivenCullingPassSharedData sharedPassData = data.BRG.SharedPassData;
		if (sharedPassData.MaxRendererGroupSlicesPerView == 0 || sharedPassData.CullingContexts.Length == 0)
		{
			return;
		}
		NativeList<int> filteredCullingContexts = FilterCullingContexts(sharedPassData, data.CullingViewFilter);
		UploadLODBuffers(data, context, sharedPassData, filteredCullingContexts);
		OcclusionCullingPassType passType = data.PassType;
		if (passType == OcclusionCullingPassType.None || passType == OcclusionCullingPassType.First)
		{
			using (new ProfilingScope(context.cmd, Profiling.BindSharedBuffers))
			{
				context.cmd.SetGlobalBuffer(ShaderIDs._CullingContexts, sharedPassData.Buffers.CullingContexts);
				context.cmd.SetGlobalBuffer(ShaderIDs._GroupInfo, data.Buffers.GroupInfo);
				context.cmd.SetGlobalBuffer(ShaderIDs._GroupCounters, sharedPassData.Buffers.GroupCounters);
			}
		}
		SetCPUInstanceVisibilityMaskData(data, sharedPassData, context);
		DispatchClearBatchCounters(data, sharedPassData, in context);
		DispatchCulling(data, sharedPassData, in context, data.PassType, filteredCullingContexts);
		DispatchFixupIndirectArgs(data, sharedPassData, in context, filteredCullingContexts);
		ReadbackVisibilityMask(data, sharedPassData, in context, data.PassType, filteredCullingContexts);
	}

	private static NativeList<int> FilterCullingContexts(GPUDrivenCullingPassSharedData sharedData, CullingViewFilter cullingViewFilter)
	{
		NativeList<int> result = new NativeList<int>(sharedData.CullingContexts.Length, Allocator.Temp);
		for (int i = 0; i < sharedData.CullingContexts.Length; i++)
		{
			ref GPUDrivenCullingContext reference = ref UnsafeCollectionExtensions.ElementAsRef(in sharedData.CullingContexts, i);
			if (cullingViewFilter(reference.BatchCullingViewType, reference.ViewType))
			{
				result.Add(in i);
			}
		}
		return result;
	}

	private static void UploadLODBuffers(PassData data, UnsafeGraphContext context, GPUDrivenCullingPassSharedData sharedData, NativeList<int> filteredCullingContexts)
	{
		using (new ProfilingScope(context.cmd, Profiling.UploadLODBuffers))
		{
			List<GPUDrivenLODViewCollection.PendingBufferUpload> value;
			using (ListPool<GPUDrivenLODViewCollection.PendingBufferUpload>.Get(out value))
			{
				List<BatchPackedCullingViewID> value2;
				using (ListPool<BatchPackedCullingViewID>.Get(out value2))
				{
					foreach (int item in filteredCullingContexts)
					{
						ref GPUDrivenCullingContext reference = ref UnsafeCollectionExtensions.ElementAsRef(in sharedData.CullingContexts, item);
						if (!value2.Contains(reference.ViewID))
						{
							value2.Add(reference.ViewID);
							if (data.BRG.LODGroupRepository.TryBeginBufferUpload(reference.ViewID, out var pendingBufferUpload))
							{
								value.Add(pendingBufferUpload);
							}
						}
					}
					foreach (GPUDrivenLODViewCollection.PendingBufferUpload item2 in value)
					{
						CommandBuffer nativeCommandBuffer = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);
						item2.Complete(nativeCommandBuffer);
					}
				}
			}
		}
	}

	private static void SetCPUInstanceVisibilityMaskData(PassData data, GPUDrivenCullingPassSharedData sharedData, UnsafeGraphContext context)
	{
		for (int i = 0; i < sharedData.CullingContexts.Length; i++)
		{
			ref GPUDrivenCullingContext reference = ref UnsafeCollectionExtensions.ElementAsRef(in sharedData.CullingContexts, i);
			if (!reference.CPUInstanceVisibilityJobHandle.Equals(default(JobHandle)))
			{
				reference.CPUInstanceVisibilityJobHandle.Complete();
				reference.CPUInstanceVisibilityJobHandle = default(JobHandle);
			}
			if (reference.CPUInstanceVisibilityMask.IsCreated)
			{
				int cullingResourcesIndex = reference.CullingResourcesIndex;
				BufferHandle bufferHandle = data.Buffers.CPUInstanceVisibilityMasks[cullingResourcesIndex];
				context.cmd.SetBufferData(bufferHandle, reference.CPUInstanceVisibilityMask, 0, reference.CPUInstanceVisibilityMaskOffset, reference.CPUInstanceVisibilityMaskCount);
			}
		}
	}

	private static void DispatchClearBatchCounters(PassData data, GPUDrivenCullingPassSharedData sharedData, in UnsafeGraphContext context)
	{
		using (new ProfilingScope(context.cmd, Profiling.ClearBatchCountersSampler))
		{
			CommandBuffer nativeCommandBuffer = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);
			data.BRG.BufferUtils.DispatchClearBuffer(nativeCommandBuffer, sharedData.Buffers.GroupCounters, 0);
		}
	}

	private static void DispatchCulling(PassData data, GPUDrivenCullingPassSharedData sharedData, in UnsafeGraphContext context, OcclusionCullingPassType occlusionCullingPassType, NativeList<int> filteredCullingContexts)
	{
		using (new ProfilingScope(context.cmd, Profiling.Culling))
		{
			ComputeShader cullingCS = data.BRG.Resources.CullingCS;
			LocalKeyword keyword = new LocalKeyword(cullingCS, "FIRST_PASS");
			context.cmd.SetKeyword(cullingCS, in keyword, occlusionCullingPassType == OcclusionCullingPassType.First);
			LocalKeyword keyword2 = new LocalKeyword(cullingCS, "FALSE_NEGATIVE_PASS");
			context.cmd.SetKeyword(cullingCS, in keyword2, occlusionCullingPassType == OcclusionCullingPassType.FalseNegative);
			LocalKeyword keyword3 = new LocalKeyword(cullingCS, "OCCLUSION_CULLING_AABB");
			context.cmd.SetKeyword(cullingCS, in keyword3, data.BRG.Settings.OccludeeUseAABB);
			LocalKeyword keyword4 = new LocalKeyword(cullingCS, "OUTPUT_CULLING_STATS");
			context.cmd.SetKeyword(cullingCS, in keyword4, occlusionCullingPassType != OcclusionCullingPassType.First && sharedData.Buffers.CullingStats.IsValid());
			LocalKeyword keyword5 = new LocalKeyword(cullingCS, "INSTANCE_SORTING");
			context.cmd.SetKeyword(cullingCS, in keyword5, data.BRG.Settings.OpaqueSortingGPU);
			if (occlusionCullingPassType == OcclusionCullingPassType.FalseNegative)
			{
				context.cmd.SetComputeTextureParam(cullingCS, 0, GlobalTextureShaderPropertyId._CameraDepthTexture, data.CameraDepthTexture);
			}
			context.cmd.SetComputeBufferParam(cullingCS, 0, ShaderIDs.Culling._PersistentIndices, data.Buffers.PersistentIndices);
			context.cmd.SetComputeBufferParam(cullingCS, 0, ShaderIDs.Culling._VisibilityInfo, data.Buffers.VisibilityInfo);
			context.cmd.SetComputeBufferParam(cullingCS, 0, ShaderIDs.Culling._LODGroupData, data.Buffers.LODGroupData);
			context.cmd.SetComputeBufferParam(cullingCS, 0, ShaderIDs.Culling._CullingJobs, data.Buffers.CullingJobs);
			context.cmd.SetComputeBufferParam(cullingCS, 0, ShaderIDs.Culling._BRGFrustumPlanes, sharedData.Buffers.FrustumPlanes);
			NativeList<CullingDispatchInfo> nativeList = new NativeList<CullingDispatchInfo>(sharedData.CullingContexts.Length, Allocator.Temp);
			foreach (int item in filteredCullingContexts)
			{
				int value = item;
				ref GPUDrivenCullingContext reference = ref UnsafeCollectionExtensions.ElementAsRef(in sharedData.CullingContexts, value);
				CullingDispatchInfo cullingDispatchInfo = default(CullingDispatchInfo);
				cullingDispatchInfo.CullingResourcesIndex = reference.CullingResourcesIndex;
				cullingDispatchInfo.InstanceVisibilityMaskIndex = reference.InstanceVisibilityMaskIndex;
				cullingDispatchInfo.ViewType = reference.ViewType;
				cullingDispatchInfo.ViewID = reference.ViewID;
				CullingDispatchInfo source = cullingDispatchInfo;
				if (nativeList.Length != 0)
				{
					if (nativeList[nativeList.Length - 1].CanBeBatchedWith(in source))
					{
						nativeList[nativeList.Length - 1].CullingContextIndices.Add(in value);
						continue;
					}
				}
				source.CullingContextIndices = new NativeList<int>(Allocator.Temp);
				source.CullingContextIndices.Add(in value);
				nativeList.Add(in source);
			}
			foreach (CullingDispatchInfo item2 in nativeList)
			{
				CullingDispatchInfo dispatchInfo = item2;
				DispatchGroupCulling(data, in context, in dispatchInfo);
			}
		}
	}

	private static void DispatchGroupCulling(PassData data, in UnsafeGraphContext context, in CullingDispatchInfo dispatchInfo)
	{
		int gPUCullingJobsCount = data.BRG.RendererGroupPool.ViewTypeInfos[(int)dispatchInfo.ViewType].GPUCullingJobsCount;
		ComputeShader cullingCS = data.BRG.Resources.CullingCS;
		GraphicsBuffer viewDependentLODGroupDataOrDefault = data.BRG.LODGroupRepository.GetViewDependentLODGroupDataOrDefault(dispatchInfo.ViewID);
		CoreUtils.SetKeyword(cullingCS, "HAS_VIEW_DEPENDENT_LOD_DATA", viewDependentLODGroupDataOrDefault != null);
		context.cmd.SetComputeBufferParam(cullingCS, 0, ShaderIDs.Culling._ViewDependentLODGroupData, viewDependentLODGroupDataOrDefault ?? ((GraphicsBuffer)data.Buffers.EmptyViewDependentLODData));
		int cullingResourcesIndex = dispatchInfo.CullingResourcesIndex;
		context.cmd.SetComputeBufferParam(cullingCS, 0, ShaderIDs.Culling._VisibleIndices, data.Buffers.VisibleIndices[cullingResourcesIndex]);
		BufferHandle bufferHandle = data.Buffers.CPUInstanceVisibilityMasks[cullingResourcesIndex];
		if (bufferHandle.IsValid())
		{
			context.cmd.SetComputeBufferParam(cullingCS, 0, ShaderIDs.Culling._CPUInstanceVisibilityMask, bufferHandle);
		}
		if (dispatchInfo.InstanceVisibilityMaskIndex.HasValue)
		{
			GPUDrivenVisibilityMaskPool.InstanceVisibilityMasks instanceVisibilityMasks = data.BRG.VisibilityMaskPool.VisibilityMasks[dispatchInfo.InstanceVisibilityMaskIndex.Value];
			context.cmd.SetComputeBufferParam(cullingCS, 0, ShaderIDs.Culling._InstanceVisibilityMask, instanceVisibilityMasks.MaskBuffer.InternalBuffer);
			context.cmd.SetComputeBufferParam(cullingCS, 0, ShaderIDs.Culling._InstanceVisibilityMaskPrev, instanceVisibilityMasks.PrevMaskBuffer.InternalBuffer);
		}
		SetCullingContextIndicesVectorArray(context.cmd, cullingCS, ShaderIDs.Culling._CullingContextIndices, dispatchInfo.CullingContextIndices);
		context.cmd.DispatchCompute(cullingCS, 0, gPUCullingJobsCount, dispatchInfo.CullingContextIndices.Length, 1);
	}

	private static void DispatchFixupIndirectArgs(PassData data, GPUDrivenCullingPassSharedData sharedData, in UnsafeGraphContext context, NativeList<int> filteredCullingContexts)
	{
		using (new ProfilingScope(context.cmd, Profiling.FixupIndirectArgsSampler))
		{
			NativeList<FixupIndirectArgsDispatchInfo> nativeList = new NativeList<FixupIndirectArgsDispatchInfo>(sharedData.CullingContexts.Length, Allocator.Temp);
			foreach (int item in filteredCullingContexts)
			{
				int value = item;
				ref GPUDrivenCullingContext reference = ref UnsafeCollectionExtensions.ElementAsRef(in sharedData.CullingContexts, value);
				FixupIndirectArgsDispatchInfo fixupIndirectArgsDispatchInfo = default(FixupIndirectArgsDispatchInfo);
				fixupIndirectArgsDispatchInfo.CullingResourcesIndex = reference.CullingResourcesIndex;
				FixupIndirectArgsDispatchInfo source = fixupIndirectArgsDispatchInfo;
				if (nativeList.Length != 0)
				{
					if (nativeList[nativeList.Length - 1].CanBeBatchedWith(in source))
					{
						nativeList[nativeList.Length - 1].CullingContextIndices.Add(in value);
						continue;
					}
				}
				source.CullingContextIndices = new NativeList<int>(Allocator.Temp);
				source.CullingContextIndices.Add(in value);
				nativeList.Add(in source);
			}
			foreach (FixupIndirectArgsDispatchInfo item2 in nativeList)
			{
				DispatchFixupIndirectArgs(data, sharedData, in context, item2);
			}
		}
	}

	private static void DispatchFixupIndirectArgs(PassData data, GPUDrivenCullingPassSharedData sharedData, in UnsafeGraphContext context, FixupIndirectArgsDispatchInfo dispatchInfo)
	{
		ComputeShader fixupIndirectArgsCS = data.BRG.Resources.FixupIndirectArgsCS;
		int maxRendererGroupSlicesPerView = sharedData.MaxRendererGroupSlicesPerView;
		context.cmd.SetComputeBufferParam(fixupIndirectArgsCS, 0, ShaderIDs.FixupIndirectArgs._IndirectArgs, data.Buffers.IndirectArgs[dispatchInfo.CullingResourcesIndex]);
		SetCullingContextIndicesVectorArray(context.cmd, fixupIndirectArgsCS, ShaderIDs.FixupIndirectArgs._CullingContextIndices, dispatchInfo.CullingContextIndices);
		context.cmd.DispatchCompute(fixupIndirectArgsCS, 0, GPUDrivenComputeShaders.ComputeGroupCount(maxRendererGroupSlicesPerView, 32), dispatchInfo.CullingContextIndices.Length, 1);
	}

	private static void SetCullingContextIndicesVectorArray(UnsafeCommandBuffer cmd, ComputeShader computeShader, int nameID, NativeList<int> nativeCullingContextIndices)
	{
		for (int i = 0; i < nativeCullingContextIndices.Length; i++)
		{
			s_SharedCullingContextIndices[i / 4][i % 4] = nativeCullingContextIndices[i];
		}
		cmd.SetComputeVectorArrayParam(computeShader, nameID, s_SharedCullingContextIndices);
	}

	private static void ReadbackVisibilityMask(PassData data, GPUDrivenCullingPassSharedData sharedData, in UnsafeGraphContext context, OcclusionCullingPassType occlusionCullingPassType, NativeList<int> filteredCullingContexts)
	{
		if (!data.BRG.Settings.OcclusionCulling || data.BRG.Settings.VisibilityMaskReadbackMode == GPUDrivenVisibilityMaskReadbackMode.Off || occlusionCullingPassType != OcclusionCullingPassType.FalseNegative)
		{
			return;
		}
		GPUDrivenVisibilityReadback visibilityReadback = data.BRG.InstanceCuller.VisibilityReadback;
		GPUDrivenVisibilityMaskPool visibilityMaskPool = data.BRG.InstanceCuller.VisibilityMaskPool;
		foreach (int item in filteredCullingContexts)
		{
			ref GPUDrivenCullingContext reference = ref UnsafeCollectionExtensions.ElementAsRef(in sharedData.CullingContexts, item);
			int? instanceVisibilityMaskIndex = reference.InstanceVisibilityMaskIndex;
			if (instanceVisibilityMaskIndex.HasValue)
			{
				BatchPackedCullingViewID viewID = reference.ViewID;
				ResizableGraphicsBuffer maskBuffer = visibilityMaskPool.VisibilityMasks[instanceVisibilityMaskIndex.Value].MaskBuffer;
				int count = maskBuffer.Count;
				int instanceID = viewID.GetInstanceID();
				visibilityReadback.OnCameraUsed(instanceID, count);
				Action<AsyncGPUReadbackRequest> readbackAction = visibilityReadback.GetReadbackAction(instanceID);
				context.cmd.RequestAsyncReadback(maskBuffer.InternalBuffer, readbackAction);
			}
		}
	}
}
