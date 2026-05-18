using Owlcat.Runtime.Core.Collections;
using Owlcat.Runtime.Core.Math;
using Owlcat.Runtime.Visual.GPUDrivenBRG;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders;

internal static class GpuDrivenForwardReflectionProbes
{
	private sealed class PassData
	{
		public struct UsedBuffers
		{
			public BufferHandle PersistentIndices;

			public BufferHandle VisibilityInfo;

			public BufferHandle CullingContexts;

			public BufferHandle CullingJobs;

			public BufferHandle GroupInfo;

			public BufferHandle GroupCounters;

			public BufferHandle ProbeIndices;
		}

		public GPUDrivenBatchRendererGroup BRG;

		public UsedBuffers Buffers;

		public int CullingContextIndex;

		public int CullingJobsCount;
	}

	private static class ShaderIDs
	{
		public const int kKernelIndex = 0;

		public static readonly int _PersistentIndices = Shader.PropertyToID("_PersistentIndices");

		public static readonly int _VisibilityInfo = Shader.PropertyToID("_VisibilityInfo");

		public static readonly int _CullingContexts = Shader.PropertyToID("_CullingContexts");

		public static readonly int _CullingJobs = Shader.PropertyToID("_CullingJobs");

		public static readonly int _GroupInfo = Shader.PropertyToID("_GroupInfo");

		public static readonly int _GroupCounters = Shader.PropertyToID("_GroupCounters");

		public static readonly int _CullingContextIndex = Shader.PropertyToID("_CullingContextIndex");

		public static readonly int _CullingJobsCount = Shader.PropertyToID("_CullingJobsCount");

		public static readonly int waaagh_ForwardReflectionProbeIndices = Shader.PropertyToID("waaagh_ForwardReflectionProbeIndices");
	}

	private const int kInvalidCullingContextIndex = -1;

	public static void Record(in RecordContext context)
	{
		PassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<PassData>("GpuDriven.ForwardReflectionProbes", out passData, WaaaghProfileId.GPUDrivenForwardReflectionProbesPass.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\GpuDriven\\GpuDrivenForwardReflectionProbes.cs", 18);
		GPUDrivenBatchRendererGroup gPUDrivenBatchRendererGroup = context.GPUDrivenBatchRendererGroup;
		RenderGraph renderGraph = context.RenderGraph;
		GPUDrivenCullingPassSharedData sharedPassData = gPUDrivenBatchRendererGroup.SharedPassData;
		sharedPassData.Buffers.ForwardReflectionProbeIndices = default(BufferHandle);
		passData.BRG = gPUDrivenBatchRendererGroup;
		passData.CullingContextIndex = -1;
		for (int i = 0; i < sharedPassData.CullingContexts.Length; i++)
		{
			BatchPackedCullingViewID viewID = UnsafeCollectionExtensions.ElementAsRef(in sharedPassData.CullingContexts, i).ViewID;
			if (viewID.GetInstanceID() == context.CameraData.camera.GetInstanceID())
			{
				passData.CullingContextIndex = i;
				break;
			}
		}
		if (passData.CullingContextIndex == -1)
		{
			return;
		}
		ref GPUDrivenCullingContext reference = ref UnsafeCollectionExtensions.ElementAsRef(in sharedPassData.CullingContexts, passData.CullingContextIndex);
		ref GPUDrivenRendererGroupPool.ViewTypeInfo reference2 = ref gPUDrivenBatchRendererGroup.RendererGroupPool.ViewTypeInfos[(int)reference.ViewType];
		passData.CullingJobsCount = reference2.GPUCullingJobsCount;
		if (passData.CullingJobsCount != 0)
		{
			ref PassData.UsedBuffers buffers = ref passData.Buffers;
			BufferHandle input = renderGraph.ImportBuffer(gPUDrivenBatchRendererGroup.PersistentIndicesBuffer);
			buffers.PersistentIndices = unsafeRenderGraphBuilder.UseBuffer(in input);
			ref PassData.UsedBuffers buffers2 = ref passData.Buffers;
			input = renderGraph.ImportBuffer(gPUDrivenBatchRendererGroup.VisibilityInfoBuffer);
			buffers2.VisibilityInfo = unsafeRenderGraphBuilder.UseBuffer(in input);
			passData.Buffers.CullingContexts = unsafeRenderGraphBuilder.UseBuffer(in sharedPassData.Buffers.CullingContexts);
			ref PassData.UsedBuffers buffers3 = ref passData.Buffers;
			input = renderGraph.ImportBuffer(gPUDrivenBatchRendererGroup.GPUCullingJobsBuffer);
			buffers3.CullingJobs = unsafeRenderGraphBuilder.UseBuffer(in input);
			ref PassData.UsedBuffers buffers4 = ref passData.Buffers;
			input = renderGraph.ImportBuffer(gPUDrivenBatchRendererGroup.GroupInfoBuffer);
			buffers4.GroupInfo = unsafeRenderGraphBuilder.UseBuffer(in input);
			passData.Buffers.GroupCounters = unsafeRenderGraphBuilder.UseBuffer(in sharedPassData.Buffers.GroupCounters);
			int count = Alignment.AlignUp(math.max(1, gPUDrivenBatchRendererGroup.InstanceCapacity), 4) / 4;
			BufferDesc desc = new BufferDesc(count, 4, GraphicsBuffer.Target.Raw)
			{
				name = "waaagh_ForwardReflectionProbeIndices"
			};
			BufferHandle input2 = renderGraph.CreateBuffer(in desc);
			sharedPassData.Buffers.ForwardReflectionProbeIndices = (passData.Buffers.ProbeIndices = unsafeRenderGraphBuilder.UseBuffer(in input2, AccessFlags.Write));
			unsafeRenderGraphBuilder.AllowPassCulling(value: false);
			unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData data, UnsafeGraphContext context)
			{
				Render(data, context);
			});
		}
	}

	private static void Render(PassData data, UnsafeGraphContext context)
	{
		if (data.CullingContextIndex != -1 && data.CullingJobsCount != 0)
		{
			context.cmd.SetGlobalBuffer(ShaderIDs.waaagh_ForwardReflectionProbeIndices, data.Buffers.ProbeIndices);
			ComputeShader findForwardReflectionProbesCS = data.BRG.Resources.FindForwardReflectionProbesCS;
			context.cmd.SetComputeBufferParam(findForwardReflectionProbesCS, 0, ShaderIDs._PersistentIndices, data.Buffers.PersistentIndices);
			context.cmd.SetComputeBufferParam(findForwardReflectionProbesCS, 0, ShaderIDs._VisibilityInfo, data.Buffers.VisibilityInfo);
			context.cmd.SetComputeBufferParam(findForwardReflectionProbesCS, 0, ShaderIDs._CullingContexts, data.Buffers.CullingContexts);
			context.cmd.SetComputeBufferParam(findForwardReflectionProbesCS, 0, ShaderIDs._CullingJobs, data.Buffers.CullingJobs);
			context.cmd.SetComputeBufferParam(findForwardReflectionProbesCS, 0, ShaderIDs._GroupInfo, data.Buffers.GroupInfo);
			context.cmd.SetComputeBufferParam(findForwardReflectionProbesCS, 0, ShaderIDs._GroupCounters, data.Buffers.GroupCounters);
			context.cmd.SetComputeIntParam(findForwardReflectionProbesCS, ShaderIDs._CullingContextIndex, data.CullingContextIndex);
			context.cmd.SetComputeIntParam(findForwardReflectionProbesCS, ShaderIDs._CullingJobsCount, data.CullingJobsCount);
			int threadGroupsX = Alignment.AlignUp(data.CullingJobsCount, 32) / 32;
			context.cmd.DispatchCompute(findForwardReflectionProbesCS, 0, threadGroupsX, 1, 1);
		}
	}
}
