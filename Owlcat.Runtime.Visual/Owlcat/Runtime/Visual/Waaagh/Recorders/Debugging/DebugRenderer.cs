using System;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.ShaderLibrary.Visual.Debug;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders.Debugging;

internal sealed class DebugRenderer : IPipelineRenderer, IDisposable
{
	private readonly FinalTargetHandles m_FinalTargetHandles = new FinalTargetHandles();

	public void Dispose()
	{
		m_FinalTargetHandles.Dispose();
	}

	public bool SupportsPipelineFeature(PipelineFeature feature)
	{
		return feature switch
		{
			PipelineFeature.GpuDriven => true, 
			PipelineFeature.MotionVectors => false, 
			_ => throw new ArgumentOutOfRangeException("feature", feature, null), 
		};
	}

	public bool AnySupportedDebugActive(DebugContext context)
	{
		if (GL.wireframe)
		{
			return true;
		}
		if (context.DebugData.RenderingDebug.OverdrawMode != 0)
		{
			return true;
		}
		return false;
	}

	public void SetupCullingParameters(ref ScriptableCullingParameters cullingParameters, WaaaghCameraData cameraData)
	{
		cullingParameters.cullingOptions = CullingOptions.None;
	}

	public void Setup(in RendererSetupContext context)
	{
	}

	public void Record(in RendererRecordContext context)
	{
		FinalTarget finalTarget = FrameResourcesFactory.CreateFinalTarget(context.RenderGraph, context.CameraData, m_FinalTargetHandles);
		if (IRS.ShouldRender(in context))
		{
			IRS.CullingPass(in context);
		}
		if (GpuDriven.IsGpuDrivenEnabled(in context))
		{
			GpuDriven.PrepareCulling(in context);
			GpuDriven.Cull(in context, finalTarget.Depth);
		}
		if (GL.wireframe)
		{
			Wireframe.Record(in context, finalTarget.Color, finalTarget.Depth);
		}
		else if (context.DebugContext.DebugData.RenderingDebug.OverdrawMode != 0)
		{
			switch (context.DebugContext.DebugData.RenderingDebug.OverdrawMode)
			{
			case DebugOverdrawMode.Overdraw:
				Overdraw.Record(in context, finalTarget.Color, finalTarget.Depth);
				break;
			case DebugOverdrawMode.QuadOverdraw:
				QuadOverdraw.Record(in context, finalTarget.Color, finalTarget.Depth);
				break;
			}
		}
	}

	public void Cleanup()
	{
	}

	void IPipelineRenderer.Setup(in RendererSetupContext context)
	{
		Setup(in context);
	}

	void IPipelineRenderer.Record(in RendererRecordContext context)
	{
		Record(in context);
	}
}
