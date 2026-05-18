using Owlcat.Runtime.Visual.Waaagh.Debugging;
using Owlcat.Runtime.Visual.Waaagh.Recorders;
using Owlcat.Runtime.Visual.XPBD;
using Owlcat.Runtime.Visual.XPBD.Debug;
using Owlcat.Runtime.Visual.XPBD.Shaders;
using Owlcat.Runtime.Visual.XPBD.Solvers;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.XPBDGizmos;

internal static class XPBDGizmosGBufferPass
{
	private sealed class PassData
	{
		public Solver Solver;

		public IGizmosImpl Gizmos;

		public Material GizmosMaterial;

		public int PassIndex;
	}

	public static void Record(in RecordContext context, in XPBDGizmosContext gizmosContext)
	{
		if (gizmosContext.XPBDDebug.DrawParticles != XPBDGizmosParticlesMode.Radius && gizmosContext.XPBDDebug.DrawParticles != XPBDGizmosParticlesMode.RadiusAndHandles)
		{
			return;
		}
		PassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<PassData>("XPBD Gizmos Particles", out passData, ProfilingSampler.Get(XPBDGizmosProfileId.XPBDGizmosParticles), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\RendererFeatures\\XPBDGizmos\\XPBDGizmosGBufferPass.cs", 28);
		passData.Solver = Owlcat.Runtime.Visual.XPBD.XPBD.Solver;
		passData.Gizmos = Owlcat.Runtime.Visual.XPBD.XPBD.Solver.SolverImpl.GizmosImpl;
		passData.GizmosMaterial = gizmosContext.GizmosMaterial;
		passData.PassIndex = gizmosContext.ParticlesRadiusPass;
		GBufferUtility.ConfigureDrawPass(unsafeRenderGraphBuilder, in context.FrameResources, context.IsVTEnabled);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData data, UnsafeGraphContext context)
		{
			ExecuteParticlesPass(CommandBufferHelpers.GetNativeCommandBuffer(context.cmd), data);
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
}
