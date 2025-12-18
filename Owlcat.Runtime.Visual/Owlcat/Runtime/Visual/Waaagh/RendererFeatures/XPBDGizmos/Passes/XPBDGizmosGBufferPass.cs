using Owlcat.Runtime.Visual.Waaagh.Debugging;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using Owlcat.Runtime.Visual.XPBD;
using Owlcat.Runtime.Visual.XPBD.Debug;
using Owlcat.Runtime.Visual.XPBD.Shaders;
using Owlcat.Runtime.Visual.XPBD.Solvers;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.XPBDGizmos.Passes;

public class XPBDGizmosGBufferPass : ScriptableRenderPass
{
	private class PassData
	{
		public Solver Solver;

		public IGizmosImpl Gizmos;

		public Material GizmosMaterial;

		public int PassIndex;

		public DrawConstraintType DrawConstraints;
	}

	private WaaaghDebugData m_DebugData;

	private XPBDGizmosFeature m_Feature;

	private int m_ParticlesPassIndex;

	public override string Name => "XPBDGizmosGBufferPass";

	public XPBDGizmosGBufferPass(RenderPassEvent evt, WaaaghDebugData debugData, XPBDGizmosFeature feature)
		: base(evt)
	{
		m_DebugData = debugData;
		m_Feature = feature;
		m_ParticlesPassIndex = m_Feature.GizmosMaterial.FindPass("PARTICLES RADIUS");
	}

	public override void RecordRenderGraph(ContextContainer frameData)
	{
		RenderGraph renderGraph = frameData.Get<WaaaghRenderingData>().RenderGraph;
		WaaaghResourceData resources = frameData.Get<WaaaghResourceData>();
		if (m_DebugData.XPBDDebug.DrawParticles == XPBDGizmosParticlesMode.Radius || m_DebugData.XPBDDebug.DrawParticles == XPBDGizmosParticlesMode.RadiusAndHandles)
		{
			RecordParticlesPass(renderGraph, resources);
		}
	}

	private void RecordParticlesPass(RenderGraph renderGraph, WaaaghResourceData resources)
	{
		PassData passData;
		using RenderGraphBuilder builder = renderGraph.AddRenderPass<PassData>("XPBD Gizmos Particles", out passData, ProfilingSampler.Get(XPBDGizmosProfileId.XPBDGizmosParticles), ".\\Library\\PackageCache\\com.owlcat.visual@01c4fcbc474f\\Runtime\\Waaagh\\RendererFeatures\\XPBDGizmos\\Passes\\XPBDGizmosGBufferPass.cs", 57);
		passData.Solver = Owlcat.Runtime.Visual.XPBD.XPBD.Solver;
		passData.Gizmos = Owlcat.Runtime.Visual.XPBD.XPBD.Solver.SolverImpl.GizmosImpl;
		passData.GizmosMaterial = m_Feature.GizmosMaterial;
		passData.PassIndex = m_ParticlesPassIndex;
		SetGBuffer(resources, builder);
		builder.SetRenderFunc(delegate(PassData data, RenderGraphContext context)
		{
			ExecuteParticlesPass(context.cmd, data);
		});
	}

	private static void ExecuteParticlesPass(CommandBuffer cmd, PassData data)
	{
		cmd.SetGlobalFloat(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._ParticleHandleSize, 0.015f);
		cmd.SetGlobalColor(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._Color, new Color(1f, 0f, 0f, 1f));
		data.Gizmos.ParticlesMapBuffer?.SetGlobal(cmd);
		data.Gizmos.ParticlePositionBuffer?.SetGlobal(cmd);
		data.Gizmos.ParticleRadiusBuffer?.SetGlobal(cmd);
		cmd.DrawMeshInstancedProcedural(RenderingUtils.SphereMesh, 0, data.GizmosMaterial, data.PassIndex, data.Solver.BodyAllocator.ParticleSoA.Count);
	}

	private static void SetGBuffer(WaaaghResourceData resources, RenderGraphBuilder builder)
	{
		TextureHandle input = resources.CameraDepthBuffer;
		builder.UseDepthBuffer(in input, DepthAccess.ReadWrite);
		builder.UseColorBuffer(in resources.CameraAlbedoRT, 0);
		builder.UseColorBuffer(in resources.CameraSpecularRT, 1);
		builder.UseColorBuffer(in resources.CameraNormalsRT, 2);
		input = resources.CameraColorBuffer;
		builder.UseColorBuffer(in input, 3);
		builder.UseColorBuffer(in resources.CameraTranslucencyRT, 4);
	}
}
