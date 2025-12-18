using System;
using System.Collections.Generic;
using Owlcat.Runtime.Core.Collections;
using Owlcat.Runtime.Visual.GPUDrivenBRG.LOD;
using Owlcat.Runtime.Visual.Waaagh;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.Passes;

public class GPUDrivenCullingPass : ScriptableRenderPass<GPUDrivenCullingPassData>, IDisposable
{
	public delegate bool CullingViewFilter(BatchCullingViewType batchCullingViewType, GPUDrivenRendererGroupPool.ViewType groupViewType);

	public enum GeometryPassType
	{
		Main,
		Shadows
	}

	public enum OcclusionCullingPassType
	{
		None,
		First,
		FalseNegative
	}

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

	private readonly CullingViewFilter m_CullingViewFilter;

	private readonly OcclusionCullingPassType m_OcclusionCullingPassType;

	public sealed override string Name { get; }

	private protected override WaaaghProfileId? ProfileId { get; }

	public GPUDrivenCullingPass(RenderPassEvent evt, OcclusionCullingPassType occlusionCullingPassType, CullingViewFilter cullingViewFilter, GeometryPassType geometryPassType = GeometryPassType.Main)
		: base(evt)
	{
		m_OcclusionCullingPassType = occlusionCullingPassType;
		m_CullingViewFilter = cullingViewFilter;
		Name = "GPUDrivenCulling";
		if (geometryPassType != 0)
		{
			Name = Name + "." + geometryPassType;
		}
		if (occlusionCullingPassType != 0)
		{
			Name += $".{occlusionCullingPassType}";
		}
		ProfileId = geometryPassType switch
		{
			GeometryPassType.Main => occlusionCullingPassType switch
			{
				OcclusionCullingPassType.None => WaaaghProfileId.GPUDrivenCullingPass, 
				OcclusionCullingPassType.First => WaaaghProfileId.GPUDrivenCullingPass_First, 
				OcclusionCullingPassType.FalseNegative => WaaaghProfileId.GPUDrivenCullingPass_FalseNegative, 
				_ => throw new ArgumentOutOfRangeException("occlusionCullingPassType", occlusionCullingPassType, null), 
			}, 
			GeometryPassType.Shadows => WaaaghProfileId.GPUDrivenCullingPass_Shadows, 
			_ => throw new ArgumentOutOfRangeException("geometryPassType", geometryPassType, null), 
		};
	}

	public void Dispose()
	{
	}

	protected override void Setup(RenderGraphBuilder builder, GPUDrivenCullingPassData data, ContextContainer frameData)
	{
		WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		GPUDrivenBatchRendererGroup gPUDrivenBatchRendererGroup = waaaghRenderingData.GPUDrivenBatchRendererGroup;
		GPUDrivenCullingPassSharedData sharedPassData = gPUDrivenBatchRendererGroup.SharedPassData;
		RenderGraph renderGraph = waaaghRenderingData.RenderGraph;
		data.BRG = gPUDrivenBatchRendererGroup;
		data.CameraType = waaaghCameraData.cameraType;
		data.Buffers.Clear();
		GPUDrivenCullingResourcesPool cullingResourcesPool = gPUDrivenBatchRendererGroup.CullingResourcesPool;
		for (int i = 0; i < cullingResourcesPool.UsedCount; i++)
		{
			GPUDrivenCullingResourcesPool.CullingResourceSet cullingResourceSet = cullingResourcesPool.Sets[i];
			data.Buffers.VisibleIndices.Add(renderGraph.ImportBuffer(cullingResourceSet.VisibleIndices.InternalBuffer));
			data.Buffers.IndirectArgs.Add(renderGraph.ImportBuffer(cullingResourceSet.IndirectArgs.InternalBuffer));
			data.Buffers.CPUInstanceVisibilityMasks.Add(cullingResourceSet.CPUInstanceVisibilityMask.IsCreated ? renderGraph.ImportBuffer(cullingResourceSet.CPUInstanceVisibilityMask.InternalBuffer) : default(BufferHandle));
		}
		data.Buffers.PersistentIndices = renderGraph.ImportBuffer(gPUDrivenBatchRendererGroup.PersistentIndicesBuffer);
		data.Buffers.VisibilityInfo = renderGraph.ImportBuffer(gPUDrivenBatchRendererGroup.VisibilityInfoBuffer);
		data.Buffers.LODGroupData = renderGraph.ImportBuffer(gPUDrivenBatchRendererGroup.LODGroupRepository.GPUDataBuffer);
		GPUDrivenCullingPassData.UsedBuffers buffers = data.Buffers;
		BufferDesc desc = new BufferDesc(1, 4, GraphicsBuffer.Target.Raw)
		{
			name = "EmptyViewDependentLODData"
		};
		buffers.EmptyViewDependentLODData = builder.CreateTransientBuffer(in desc);
		data.Buffers.GroupInfo = renderGraph.ImportBuffer(gPUDrivenBatchRendererGroup.GroupInfoBuffer);
		data.Buffers.CullingJobs = renderGraph.ImportBuffer(gPUDrivenBatchRendererGroup.GPUCullingJobsBuffer);
		if (m_OcclusionCullingPassType == OcclusionCullingPassType.FalseNegative)
		{
			data.Textures.CameraDepthPyramid = builder.ReadTexture(in waaaghResourceData.CameraDepthPyramidRT);
			GPUDrivenCullingPassData.UsedTextures textures = data.Textures;
			TextureHandle input = waaaghResourceData.CameraDepthBuffer;
			textures.CameraDepthBuffer = builder.ReadTexture(in input);
		}
		else
		{
			data.Textures.CameraDepthPyramid = default(TextureHandle);
			data.Textures.CameraDepthBuffer = default(TextureHandle);
		}
		if (m_OcclusionCullingPassType == OcclusionCullingPassType.FalseNegative && sharedPassData.Buffers.MainViewOcclusionTestDebug.IsValid())
		{
			builder.WriteBuffer(in sharedPassData.Buffers.MainViewOcclusionTestDebug);
		}
		builder.AllowPassCulling(value: false);
		builder.ReadBuffer(in data.Buffers.PersistentIndices);
		builder.ReadBuffer(in data.Buffers.VisibilityInfo);
		builder.ReadBuffer(in data.Buffers.LODGroupData);
		builder.ReadBuffer(in sharedPassData.Buffers.FrustumPlanes);
		builder.ReadBuffer(in sharedPassData.Buffers.CullingContexts);
		builder.WriteBuffer(in data.Buffers.GroupInfo);
		builder.WriteBuffer(in data.Buffers.CullingJobs);
		builder.WriteBuffer(in sharedPassData.Buffers.GroupCounters);
		foreach (BufferHandle indirectArg in data.Buffers.IndirectArgs)
		{
			BufferHandle input2 = indirectArg;
			builder.WriteBuffer(in input2);
		}
		foreach (BufferHandle visibleIndex in data.Buffers.VisibleIndices)
		{
			BufferHandle input3 = visibleIndex;
			builder.WriteBuffer(in input3);
		}
		foreach (BufferHandle cPUInstanceVisibilityMask in data.Buffers.CPUInstanceVisibilityMasks)
		{
			BufferHandle input4 = cPUInstanceVisibilityMask;
			if (input4.IsValid())
			{
				builder.WriteBuffer(in input4);
			}
		}
	}

	protected override void Render(GPUDrivenCullingPassData data, RenderGraphContext context)
	{
		GPUDrivenCullingPassSharedData sharedPassData = data.BRG.SharedPassData;
		if (sharedPassData.MaxRendererGroupSlicesPerView == 0 || sharedPassData.CullingContexts.Length == 0)
		{
			return;
		}
		NativeList<int> filteredCullingContexts = FilterCullingContexts(sharedPassData, m_CullingViewFilter);
		UploadLODBuffers(data, context, sharedPassData, filteredCullingContexts);
		OcclusionCullingPassType occlusionCullingPassType = m_OcclusionCullingPassType;
		if (occlusionCullingPassType == OcclusionCullingPassType.None || occlusionCullingPassType == OcclusionCullingPassType.First)
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
		DispatchCulling(data, sharedPassData, in context, m_OcclusionCullingPassType, filteredCullingContexts);
		DispatchFixupIndirectArgs(data, sharedPassData, in context, filteredCullingContexts);
		ReadbackVisibilityMask(data, sharedPassData, in context, m_OcclusionCullingPassType, filteredCullingContexts);
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

	private static void UploadLODBuffers(GPUDrivenCullingPassData data, RenderGraphContext context, GPUDrivenCullingPassSharedData sharedData, NativeList<int> filteredCullingContexts)
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
						item2.Complete(context.cmd);
					}
				}
			}
		}
	}

	private static void SetCPUInstanceVisibilityMaskData(GPUDrivenCullingPassData data, GPUDrivenCullingPassSharedData sharedData, RenderGraphContext context)
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

	private static void DispatchClearBatchCounters(GPUDrivenCullingPassData data, GPUDrivenCullingPassSharedData sharedData, in RenderGraphContext context)
	{
		ProfilingScope profilingScope = new ProfilingScope(context.cmd, Profiling.ClearBatchCountersSampler);
		try
		{
			data.BRG.BufferUtils.DispatchClearBuffer(context.cmd, sharedData.Buffers.GroupCounters, 0);
		}
		finally
		{
			((IDisposable)profilingScope).Dispose();
		}
	}

	private static void DispatchCulling(GPUDrivenCullingPassData data, GPUDrivenCullingPassSharedData sharedData, in RenderGraphContext context, OcclusionCullingPassType occlusionCullingPassType, NativeList<int> filteredCullingContexts)
	{
		ProfilingScope profilingScope = new ProfilingScope(context.cmd, Profiling.Culling);
		try
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
			context.cmd.SetComputeBufferParam(cullingCS, 0, ShaderIDs.Culling._PersistentIndices, data.Buffers.PersistentIndices);
			context.cmd.SetComputeBufferParam(cullingCS, 0, ShaderIDs.Culling._VisibilityInfo, data.Buffers.VisibilityInfo);
			context.cmd.SetComputeBufferParam(cullingCS, 0, ShaderIDs.Culling._LODGroupData, data.Buffers.LODGroupData);
			context.cmd.SetComputeBufferParam(cullingCS, 0, ShaderIDs.Culling._CullingJobs, data.Buffers.CullingJobs);
			context.cmd.SetComputeBufferParam(cullingCS, 0, ShaderIDs.Culling._BRGFrustumPlanes, sharedData.Buffers.FrustumPlanes);
			SetComputeTextureParamOrDefault(context.cmd, cullingCS, 0, ShaderPropertyId._CameraDepthTexture, data.Textures.CameraDepthBuffer);
			SetComputeTextureParamOrDefault(context.cmd, cullingCS, 0, ShaderPropertyId._CameraDepthPyramidRT, data.Textures.CameraDepthPyramid);
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
		finally
		{
			((IDisposable)profilingScope).Dispose();
		}
	}

	private static void SetComputeTextureParamOrDefault(CommandBuffer cmd, ComputeShader computeShader, int kernelIndex, int nameID, TextureHandle textureHandle)
	{
		if (textureHandle.IsValid())
		{
			cmd.SetComputeTextureParam(computeShader, kernelIndex, nameID, textureHandle);
		}
	}

	private static void DispatchGroupCulling(GPUDrivenCullingPassData data, in RenderGraphContext context, in CullingDispatchInfo dispatchInfo)
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

	private static void DispatchFixupIndirectArgs(GPUDrivenCullingPassData data, GPUDrivenCullingPassSharedData sharedData, in RenderGraphContext context, NativeList<int> filteredCullingContexts)
	{
		ProfilingScope profilingScope = new ProfilingScope(context.cmd, Profiling.FixupIndirectArgsSampler);
		try
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
		finally
		{
			((IDisposable)profilingScope).Dispose();
		}
	}

	private static void DispatchFixupIndirectArgs(GPUDrivenCullingPassData data, GPUDrivenCullingPassSharedData sharedData, in RenderGraphContext context, FixupIndirectArgsDispatchInfo dispatchInfo)
	{
		ComputeShader fixupIndirectArgsCS = data.BRG.Resources.FixupIndirectArgsCS;
		int maxRendererGroupSlicesPerView = sharedData.MaxRendererGroupSlicesPerView;
		context.cmd.SetComputeBufferParam(fixupIndirectArgsCS, 0, ShaderIDs.FixupIndirectArgs._IndirectArgs, data.Buffers.IndirectArgs[dispatchInfo.CullingResourcesIndex]);
		SetCullingContextIndicesVectorArray(context.cmd, fixupIndirectArgsCS, ShaderIDs.FixupIndirectArgs._CullingContextIndices, dispatchInfo.CullingContextIndices);
		context.cmd.DispatchCompute(fixupIndirectArgsCS, 0, GPUDrivenComputeShaders.ComputeGroupCount(maxRendererGroupSlicesPerView, 32), dispatchInfo.CullingContextIndices.Length, 1);
	}

	private static void SetCullingContextIndicesVectorArray(CommandBuffer cmd, ComputeShader computeShader, int nameID, NativeList<int> nativeCullingContextIndices)
	{
		for (int i = 0; i < nativeCullingContextIndices.Length; i++)
		{
			s_SharedCullingContextIndices[i / 4][i % 4] = nativeCullingContextIndices[i];
		}
		cmd.SetComputeVectorArrayParam(computeShader, nameID, s_SharedCullingContextIndices);
	}

	private static void ReadbackVisibilityMask(GPUDrivenCullingPassData data, GPUDrivenCullingPassSharedData sharedData, in RenderGraphContext context, OcclusionCullingPassType occlusionCullingPassType, NativeList<int> filteredCullingContexts)
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
