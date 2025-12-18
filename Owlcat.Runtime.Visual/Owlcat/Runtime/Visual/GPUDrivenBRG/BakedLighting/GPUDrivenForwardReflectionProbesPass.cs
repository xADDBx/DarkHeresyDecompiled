using Owlcat.Runtime.Core.Collections;
using Owlcat.Runtime.Core.Math;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Passes;
using Owlcat.Runtime.Visual.Waaagh;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.BakedLighting;

public class GPUDrivenForwardReflectionProbesPass : ScriptableRenderPass<GPUDrivenReflectionProbesPassData>
{
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

	public override string Name => "GPUDrivenForwardReflectionProbesPass";

	private protected override WaaaghProfileId? ProfileId => WaaaghProfileId.GPUDrivenForwardReflectionProbesPass;

	public GPUDrivenForwardReflectionProbesPass(RenderPassEvent evt)
		: base(evt)
	{
	}

	protected override void Setup(RenderGraphBuilder builder, GPUDrivenReflectionProbesPassData data, ContextContainer frameData)
	{
		WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		RenderGraph renderGraph = waaaghRenderingData.RenderGraph;
		GPUDrivenBatchRendererGroup gPUDrivenBatchRendererGroup = waaaghRenderingData.GPUDrivenBatchRendererGroup;
		GPUDrivenCullingPassSharedData sharedPassData = gPUDrivenBatchRendererGroup.SharedPassData;
		sharedPassData.Buffers.ForwardReflectionProbeIndices = default(BufferHandle);
		data.BRG = gPUDrivenBatchRendererGroup;
		data.CullingContextIndex = -1;
		for (int i = 0; i < sharedPassData.CullingContexts.Length; i++)
		{
			BatchPackedCullingViewID viewID = UnsafeCollectionExtensions.ElementAsRef(in sharedPassData.CullingContexts, i).ViewID;
			if (viewID.GetInstanceID() == waaaghCameraData.camera.GetInstanceID())
			{
				data.CullingContextIndex = i;
				break;
			}
		}
		if (data.CullingContextIndex != -1)
		{
			ref GPUDrivenCullingContext reference = ref UnsafeCollectionExtensions.ElementAsRef(in sharedPassData.CullingContexts, data.CullingContextIndex);
			data.CullingJobsCount = gPUDrivenBatchRendererGroup.RendererGroupPool.ViewTypeInfos[(int)reference.ViewType].GPUCullingJobsCount;
			if (data.CullingJobsCount != 0)
			{
				ref GPUDrivenReflectionProbesPassData.UsedBuffers buffers = ref data.Buffers;
				BufferHandle input = renderGraph.ImportBuffer(gPUDrivenBatchRendererGroup.PersistentIndicesBuffer);
				buffers.PersistentIndices = builder.ReadBuffer(in input);
				ref GPUDrivenReflectionProbesPassData.UsedBuffers buffers2 = ref data.Buffers;
				input = renderGraph.ImportBuffer(gPUDrivenBatchRendererGroup.VisibilityInfoBuffer);
				buffers2.VisibilityInfo = builder.ReadBuffer(in input);
				data.Buffers.CullingContexts = builder.ReadBuffer(in sharedPassData.Buffers.CullingContexts);
				ref GPUDrivenReflectionProbesPassData.UsedBuffers buffers3 = ref data.Buffers;
				input = renderGraph.ImportBuffer(gPUDrivenBatchRendererGroup.GPUCullingJobsBuffer);
				buffers3.CullingJobs = builder.ReadBuffer(in input);
				ref GPUDrivenReflectionProbesPassData.UsedBuffers buffers4 = ref data.Buffers;
				input = renderGraph.ImportBuffer(gPUDrivenBatchRendererGroup.GroupInfoBuffer);
				buffers4.GroupInfo = builder.ReadBuffer(in input);
				data.Buffers.GroupCounters = builder.ReadBuffer(in sharedPassData.Buffers.GroupCounters);
				int count = Alignment.AlignUp(math.max(1, gPUDrivenBatchRendererGroup.InstanceCapacity), 4) / 4;
				BufferDesc desc = new BufferDesc(count, 4, GraphicsBuffer.Target.Raw)
				{
					name = "waaagh_ForwardReflectionProbeIndices"
				};
				BufferHandle input2 = renderGraph.CreateBuffer(in desc);
				sharedPassData.Buffers.ForwardReflectionProbeIndices = (data.Buffers.ProbeIndices = builder.WriteBuffer(in input2));
				builder.AllowPassCulling(value: false);
			}
		}
	}

	protected override void Render(GPUDrivenReflectionProbesPassData data, RenderGraphContext context)
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
