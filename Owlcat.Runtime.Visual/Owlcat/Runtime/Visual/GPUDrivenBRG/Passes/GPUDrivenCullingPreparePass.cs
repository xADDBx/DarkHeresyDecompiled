using System;
using Owlcat.Runtime.Core.Collections;
using Owlcat.Runtime.Visual.Waaagh;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.Passes;

public class GPUDrivenCullingPreparePass : ScriptableRenderPass<GPUDrivenCullingPreparePassData>, IDisposable
{
	private static class Profiling
	{
		public static readonly ProfilingSampler UploadCullingContexts = new ProfilingSampler("Upload Culling Contexts");

		public static readonly ProfilingSampler ClearInstanceVisibilityMasks = new ProfilingSampler("Clear Instance Visibility Masks");

		public static readonly ProfilingSampler UpdateInstancesCreatedThisFrame = new ProfilingSampler("Update Instances Created This Frame");
	}

	private static class ShaderIDs
	{
		public static class UpdateInstancesCreatedThisFrame
		{
			public const int KernelIndex = 0;

			public static readonly int _InstanceVisibilityMask = Shader.PropertyToID("_InstanceVisibilityMask");

			public static readonly int _InstancesCreatedThisFrame = Shader.PropertyToID("_InstancesCreatedThisFrame");

			public static readonly int _InstancesCreatedThisFrameCount = Shader.PropertyToID("_InstancesCreatedThisFrameCount");
		}
	}

	private const int kMaxFrustumPlanesPerSplit = 16;

	public override string Name => "GPUDrivenCulling.PreparePass";

	private protected override WaaaghProfileId? ProfileId => WaaaghProfileId.GPUDrivenCullingPass_Prepare;

	public GPUDrivenCullingPreparePass(RenderPassEvent evt)
		: base(evt)
	{
	}

	public void Dispose()
	{
	}

	protected override void Setup(RenderGraphBuilder builder, GPUDrivenCullingPreparePassData data, ContextContainer frameData)
	{
		WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
		GPUDrivenBatchRendererGroup gPUDrivenBatchRendererGroup = waaaghRenderingData.GPUDrivenBatchRendererGroup;
		GPUDrivenCullingPassSharedData sharedPassData = gPUDrivenBatchRendererGroup.SharedPassData;
		RenderGraph renderGraph = waaaghRenderingData.RenderGraph;
		data.BRG = gPUDrivenBatchRendererGroup;
		sharedPassData.MaxRendererGroupSlicesPerView = ComputeMaxRendererGroupSlicesPerView(gPUDrivenBatchRendererGroup);
		sharedPassData.CullingContexts = gPUDrivenBatchRendererGroup.GetCullingContextsAndClear(Allocator.Temp);
		GPUDrivenCullingPassSharedData.UsedBuffers buffers = sharedPassData.Buffers;
		BufferDesc desc = new BufferDesc(math.max(1, sharedPassData.CullingContexts.Length), UnsafeUtility.SizeOf<GPUDrivenComputeShaders.GPUCullingContext>(), GraphicsBuffer.Target.Structured);
		buffers.CullingContexts = renderGraph.CreateBuffer(in desc);
		GPUDrivenCullingPassSharedData.UsedBuffers buffers2 = sharedPassData.Buffers;
		desc = new BufferDesc(math.max(1, sharedPassData.CullingContexts.Length) * 16, UnsafeUtility.SizeOf<float4>(), GraphicsBuffer.Target.Structured);
		buffers2.FrustumPlanes = renderGraph.CreateBuffer(in desc);
		GPUDrivenCullingPassSharedData.UsedBuffers buffers3 = sharedPassData.Buffers;
		desc = new BufferDesc(math.max(1, sharedPassData.MaxRendererGroupSlicesPerView * sharedPassData.CullingContexts.Length), 4, GraphicsBuffer.Target.Raw);
		buffers3.GroupCounters = renderGraph.CreateBuffer(in desc);
		GPUDrivenCullingPreparePassData.UsedBuffers buffers4 = data.Buffers;
		desc = new BufferDesc(math.max(1, data.BRG.GetInstancesCreatedThisFrame().Length), 4, GraphicsBuffer.Target.Raw);
		buffers4.InstancesCreatedThisFrame = renderGraph.CreateBuffer(in desc);
		sharedPassData.UsedInstanceVisibilityMaskIndices.Clear();
		foreach (GPUDrivenCullingContext cullingContext in sharedPassData.CullingContexts)
		{
			if (cullingContext.InstanceVisibilityMaskIndex.HasValue)
			{
				sharedPassData.UsedInstanceVisibilityMaskIndices.Add(cullingContext.InstanceVisibilityMaskIndex.Value);
			}
		}
		builder.WriteBuffer(in sharedPassData.Buffers.CullingContexts);
		builder.WriteBuffer(in sharedPassData.Buffers.FrustumPlanes);
		builder.WriteBuffer(in data.Buffers.InstancesCreatedThisFrame);
	}

	private static int ComputeMaxRendererGroupSlicesPerView(GPUDrivenBatchRendererGroup brg)
	{
		int num = 0;
		GPUDrivenRendererGroupPool.ViewTypeInfo[] viewTypeInfos = brg.RendererGroupPool.ViewTypeInfos;
		for (int i = 0; i < viewTypeInfos.Length; i++)
		{
			GPUDrivenRendererGroupPool.ViewTypeInfo viewTypeInfo = viewTypeInfos[i];
			num = math.max(num, viewTypeInfo.RendererGroupSlicesCount);
		}
		return num;
	}

	protected override void Render(GPUDrivenCullingPreparePassData data, RenderGraphContext context)
	{
		GPUDrivenBatchRendererGroup bRG = data.BRG;
		bRG.RendererGroupPool.UploadGroupsToGPU(context.cmd);
		GPUDrivenCullingPassSharedData sharedPassData = bRG.SharedPassData;
		if (sharedPassData.MaxRendererGroupSlicesPerView != 0 && sharedPassData.CullingContexts.Length != 0)
		{
			DispatchClearInstanceVisibilityMasks(data, sharedPassData, in context);
			UpdateCullingContextsAndSetBufferData(data, sharedPassData, in context);
			DispatchUpdateInstancesCreatedThisFrame(data, sharedPassData, in context);
		}
	}

	private static void DispatchClearInstanceVisibilityMasks(GPUDrivenCullingPreparePassData data, GPUDrivenCullingPassSharedData sharedData, in RenderGraphContext context)
	{
		ProfilingScope profilingScope = new ProfilingScope(context.cmd, Profiling.ClearInstanceVisibilityMasks);
		try
		{
			(int count, int stride) tuple = GPUDrivenCullingResourcesPool.ComputeVisibilityMaskBufferCountAndStride(data.BRG.InstanceCapacity);
			int item = tuple.count;
			int item2 = tuple.stride;
			int sizeInBytes = item * item2;
			GPUDrivenVisibilityMaskPool visibilityMaskPool = data.BRG.VisibilityMaskPool;
			foreach (int usedInstanceVisibilityMaskIndex in sharedData.UsedInstanceVisibilityMaskIndices)
			{
				GPUDrivenVisibilityMaskPool.InstanceVisibilityMasks instanceVisibilityMasks = visibilityMaskPool.VisibilityMasks[usedInstanceVisibilityMaskIndex];
				data.BRG.BufferUtils.DispatchClearBuffer(context.cmd, instanceVisibilityMasks.MaskBuffer.InternalBuffer, 0, 0, sizeInBytes);
			}
		}
		finally
		{
			((IDisposable)profilingScope).Dispose();
		}
	}

	private static void UpdateCullingContextsAndSetBufferData(GPUDrivenCullingPreparePassData data, GPUDrivenCullingPassSharedData sharedData, in RenderGraphContext context)
	{
		ProfilingScope profilingScope = new ProfilingScope(context.cmd, Profiling.UploadCullingContexts);
		try
		{
			NativeArray<GPUDrivenComputeShaders.GPUCullingContext> gpuCullingContexts = new NativeArray<GPUDrivenComputeShaders.GPUCullingContext>(sharedData.CullingContexts.Length, Allocator.Temp);
			NativeArray<float4> frustumPlanes = new NativeArray<float4>(sharedData.CullingContexts.Length * 16, Allocator.Temp);
			BuildGPUCullingContextsAndFrustumPlanes(data, ref gpuCullingContexts, ref frustumPlanes);
			context.cmd.SetBufferData(sharedData.Buffers.CullingContexts, gpuCullingContexts);
			context.cmd.SetBufferData(sharedData.Buffers.FrustumPlanes, frustumPlanes);
		}
		finally
		{
			((IDisposable)profilingScope).Dispose();
		}
	}

	private static void BuildGPUCullingContextsAndFrustumPlanes(GPUDrivenCullingPreparePassData data, ref NativeArray<GPUDrivenComputeShaders.GPUCullingContext> gpuCullingContexts, ref NativeArray<float4> frustumPlanes)
	{
		GPUDrivenCullingPassSharedData sharedPassData = data.BRG.SharedPassData;
		int num = 0;
		for (int i = 0; i < sharedPassData.CullingContexts.Length; i++)
		{
			ref GPUDrivenCullingContext reference = ref UnsafeCollectionExtensions.ElementAsRef(in sharedPassData.CullingContexts, i);
			ref GPUDrivenComputeShaders.GPUCullingContext reference2 = ref UnsafeCollectionExtensions.ElementAsRef(in gpuCullingContexts, i);
			FillFrustumPlanes(data, in reference, frustumPlanes, num, out var frustumPlanesCount);
			reference2.BatchCullingViewType = (uint)reference.BatchCullingViewType;
			reference2.WorldToLightSpaceRotation = new float4x4(reference.WorldToLightSpaceRotation, float3.zero);
			reference2.CullingSphereLS = reference.CullingSphereLS;
			reference2.CameraPosition = math.float4(reference.CameraPosition, 1f);
			reference2.CullingMatrix = GL.GetGPUProjectionMatrix(reference.CullingMatrix, renderIntoTexture: true);
			reference2.VisibleIndicesOffset = (uint)reference.VisibleIndicesOffset;
			reference2.FrustumPlanesOffset = (uint)num;
			reference2.FrustumPlanesCount = (uint)frustumPlanesCount;
			reference2.GroupCountersOffset = (uint)(i * sharedPassData.MaxRendererGroupSlicesPerView);
			ref uint sceneCullingMaskLow = ref reference2.SceneCullingMaskLow;
			ref uint sceneCullingMaskHigh = ref reference2.SceneCullingMaskHigh;
			(uint, uint) tuple = GPUDrivenMathUtils.SplitULong(reference.SceneCullingMask);
			sceneCullingMaskLow = tuple.Item1;
			sceneCullingMaskHigh = tuple.Item2;
			reference2.IndirectArgsOffset = (uint)reference.IndirectArgsOffset;
			reference2.PersistentIndicesOffset = (uint)reference.PersistentIndicesOffset;
			reference2.CPUInstanceVisibilityMaskOffset = (uint)reference.CPUInstanceVisibilityMaskOffset;
			ref GPUDrivenRendererGroupPool.ViewTypeInfo reference3 = ref data.BRG.RendererGroupPool.ViewTypeInfos[(int)reference.ViewType];
			reference2.CullingJobsOffset = (uint)reference3.GPUCullingJobsOffset;
			reference2.GroupsOffset = (uint)reference3.RendererGroupSlicesOffset;
			reference2.GroupCount = (uint)reference3.RendererGroupSlicesCount;
			reference2.CameraType = reference.CameraType;
			reference2.LOD_IsOrtho = (reference.LOD.IsOrtho ? 1u : 0u);
			reference2.LOD_UseSelectionForcedLOD = (reference.LOD.UseSelectionForcedLOD ? 1u : 0u);
			reference2.LOD_SqrScreenRelativeMetric = reference.LOD.SqrScreenRelativeMetric;
			reference2.LOD_CameraPosition = reference.LOD.CameraPosition;
			reference2.LOD_MaxLOD = reference.LOD.MaxLOD;
			reference2.LOD_FixedLODIndex = reference.LOD.FixedLODIndex;
			reference2.LOD_LODBias = reference.LOD.LODBias;
			num += frustumPlanesCount;
		}
	}

	private static void FillFrustumPlanes(GPUDrivenCullingPreparePassData data, in GPUDrivenCullingContext cullingContext, NativeArray<float4> frustumPlanes, int frustumPlanesOffset, out int frustumPlanesCount)
	{
		frustumPlanesCount = cullingContext.FrustumPlanes.Length;
		for (int i = 0; i < cullingContext.FrustumPlanes.Length; i++)
		{
			Plane plane = cullingContext.FrustumPlanes[i];
			frustumPlanes[frustumPlanesOffset + i] = math.float4(plane.normal, plane.distance);
		}
	}

	private static void DispatchUpdateInstancesCreatedThisFrame(GPUDrivenCullingPreparePassData data, GPUDrivenCullingPassSharedData sharedData, in RenderGraphContext context)
	{
		ProfilingScope profilingScope = new ProfilingScope(context.cmd, Profiling.UpdateInstancesCreatedThisFrame);
		try
		{
			NativeArray<int> instancesCreatedThisFrame = data.BRG.GetInstancesCreatedThisFrame();
			if (instancesCreatedThisFrame.Length == 0)
			{
				return;
			}
			GPUDrivenVisibilityMaskPool visibilityMaskPool = data.BRG.VisibilityMaskPool;
			if (sharedData.UsedInstanceVisibilityMaskIndices.Count <= 0)
			{
				return;
			}
			context.cmd.SetBufferData(data.Buffers.InstancesCreatedThisFrame, instancesCreatedThisFrame);
			foreach (int usedInstanceVisibilityMaskIndex in sharedData.UsedInstanceVisibilityMaskIndices)
			{
				GraphicsBuffer internalBuffer = visibilityMaskPool.VisibilityMasks[usedInstanceVisibilityMaskIndex].PrevMaskBuffer.InternalBuffer;
				ComputeShader updateInstancesCreatedThisFrameCS = data.BRG.Resources.UpdateInstancesCreatedThisFrameCS;
				context.cmd.SetComputeBufferParam(updateInstancesCreatedThisFrameCS, 0, ShaderIDs.UpdateInstancesCreatedThisFrame._InstanceVisibilityMask, internalBuffer);
				context.cmd.SetComputeBufferParam(updateInstancesCreatedThisFrameCS, 0, ShaderIDs.UpdateInstancesCreatedThisFrame._InstancesCreatedThisFrame, data.Buffers.InstancesCreatedThisFrame);
				context.cmd.SetComputeIntParam(updateInstancesCreatedThisFrameCS, ShaderIDs.UpdateInstancesCreatedThisFrame._InstancesCreatedThisFrameCount, instancesCreatedThisFrame.Length);
				context.cmd.DispatchCompute(updateInstancesCreatedThisFrameCS, 0, GPUDrivenComputeShaders.ComputeGroupCount(instancesCreatedThisFrame.Length, 32), 1, 1);
			}
		}
		finally
		{
			((IDisposable)profilingScope).Dispose();
		}
	}
}
