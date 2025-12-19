using System.Collections.Generic;
using Owlcat.Runtime.Visual.Waaagh.Debugging;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using Owlcat.Runtime.Visual.XPBD;
using Owlcat.Runtime.Visual.XPBD.Authoring;
using Owlcat.Runtime.Visual.XPBD.Debug;
using Owlcat.Runtime.Visual.XPBD.Shaders;
using Owlcat.Runtime.Visual.XPBD.SoA;
using Owlcat.Runtime.Visual.XPBD.Solvers;
using Owlcat.Runtime.Visual.XPBD.Utilities;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.XPBDGizmos;

public class XPBDGizmosAfterRenderPass : ScriptableRenderPass
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

	private readonly int m_ParticlesPassIndex;

	private readonly int m_ConstraintsPassIndex;

	private readonly int m_VelocitiesPassIndex;

	private readonly int m_InertialForcesPassIndex;

	private readonly int m_PassAabb;

	private readonly int m_PassColliderContacts;

	private readonly int m_ContactNormalsPass;

	private readonly int m_PassNormals;

	private readonly int m_DrawDeformedVerticesPass;

	private readonly int m_DrawSimplexContactsPass;

	private static List<AuthoringBase> s_SelectedBodies = new List<AuthoringBase>();

	public override string Name => "XPBDGizmosAfterRenderPass";

	public XPBDGizmosAfterRenderPass(RenderPassEvent evt, WaaaghDebugData debugData, XPBDGizmosFeature feature)
		: base(evt)
	{
		m_DebugData = debugData;
		m_Feature = feature;
		m_ParticlesPassIndex = m_Feature.GizmosMaterial.FindPass("PARTICLES");
		m_ConstraintsPassIndex = m_Feature.GizmosMaterial.FindPass("CONSTRAINTS");
		m_VelocitiesPassIndex = m_Feature.GizmosMaterial.FindPass("VELOCITIES");
		m_InertialForcesPassIndex = m_Feature.GizmosMaterial.FindPass("INERTIAL FORCES");
		m_PassAabb = m_Feature.GizmosMaterial.FindPass("AABB");
		m_PassColliderContacts = m_Feature.GizmosMaterial.FindPass("COLLIDER CONTACTS");
		m_ContactNormalsPass = m_Feature.GizmosMaterial.FindPass("CONTACT NORMALS");
		m_PassNormals = m_Feature.GizmosMaterial.FindPass("NORMALS");
		m_DrawDeformedVerticesPass = m_Feature.GizmosMaterial.FindPass("DEFORMABLE VERTICES");
		m_DrawSimplexContactsPass = m_Feature.GizmosMaterial.FindPass("SIMPLEX CONTACTS");
	}

	public override void RecordRenderGraph(ContextContainer frameData)
	{
		RenderGraph renderGraph = frameData.Get<WaaaghRenderingData>().RenderGraph;
		WaaaghResourceData resources = frameData.Get<WaaaghResourceData>();
		Solver solver = Owlcat.Runtime.Visual.XPBD.XPBD.Solver;
		if (m_DebugData.XPBDDebug.DrawConstraints != 0)
		{
			RecordConstraintPass(renderGraph, resources);
		}
		if ((m_DebugData.XPBDDebug.DrawParticles == XPBDGizmosParticlesMode.Handles || m_DebugData.XPBDDebug.DrawParticles == XPBDGizmosParticlesMode.RadiusAndHandles) && !solver.BodyAllocator.IsEmpty)
		{
			RecordParticlesPass(renderGraph, resources);
		}
		if (m_DebugData.XPBDDebug.DrawVelocities && !solver.BodyAllocator.IsEmpty)
		{
			RecordVelocitiesPass(renderGraph, resources);
		}
		if (m_DebugData.XPBDDebug.DrawInertialForces && !solver.BodyAllocator.IsEmpty)
		{
			RecordInertialForcesPass(renderGraph, resources);
		}
		if (m_DebugData.XPBDDebug.DrawColliderAabb && !solver.ColliderWorld.IsEmpty)
		{
			RecordColliderAabbPass(renderGraph, resources);
		}
		if (m_DebugData.XPBDDebug.DrawSimplexAabb && !solver.ColliderWorld.IsEmpty)
		{
			RecordSimplexAabbPass(renderGraph, resources);
		}
		if (m_DebugData.XPBDDebug.DrawColliderContacts && !solver.BodyAllocator.IsEmpty)
		{
			RecordColliderContactsPass(renderGraph, resources);
		}
		if (m_DebugData.XPBDDebug.DrawContactNormals && !solver.BodyAllocator.IsEmpty)
		{
			RecordContactNormalsPass(renderGraph, resources);
		}
		if (m_DebugData.XPBDDebug.DrawVisibleBodyAabbs && !solver.BodyAllocator.IsEmpty)
		{
			RecordVisibleBodyAabbsPass(renderGraph, resources);
		}
		if (m_DebugData.XPBDDebug.DrawNormals && !solver.BodyAllocator.IsEmpty)
		{
			RecordDrawNormalsPass(renderGraph, resources);
		}
		if (m_DebugData.XPBDDebug.DrawRestNormals && !solver.BodyAllocator.IsEmpty)
		{
			RecordDrawRestNormalsPass(renderGraph, resources);
		}
		if (m_DebugData.XPBDDebug.DrawDeformedVertices && !solver.MeshDeformerAllocator.IsEmpty && solver.MeshDeformerAllocator.DeformableSkinnedVerticesSoA.Count > 0)
		{
			RecordDrawDeformedVerticesPass(renderGraph, resources);
		}
	}

	private void RecordConstraintPass(RenderGraph renderGraph, WaaaghResourceData resources)
	{
		PassData passData;
		using RenderGraphBuilder builder = renderGraph.AddRenderPass<PassData>("XPBD Gizmos Constraints", out passData, ProfilingSampler.Get(XPBDGizmosProfileId.XPBDGizmosConstraints), ".\\Library\\PackageCache\\com.owlcat.visual@2c5e70bf14b9\\Runtime\\Waaagh\\RendererFeatures\\XPBDGizmos\\Passes\\XPBDGizmosAfterRenderPass.cs", 166);
		passData.Solver = Owlcat.Runtime.Visual.XPBD.XPBD.Solver;
		passData.Gizmos = Owlcat.Runtime.Visual.XPBD.XPBD.Solver.SolverImpl.GizmosImpl;
		passData.GizmosMaterial = m_Feature.GizmosMaterial;
		passData.PassIndex = m_ConstraintsPassIndex;
		passData.DrawConstraints = m_DebugData.XPBDDebug.DrawConstraints;
		SetRenderTarget(resources, builder, m_DebugData.XPBDDebug.UseDepthBuffer);
		builder.SetRenderFunc(delegate(PassData data, RenderGraphContext context)
		{
			ExecuteConstraintsPass(context.cmd, data);
		});
	}

	private void SetRenderTarget(WaaaghResourceData resources, RenderGraphBuilder builder, bool useDepthBuffer)
	{
		TextureHandle input;
		if (useDepthBuffer)
		{
			input = resources.CameraDepthBuffer;
			builder.UseDepthBuffer(in input, DepthAccess.ReadWrite);
		}
		input = resources.CameraColorBuffer;
		builder.UseColorBuffer(in input, 0);
	}

	private static void ExecuteConstraintsPass(CommandBuffer cmd, PassData data)
	{
		switch (data.DrawConstraints)
		{
		case DrawConstraintType.Distance:
			DrawDistanceConstraints(cmd, data);
			break;
		case DrawConstraintType.Angular:
			DrawAngularConstraints(cmd, data);
			break;
		case DrawConstraintType.Bend:
			DrawBendConstraints(cmd, data);
			break;
		case DrawConstraintType.All:
			DrawBendConstraints(cmd, data);
			DrawDistanceConstraints(cmd, data);
			DrawAngularConstraints(cmd, data);
			break;
		}
	}

	private static void DrawDistanceConstraints(CommandBuffer cmd, PassData data)
	{
		GraphicsBufferWrapper distanceConstraintsMapBuffer = data.Gizmos.DistanceConstraintsMapBuffer;
		DrawConstraints(cmd, distanceConstraintsMapBuffer, Color.yellow, 1, 2, data.Gizmos, data.GizmosMaterial, data.PassIndex, data.Solver);
	}

	private static void DrawAngularConstraints(CommandBuffer cmd, PassData data)
	{
		GraphicsBufferWrapper angularConstraintsMapBuffer = data.Gizmos.AngularConstraintsMapBuffer;
		DrawConstraints(cmd, angularConstraintsMapBuffer, Color.green, 1, 2, data.Gizmos, data.GizmosMaterial, data.PassIndex, data.Solver);
	}

	private static void DrawBendConstraints(CommandBuffer cmd, PassData data)
	{
		GraphicsBufferWrapper bendConstraintsMapBuffer = data.Gizmos.BendConstraintsMapBuffer;
		DrawConstraints(cmd, bendConstraintsMapBuffer, Color.blue, 3, 3, data.Gizmos, data.GizmosMaterial, data.PassIndex, data.Solver);
	}

	private static void DrawConstraints(CommandBuffer cmd, GraphicsBufferWrapper constraintsMap, Color color, int linesCount, int particlesCount, IGizmosImpl gizmos, Material gizmosMaterial, int constraintsPassIndex, Solver solver)
	{
		if (constraintsMap != null && constraintsMap.Buffer != null && constraintsMap.Buffer.count > 0)
		{
			cmd.SetGlobalColor(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._Color, color);
			cmd.SetGlobalFloat(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._XpbdConstraintLinesCount, linesCount);
			cmd.SetGlobalFloat(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._XpbdConstraintParticlesCount, particlesCount);
			cmd.SetGlobalBuffer(gizmos.ParticlePositionBuffer.NameId, gizmos.ParticlePositionBuffer.Buffer);
			cmd.SetGlobalBuffer(constraintsMap.NameId, constraintsMap.Buffer);
			cmd.SetGlobalBuffer(gizmos.ConstraintsIndicesBuffer.NameId, gizmos.ConstraintsIndicesBuffer.Buffer);
			cmd.DrawProcedural(Matrix4x4.identity, gizmosMaterial, constraintsPassIndex, MeshTopology.Quads, 4, constraintsMap.Buffer.count * linesCount);
		}
	}

	private void RecordParticlesPass(RenderGraph renderGraph, WaaaghResourceData resources)
	{
		PassData passData;
		using RenderGraphBuilder builder = renderGraph.AddRenderPass<PassData>("XPBD Gizmos Particles", out passData, ProfilingSampler.Get(XPBDGizmosProfileId.XPBDGizmosParticles), ".\\Library\\PackageCache\\com.owlcat.visual@2c5e70bf14b9\\Runtime\\Waaagh\\RendererFeatures\\XPBDGizmos\\Passes\\XPBDGizmosAfterRenderPass.cs", 249);
		passData.Solver = Owlcat.Runtime.Visual.XPBD.XPBD.Solver;
		passData.Gizmos = Owlcat.Runtime.Visual.XPBD.XPBD.Solver.SolverImpl.GizmosImpl;
		passData.GizmosMaterial = m_Feature.GizmosMaterial;
		passData.PassIndex = m_ParticlesPassIndex;
		SetRenderTarget(resources, builder, m_DebugData.XPBDDebug.UseDepthBuffer);
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
		cmd.DrawProcedural(Matrix4x4.identity, data.GizmosMaterial, data.PassIndex, MeshTopology.Quads, 4, data.Solver.BodyAllocator.ParticleSoA.Count);
	}

	private void RecordVelocitiesPass(RenderGraph renderGraph, WaaaghResourceData resources)
	{
		PassData passData;
		using RenderGraphBuilder builder = renderGraph.AddRenderPass<PassData>("XPBD Gizmos Velocities", out passData, ProfilingSampler.Get(XPBDGizmosProfileId.XPBDGizmosVelocities), ".\\Library\\PackageCache\\com.owlcat.visual@2c5e70bf14b9\\Runtime\\Waaagh\\RendererFeatures\\XPBDGizmos\\Passes\\XPBDGizmosAfterRenderPass.cs", 278);
		passData.Solver = Owlcat.Runtime.Visual.XPBD.XPBD.Solver;
		passData.Gizmos = Owlcat.Runtime.Visual.XPBD.XPBD.Solver.SolverImpl.GizmosImpl;
		passData.GizmosMaterial = m_Feature.GizmosMaterial;
		passData.PassIndex = m_VelocitiesPassIndex;
		SetRenderTarget(resources, builder, m_DebugData.XPBDDebug.UseDepthBuffer);
		builder.SetRenderFunc(delegate(PassData data, RenderGraphContext context)
		{
			ExecuteVelocitiesPass(context.cmd, data);
		});
	}

	private static void ExecuteVelocitiesPass(CommandBuffer cmd, PassData data)
	{
		cmd.SetGlobalColor(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._Color, new Color(1f, 0f, 0f, 1f));
		cmd.SetGlobalBuffer(data.Gizmos.ParticlesMapBuffer.NameId, data.Gizmos.ParticlesMapBuffer.Buffer);
		cmd.SetGlobalBuffer(data.Gizmos.ParticlePositionBuffer.NameId, data.Gizmos.ParticlePositionBuffer.Buffer);
		cmd.SetGlobalBuffer(data.Gizmos.ParticleVelocityBuffer.NameId, data.Gizmos.ParticleVelocityBuffer.Buffer);
		cmd.DrawProcedural(Matrix4x4.identity, data.GizmosMaterial, data.PassIndex, MeshTopology.Quads, 4, data.Solver.BodyAllocator.ParticleSoA.Count);
	}

	private void RecordInertialForcesPass(RenderGraph renderGraph, WaaaghResourceData resources)
	{
		PassData passData;
		using RenderGraphBuilder builder = renderGraph.AddRenderPass<PassData>("XPBD Gizmos Inertial Forces", out passData, ProfilingSampler.Get(XPBDGizmosProfileId.XPBDGizmosInertialForces), ".\\Library\\PackageCache\\com.owlcat.visual@2c5e70bf14b9\\Runtime\\Waaagh\\RendererFeatures\\XPBDGizmos\\Passes\\XPBDGizmosAfterRenderPass.cs", 306);
		passData.Solver = Owlcat.Runtime.Visual.XPBD.XPBD.Solver;
		passData.Gizmos = Owlcat.Runtime.Visual.XPBD.XPBD.Solver.SolverImpl.GizmosImpl;
		passData.GizmosMaterial = m_Feature.GizmosMaterial;
		passData.PassIndex = m_InertialForcesPassIndex;
		SetRenderTarget(resources, builder, m_DebugData.XPBDDebug.UseDepthBuffer);
		builder.SetRenderFunc(delegate(PassData data, RenderGraphContext context)
		{
			ExecuteInertialForcesPass(context.cmd, data);
		});
	}

	private static void ExecuteInertialForcesPass(CommandBuffer cmd, PassData data)
	{
	}

	private void RecordColliderAabbPass(RenderGraph renderGraph, WaaaghResourceData resources)
	{
		PassData passData;
		using RenderGraphBuilder builder = renderGraph.AddRenderPass<PassData>("XPBD Gizmos Collider Aabb", out passData, ProfilingSampler.Get(XPBDGizmosProfileId.XPBDGizmosColliderAabb), ".\\Library\\PackageCache\\com.owlcat.visual@2c5e70bf14b9\\Runtime\\Waaagh\\RendererFeatures\\XPBDGizmos\\Passes\\XPBDGizmosAfterRenderPass.cs", 361);
		passData.Solver = Owlcat.Runtime.Visual.XPBD.XPBD.Solver;
		passData.Gizmos = Owlcat.Runtime.Visual.XPBD.XPBD.Solver.SolverImpl.GizmosImpl;
		passData.GizmosMaterial = m_Feature.GizmosMaterial;
		passData.PassIndex = m_PassAabb;
		SetRenderTarget(resources, builder, m_DebugData.XPBDDebug.UseDepthBuffer);
		builder.SetRenderFunc(delegate(PassData data, RenderGraphContext context)
		{
			ExecuteColliderAabbPass(context.cmd, data);
		});
	}

	private static void ExecuteColliderAabbPass(CommandBuffer cmd, PassData data)
	{
		cmd.SetGlobalBuffer(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._XpbdDebugAabbBuffer, data.Solver.SolverImpl.GizmosImpl.ColliderAabbBuffer);
		cmd.SetGlobalBuffer(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._XpbdDebugAabbMap, data.Solver.SolverImpl.GizmosImpl.CollidersMapBuffer);
		cmd.SetGlobalFloat(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._XpbdDebugConstructAabbFromConstraintParameters, 0f);
		cmd.SetGlobalColor(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._Color, Color.cyan);
		cmd.DrawMeshInstancedProcedural(XPBDMeshUtils.CubeMeshWithUvAndNormals, 0, data.GizmosMaterial, data.PassIndex, data.Solver.ColliderWorld.EntityAllocationMap.Count);
	}

	private void RecordSimplexAabbPass(RenderGraph renderGraph, WaaaghResourceData resources)
	{
		PassData passData;
		using RenderGraphBuilder builder = renderGraph.AddRenderPass<PassData>("XPBD Gizmos Simplex Aabb", out passData, ProfilingSampler.Get(XPBDGizmosProfileId.XPBDGizmosSimplexAabb), ".\\Library\\PackageCache\\com.owlcat.visual@2c5e70bf14b9\\Runtime\\Waaagh\\RendererFeatures\\XPBDGizmos\\Passes\\XPBDGizmosAfterRenderPass.cs", 389);
		passData.Solver = Owlcat.Runtime.Visual.XPBD.XPBD.Solver;
		passData.Gizmos = Owlcat.Runtime.Visual.XPBD.XPBD.Solver.SolverImpl.GizmosImpl;
		passData.GizmosMaterial = m_Feature.GizmosMaterial;
		passData.PassIndex = m_PassAabb;
		SetRenderTarget(resources, builder, m_DebugData.XPBDDebug.UseDepthBuffer);
		builder.SetRenderFunc(delegate(PassData data, RenderGraphContext context)
		{
			ExecuteSimplexAabbPass(context.cmd, data);
		});
	}

	private static void ExecuteSimplexAabbPass(CommandBuffer cmd, PassData data)
	{
		cmd.SetGlobalBuffer(data.Gizmos.ConstraintsParameters0Buffer.NameId, data.Gizmos.ConstraintsParameters0Buffer.Buffer);
		cmd.SetGlobalBuffer(data.Gizmos.ConstraintsParameters1Buffer.NameId, data.Gizmos.ConstraintsParameters1Buffer.Buffer);
		cmd.SetGlobalBuffer(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._XpbdDebugAabbMap, data.Gizmos.SimplexMapBuffer);
		cmd.SetGlobalFloat(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._XpbdDebugConstructAabbFromConstraintParameters, 1f);
		cmd.SetGlobalColor(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._Color, new Color32(145, 66, 26, byte.MaxValue));
		cmd.DrawMeshInstancedProcedural(XPBDMeshUtils.CubeMeshWithUvAndNormals, 0, data.GizmosMaterial, data.PassIndex, data.Solver.SolverImpl.GizmosImpl.SimplexMapBuffer.Buffer.count);
	}

	private void RecordColliderContactsPass(RenderGraph renderGraph, WaaaghResourceData resources)
	{
		PassData passData;
		using RenderGraphBuilder builder = renderGraph.AddRenderPass<PassData>("XPBD Gizmos Collider Contacts", out passData, ProfilingSampler.Get(XPBDGizmosProfileId.XPBDGizmosColliderContacts), ".\\Library\\PackageCache\\com.owlcat.visual@2c5e70bf14b9\\Runtime\\Waaagh\\RendererFeatures\\XPBDGizmos\\Passes\\XPBDGizmosAfterRenderPass.cs", 418);
		passData.Solver = Owlcat.Runtime.Visual.XPBD.XPBD.Solver;
		passData.Gizmos = Owlcat.Runtime.Visual.XPBD.XPBD.Solver.SolverImpl.GizmosImpl;
		passData.GizmosMaterial = m_Feature.GizmosMaterial;
		passData.PassIndex = m_PassColliderContacts;
		SetRenderTarget(resources, builder, m_DebugData.XPBDDebug.UseDepthBuffer);
		builder.SetRenderFunc(delegate(PassData data, RenderGraphContext context)
		{
			ExecuteColliderContactsPass(context.cmd, data);
		});
	}

	private static void ExecuteColliderContactsPass(CommandBuffer cmd, PassData data)
	{
		data.Gizmos.ContactsBuffer.SetGlobal(cmd);
		cmd.SetGlobalBuffer(data.Gizmos.ConstraintsParameters0Buffer.NameId, data.Gizmos.ConstraintsParameters0Buffer.Buffer);
		cmd.SetGlobalBuffer(data.Gizmos.ConstraintsParameters1Buffer.NameId, data.Gizmos.ConstraintsParameters1Buffer.Buffer);
		cmd.SetGlobalBuffer(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._XpbdDebugAabbBuffer, data.Solver.SolverImpl.GizmosImpl.ColliderAabbBuffer);
		cmd.SetGlobalColor(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._Color, new Color32(byte.MaxValue, 0, byte.MaxValue, byte.MaxValue));
		cmd.DrawMeshInstancedIndirect(XPBDMeshUtils.CubeMeshWithUvAndNormals, 0, data.GizmosMaterial, data.PassIndex, data.Gizmos.ColliderContactsIndirectArgsBuffer.Buffer, 0);
	}

	private void RecordContactNormalsPass(RenderGraph renderGraph, WaaaghResourceData resources)
	{
		PassData passData;
		using RenderGraphBuilder builder = renderGraph.AddRenderPass<PassData>("XPBD Gizmos Contact Normals", out passData, ProfilingSampler.Get(XPBDGizmosProfileId.XPBDGizmosContactNormals), ".\\Library\\PackageCache\\com.owlcat.visual@2c5e70bf14b9\\Runtime\\Waaagh\\RendererFeatures\\XPBDGizmos\\Passes\\XPBDGizmosAfterRenderPass.cs", 447);
		passData.Solver = Owlcat.Runtime.Visual.XPBD.XPBD.Solver;
		passData.Gizmos = Owlcat.Runtime.Visual.XPBD.XPBD.Solver.SolverImpl.GizmosImpl;
		passData.GizmosMaterial = m_Feature.GizmosMaterial;
		passData.PassIndex = m_ContactNormalsPass;
		SetRenderTarget(resources, builder, m_DebugData.XPBDDebug.UseDepthBuffer);
		builder.SetRenderFunc(delegate(PassData data, RenderGraphContext context)
		{
			ExecuteContactNormalsPass(context.cmd, data);
		});
	}

	private static void ExecuteContactNormalsPass(CommandBuffer cmd, PassData data)
	{
		cmd.SetGlobalColor(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._Color, new Color32(byte.MaxValue, 0, byte.MaxValue, byte.MaxValue));
		cmd.SetGlobalFloat(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._XpbdParticlesCount, data.Solver.BodyAllocator.ParticleSoA.Count);
		data.Gizmos.ContactsBuffer.SetGlobal(cmd);
		data.Gizmos.ParticlePositionBuffer.SetGlobal(cmd);
		cmd.DrawMeshInstancedIndirect(RenderingUtils.QuadMesh, 0, data.GizmosMaterial, data.PassIndex, data.Gizmos.SimplexContactsIndirectArgsBuffer.Buffer, 0);
	}

	private void RecordVisibleBodyAabbsPass(RenderGraph renderGraph, WaaaghResourceData resources)
	{
		PassData passData;
		using RenderGraphBuilder builder = renderGraph.AddRenderPass<PassData>("XPBD Gizmos Visible Body Aabbs", out passData, ProfilingSampler.Get(XPBDGizmosProfileId.XPBDGizmosVisibleBodyAabbs), ".\\Library\\PackageCache\\com.owlcat.visual@2c5e70bf14b9\\Runtime\\Waaagh\\RendererFeatures\\XPBDGizmos\\Passes\\XPBDGizmosAfterRenderPass.cs", 476);
		passData.Solver = Owlcat.Runtime.Visual.XPBD.XPBD.Solver;
		passData.Gizmos = Owlcat.Runtime.Visual.XPBD.XPBD.Solver.SolverImpl.GizmosImpl;
		passData.GizmosMaterial = m_Feature.GizmosMaterial;
		passData.PassIndex = m_PassAabb;
		SetRenderTarget(resources, builder, m_DebugData.XPBDDebug.UseDepthBuffer);
		builder.SetRenderFunc(delegate(PassData data, RenderGraphContext context)
		{
			ExecuteVisibleBodyAabbsPass(context.cmd, data);
		});
	}

	private static void ExecuteVisibleBodyAabbsPass(CommandBuffer cmd, PassData data)
	{
		cmd.SetGlobalBuffer(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._XpbdDebugAabbBuffer, data.Gizmos.BodyAabbBuffer);
		cmd.SetGlobalBuffer(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._XpbdDebugAabbMap, data.Gizmos.BodiesMapBuffer);
		cmd.SetGlobalBuffer(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._XpbdDebugAabbVisibilityBuffer, data.Gizmos.BodyVisibilityBuffer);
		cmd.SetGlobalFloat(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._XpbdDebugConstructAabbFromConstraintParameters, 0f);
		cmd.SetGlobalColor(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._Color, Color.green);
		cmd.SetGlobalColor(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._ColorSecondary, Color.red);
		cmd.SetGlobalFloat(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._XpbdDebugUseVisibilityBuffer, 1f);
		cmd.DrawMeshInstancedProcedural(XPBDMeshUtils.CubeMeshWithUvAndNormals, 0, data.GizmosMaterial, data.PassIndex, data.Solver.BodyAllocator.EntityAllocationMap.Count);
		cmd.SetGlobalFloat(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._XpbdDebugUseVisibilityBuffer, 0f);
	}

	private void RecordDrawNormalsPass(RenderGraph renderGraph, WaaaghResourceData resources)
	{
		PassData passData;
		using RenderGraphBuilder builder = renderGraph.AddRenderPass<PassData>("XPBD Gizmos Normals", out passData, ProfilingSampler.Get(XPBDGizmosProfileId.XPBDGizmosDrawNormals), ".\\Library\\PackageCache\\com.owlcat.visual@2c5e70bf14b9\\Runtime\\Waaagh\\RendererFeatures\\XPBDGizmos\\Passes\\XPBDGizmosAfterRenderPass.cs", 509);
		passData.Solver = Owlcat.Runtime.Visual.XPBD.XPBD.Solver;
		passData.Gizmos = Owlcat.Runtime.Visual.XPBD.XPBD.Solver.SolverImpl.GizmosImpl;
		passData.GizmosMaterial = m_Feature.GizmosMaterial;
		passData.PassIndex = m_PassNormals;
		SetRenderTarget(resources, builder, m_DebugData.XPBDDebug.UseDepthBuffer);
		builder.SetRenderFunc(delegate(PassData data, RenderGraphContext context)
		{
			ExecuteDrawNormalsPass(context.cmd, data);
		});
	}

	private static void ExecuteDrawNormalsPass(CommandBuffer cmd, PassData data)
	{
		cmd.SetGlobalColor(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._Color, new Color(0f, 1f, 0f, 1f));
		data.Gizmos.MeshVerticesBuffer.SetGlobal(cmd);
		cmd.SetGlobalFloat(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._XpbdDebugDrawRestNormals, 0f);
		cmd.DrawProcedural(Matrix4x4.identity, data.GizmosMaterial, data.PassIndex, MeshTopology.Quads, 4, data.Solver.BodyAllocator.VerticesSoA.Count);
	}

	private void RecordDrawRestNormalsPass(RenderGraph renderGraph, WaaaghResourceData resources)
	{
		PassData passData;
		using RenderGraphBuilder builder = renderGraph.AddRenderPass<PassData>("XPBD Gizmos Rest Normals", out passData, ProfilingSampler.Get(XPBDGizmosProfileId.XPBDGizmosDrawRestNormals), ".\\Library\\PackageCache\\com.owlcat.visual@2c5e70bf14b9\\Runtime\\Waaagh\\RendererFeatures\\XPBDGizmos\\Passes\\XPBDGizmosAfterRenderPass.cs", 536);
		passData.Solver = Owlcat.Runtime.Visual.XPBD.XPBD.Solver;
		passData.Gizmos = Owlcat.Runtime.Visual.XPBD.XPBD.Solver.SolverImpl.GizmosImpl;
		passData.GizmosMaterial = m_Feature.GizmosMaterial;
		passData.PassIndex = m_PassNormals;
		SetRenderTarget(resources, builder, m_DebugData.XPBDDebug.UseDepthBuffer);
		builder.SetRenderFunc(delegate(PassData data, RenderGraphContext context)
		{
			ExecuteDrawRestNormalsPass(context.cmd, data);
		});
	}

	private static void ExecuteDrawRestNormalsPass(CommandBuffer cmd, PassData data)
	{
		cmd.SetGlobalColor(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._Color, new Color(0f, 0f, 1f, 1f));
		cmd.SetGlobalFloat(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._XpbdDebugDrawRestNormals, 1f);
		data.Gizmos.MeshVerticesBuffer.SetGlobal(cmd);
		cmd.DrawProcedural(Matrix4x4.identity, data.GizmosMaterial, data.PassIndex, MeshTopology.Quads, 4, data.Solver.BodyAllocator.VerticesSoA.Count);
	}

	private void RecordDrawDeformedVerticesPass(RenderGraph renderGraph, WaaaghResourceData resources)
	{
		PassData passData;
		using RenderGraphBuilder builder = renderGraph.AddRenderPass<PassData>("XPBD Gizmos Deformed Vertices", out passData, ProfilingSampler.Get(XPBDGizmosProfileId.XPBDGizmosDrawDeformedVertices), ".\\Library\\PackageCache\\com.owlcat.visual@2c5e70bf14b9\\Runtime\\Waaagh\\RendererFeatures\\XPBDGizmos\\Passes\\XPBDGizmosAfterRenderPass.cs", 563);
		passData.Solver = Owlcat.Runtime.Visual.XPBD.XPBD.Solver;
		passData.Gizmos = Owlcat.Runtime.Visual.XPBD.XPBD.Solver.SolverImpl.GizmosImpl;
		passData.GizmosMaterial = m_Feature.GizmosMaterial;
		passData.PassIndex = m_DrawDeformedVerticesPass;
		SetRenderTarget(resources, builder, m_DebugData.XPBDDebug.UseDepthBuffer);
		builder.SetRenderFunc(delegate(PassData data, RenderGraphContext context)
		{
			ExecuteDrawDeformedVerticesPass(context.cmd, data);
		});
	}

	private static void ExecuteDrawDeformedVerticesPass(CommandBuffer cmd, PassData data)
	{
		data.Gizmos.DeformableGizmosVerticesBuffer.SetGlobal(cmd);
		cmd.DrawProcedural(Matrix4x4.identity, data.GizmosMaterial, data.PassIndex, MeshTopology.Quads, 4, data.Solver.MeshDeformerAllocator.DeformableSkinnedVerticesSoA.Count * 3);
	}

	private void RecordDrawSimplexContactsPass(RenderGraph renderGraph, WaaaghResourceData resources)
	{
		PassData passData;
		using RenderGraphBuilder builder = renderGraph.AddRenderPass<PassData>("XPBD Gizmos Simplex Contacts", out passData, ProfilingSampler.Get(XPBDGizmosProfileId.XPBDGizmosDrawSimplexContacts), ".\\Library\\PackageCache\\com.owlcat.visual@2c5e70bf14b9\\Runtime\\Waaagh\\RendererFeatures\\XPBDGizmos\\Passes\\XPBDGizmosAfterRenderPass.cs", 588);
		passData.Solver = Owlcat.Runtime.Visual.XPBD.XPBD.Solver;
		passData.Gizmos = Owlcat.Runtime.Visual.XPBD.XPBD.Solver.SolverImpl.GizmosImpl;
		passData.GizmosMaterial = m_Feature.GizmosMaterial;
		passData.PassIndex = m_DrawSimplexContactsPass;
		SetRenderTarget(resources, builder, m_DebugData.XPBDDebug.UseDepthBuffer);
		builder.SetRenderFunc(delegate(PassData data, RenderGraphContext context)
		{
			ExecuteDrawSimplexContactsPass(context.cmd, data);
		});
	}

	private void ExecuteDrawSimplexContactsPass(CommandBuffer cmd, PassData data)
	{
		data.Gizmos.ContactsBuffer.SetGlobal(cmd);
		data.Gizmos.ParticlePositionBuffer.SetGlobal(cmd);
		data.Gizmos.ContactsBuffer.SetGlobal(cmd);
		cmd.DrawMeshInstancedIndirect(RenderingUtils.QuadMesh, 0, data.GizmosMaterial, data.PassIndex, data.Gizmos.SimplexContactsIndirectArgsBuffer.Buffer, 0);
	}
}
