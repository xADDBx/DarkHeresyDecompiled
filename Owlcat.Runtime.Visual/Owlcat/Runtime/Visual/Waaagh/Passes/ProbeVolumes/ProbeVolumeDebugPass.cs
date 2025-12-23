using System;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.ProbeVolumes;

public class ProbeVolumeDebugPass : ScriptableRenderPass
{
	private class PassData
	{
		public ComputeShader ComputeShader;

		public Vector4 PositionSS;

		public BufferHandle ResultBuffer;
	}

	private static readonly BaseRenderFunc<PassData, RenderGraphContext> s_RenderFunc = ExecutePass;

	private readonly ComputeShader m_ComputeShader;

	public override string Name => "ProbeVolumeDebugPass";

	public ProbeVolumeDebugPass(RenderPassEvent evt, ComputeShader computeShader)
		: base(evt)
	{
		m_ComputeShader = computeShader;
	}

	public override void RecordRenderGraph(ContextContainer frameData)
	{
		if (!ProbeReferenceVolume.instance.isInitialized)
		{
			return;
		}
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		if (!ProbeReferenceVolume.instance.GetProbeSamplingDebugResources(waaaghCameraData.camera, out var resultBuffer, out var coords))
		{
			return;
		}
		WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		PassData passData;
		RenderGraphBuilder renderGraphBuilder = waaaghRenderingData.RenderGraph.AddRenderPass<PassData>(Name, out passData, ".\\Library\\PackageCache\\com.owlcat.visual@141c9a01de77\\Runtime\\Waaagh\\Passes\\ProbeVolumes\\ProbeVolumeDebugPass.cs", 35);
		try
		{
			passData.ComputeShader = m_ComputeShader;
			passData.PositionSS = new Vector4(coords.x, coords.y, 0f, 0f);
			passData.ResultBuffer = waaaghRenderingData.RenderGraph.ImportBuffer(resultBuffer);
			renderGraphBuilder.WriteBuffer(in passData.ResultBuffer);
			renderGraphBuilder.ReadTexture(in waaaghResourceData.CameraNormalsRT);
			TextureHandle input = waaaghResourceData.CameraDepthBuffer;
			renderGraphBuilder.ReadTexture(in input);
			renderGraphBuilder.SetRenderFunc(s_RenderFunc);
		}
		finally
		{
			((IDisposable)renderGraphBuilder).Dispose();
		}
	}

	private static void ExecutePass(PassData passData, RenderGraphContext context)
	{
		int kernelIndex = passData.ComputeShader.FindKernel("ComputePositionNormal");
		context.cmd.SetComputeVectorParam(passData.ComputeShader, "_PositionSS", passData.PositionSS);
		context.cmd.SetComputeBufferParam(passData.ComputeShader, kernelIndex, "_ResultBuffer", passData.ResultBuffer);
		context.cmd.DispatchCompute(passData.ComputeShader, kernelIndex, 1, 1, 1);
	}
}
