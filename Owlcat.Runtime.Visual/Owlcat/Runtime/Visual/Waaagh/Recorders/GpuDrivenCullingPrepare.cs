using Owlcat.Runtime.Core.Collections;
using Owlcat.Runtime.Visual.GPUDrivenBRG;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders;

internal static class GpuDrivenCullingPrepare
{
	private sealed class PassData
	{
		public GPUDrivenBatchRendererGroup BRG;

		public BufferHandle InstancesCreatedThisFrame;
	}

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

	private static readonly Plane[] OverrideCameraFrustumPlanes = new Plane[6];

	public static void Record(RenderGraph renderGraph, GPUDrivenBatchRendererGroup brg)
	{
		PassData passData2;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = renderGraph.AddUnsafePass<PassData>("GpuDriven.PrepareCulling", out passData2, WaaaghProfileId.GPUDrivenCullingPass_Prepare.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\GpuDriven\\GpuDrivenCullingPrepare.cs", 27);
		GPUDrivenCullingPassSharedData sharedPassData = brg.SharedPassData;
		sharedPassData.MaxRendererGroupSlicesPerView = ComputeMaxRendererGroupSlicesPerView(brg);
		sharedPassData.CullingContexts = brg.GetCullingContextsAndClear(Allocator.Temp);
		GPUDrivenCullingPassSharedData.UsedBuffers buffers = sharedPassData.Buffers;
		BufferDesc desc = new BufferDesc(math.max(1, sharedPassData.CullingContexts.Length), UnsafeUtility.SizeOf<GPUDrivenComputeShaders.GPUCullingContext>(), GraphicsBuffer.Target.Structured);
		buffers.CullingContexts = renderGraph.CreateBuffer(in desc);
		GPUDrivenCullingPassSharedData.UsedBuffers buffers2 = sharedPassData.Buffers;
		desc = new BufferDesc(math.max(1, sharedPassData.CullingContexts.Length) * 16, UnsafeUtility.SizeOf<float4>(), GraphicsBuffer.Target.Structured);
		buffers2.FrustumPlanes = renderGraph.CreateBuffer(in desc);
		GPUDrivenCullingPassSharedData.UsedBuffers buffers3 = sharedPassData.Buffers;
		desc = new BufferDesc(math.max(1, sharedPassData.MaxRendererGroupSlicesPerView * sharedPassData.CullingContexts.Length), 4, GraphicsBuffer.Target.Raw);
		buffers3.GroupCounters = renderGraph.CreateBuffer(in desc);
		sharedPassData.UsedInstanceVisibilityMaskIndices.Clear();
		foreach (GPUDrivenCullingContext cullingContext in sharedPassData.CullingContexts)
		{
			if (cullingContext.InstanceVisibilityMaskIndex.HasValue)
			{
				sharedPassData.UsedInstanceVisibilityMaskIndices.Add(cullingContext.InstanceVisibilityMaskIndex.Value);
			}
		}
		passData2.BRG = brg;
		PassData passData3 = passData2;
		desc = new BufferDesc(math.max(1, passData2.BRG.GetInstancesCreatedThisFrame().Length), 4, GraphicsBuffer.Target.Raw);
		passData3.InstancesCreatedThisFrame = renderGraph.CreateBuffer(in desc);
		unsafeRenderGraphBuilder.UseBuffer(in sharedPassData.Buffers.CullingContexts, AccessFlags.Write);
		unsafeRenderGraphBuilder.UseBuffer(in sharedPassData.Buffers.FrustumPlanes, AccessFlags.Write);
		unsafeRenderGraphBuilder.UseBuffer(in passData2.InstancesCreatedThisFrame, AccessFlags.Write);
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData passData, UnsafeGraphContext context)
		{
			passData.BRG.RendererGroupPool.UploadGroupsToGPU(context.cmd);
			GPUDrivenCullingPassSharedData sharedPassData2 = passData.BRG.SharedPassData;
			if (sharedPassData2.MaxRendererGroupSlicesPerView != 0 && sharedPassData2.CullingContexts.Length != 0)
			{
				DispatchClearInstanceVisibilityMasks(passData, sharedPassData2, in context);
				UpdateCullingContextsAndSetBufferData(passData, sharedPassData2, in context);
				DispatchUpdateInstancesCreatedThisFrame(passData, sharedPassData2, in context);
			}
		});
	}

	private static void DispatchClearInstanceVisibilityMasks(PassData data, GPUDrivenCullingPassSharedData sharedData, in UnsafeGraphContext context)
	{
		using (new ProfilingScope(context.cmd, Profiling.ClearInstanceVisibilityMasks))
		{
			(int count, int stride) tuple = GPUDrivenCullingResourcesPool.ComputeVisibilityMaskBufferCountAndStride(data.BRG.InstanceCapacity);
			int item = tuple.count;
			int item2 = tuple.stride;
			int sizeInBytes = item * item2;
			GPUDrivenVisibilityMaskPool visibilityMaskPool = data.BRG.VisibilityMaskPool;
			foreach (int usedInstanceVisibilityMaskIndex in sharedData.UsedInstanceVisibilityMaskIndices)
			{
				GPUDrivenVisibilityMaskPool.InstanceVisibilityMasks instanceVisibilityMasks = visibilityMaskPool.VisibilityMasks[usedInstanceVisibilityMaskIndex];
				CommandBuffer nativeCommandBuffer = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);
				data.BRG.BufferUtils.DispatchClearBuffer(nativeCommandBuffer, instanceVisibilityMasks.MaskBuffer.InternalBuffer, 0, 0, sizeInBytes);
			}
		}
	}

	private static void UpdateCullingContextsAndSetBufferData(PassData data, GPUDrivenCullingPassSharedData sharedData, in UnsafeGraphContext context)
	{
		using (new ProfilingScope(context.cmd, Profiling.UploadCullingContexts))
		{
			NativeArray<GPUDrivenComputeShaders.GPUCullingContext> gpuCullingContexts = new NativeArray<GPUDrivenComputeShaders.GPUCullingContext>(sharedData.CullingContexts.Length, Allocator.Temp);
			NativeArray<float4> frustumPlanes = new NativeArray<float4>(sharedData.CullingContexts.Length * 16, Allocator.Temp);
			BuildGPUCullingContextsAndFrustumPlanes(data, ref gpuCullingContexts, ref frustumPlanes);
			context.cmd.SetBufferData(sharedData.Buffers.CullingContexts, gpuCullingContexts);
			context.cmd.SetBufferData(sharedData.Buffers.FrustumPlanes, frustumPlanes);
		}
	}

	private static void BuildGPUCullingContextsAndFrustumPlanes(PassData data, ref NativeArray<GPUDrivenComputeShaders.GPUCullingContext> gpuCullingContexts, ref NativeArray<float4> frustumPlanes)
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

	private static void FillFrustumPlanes(PassData data, in GPUDrivenCullingContext cullingContext, NativeArray<float4> frustumPlanes, int frustumPlanesOffset, out int frustumPlanesCount)
	{
		frustumPlanesCount = cullingContext.FrustumPlanes.Length;
		for (int i = 0; i < cullingContext.FrustumPlanes.Length; i++)
		{
			Plane plane = cullingContext.FrustumPlanes[i];
			frustumPlanes[frustumPlanesOffset + i] = math.float4(plane.normal, plane.distance);
		}
	}

	private static void DispatchUpdateInstancesCreatedThisFrame(PassData data, GPUDrivenCullingPassSharedData sharedData, in UnsafeGraphContext context)
	{
		using (new ProfilingScope(context.cmd, Profiling.UpdateInstancesCreatedThisFrame))
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
			context.cmd.SetBufferData(data.InstancesCreatedThisFrame, instancesCreatedThisFrame);
			foreach (int usedInstanceVisibilityMaskIndex in sharedData.UsedInstanceVisibilityMaskIndices)
			{
				GraphicsBuffer internalBuffer = visibilityMaskPool.VisibilityMasks[usedInstanceVisibilityMaskIndex].PrevMaskBuffer.InternalBuffer;
				ComputeShader updateInstancesCreatedThisFrameCS = data.BRG.Resources.UpdateInstancesCreatedThisFrameCS;
				context.cmd.SetComputeBufferParam(updateInstancesCreatedThisFrameCS, 0, ShaderIDs.UpdateInstancesCreatedThisFrame._InstanceVisibilityMask, internalBuffer);
				context.cmd.SetComputeBufferParam(updateInstancesCreatedThisFrameCS, 0, ShaderIDs.UpdateInstancesCreatedThisFrame._InstancesCreatedThisFrame, data.InstancesCreatedThisFrame);
				context.cmd.SetComputeIntParam(updateInstancesCreatedThisFrameCS, ShaderIDs.UpdateInstancesCreatedThisFrame._InstancesCreatedThisFrameCount, instancesCreatedThisFrame.Length);
				context.cmd.DispatchCompute(updateInstancesCreatedThisFrameCS, 0, GPUDrivenComputeShaders.ComputeGroupCount(instancesCreatedThisFrame.Length, 32), 1, 1);
			}
		}
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
}
