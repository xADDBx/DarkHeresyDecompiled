using System.Collections.Generic;
using Owlcat.Runtime.Visual.Waaagh.Debugging;
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

internal static class XPBDGizmosAfterRenderPass
{
	private class PassData
	{
		public Solver Solver;

		public IGizmosImpl Gizmos;

		public Material GizmosMaterial;

		public int PassIndex;

		public DrawConstraintType DrawConstraints;
	}

	private static List<AuthoringBase> s_SelectedBodies = new List<AuthoringBase>();

	public static void Record(in RecordContext context, in XPBDGizmosContext gizmosContext)
	{
		Solver solver = Owlcat.Runtime.Visual.XPBD.XPBD.Solver;
		XPBDDebug xPBDDebug = gizmosContext.XPBDDebug;
		if (xPBDDebug.DrawConstraints != 0)
		{
			RecordConstraintPass(in context, in gizmosContext);
		}
		if ((xPBDDebug.DrawParticles == XPBDGizmosParticlesMode.Handles || xPBDDebug.DrawParticles == XPBDGizmosParticlesMode.RadiusAndHandles) && !solver.BodyAllocator.IsEmpty)
		{
			RecordParticlesPass(in context, in gizmosContext);
		}
		if (xPBDDebug.DrawVelocities && !solver.BodyAllocator.IsEmpty)
		{
			RecordVelocitiesPass(in context, in gizmosContext);
		}
		if (xPBDDebug.DrawInertialForces && !solver.BodyAllocator.IsEmpty)
		{
			RecordInertialForcesPass(in context, in gizmosContext);
		}
		if (xPBDDebug.DrawColliderAabb && !solver.ColliderWorld.IsEmpty)
		{
			RecordColliderAabbPass(in context, in gizmosContext);
		}
		if (xPBDDebug.DrawSimplexAabb && !solver.ColliderWorld.IsEmpty)
		{
			RecordSimplexAabbPass(in context, in gizmosContext);
		}
		if (xPBDDebug.DrawColliderContacts && !solver.BodyAllocator.IsEmpty)
		{
			RecordColliderContactsPass(in context, in gizmosContext);
		}
		if (xPBDDebug.DrawContactNormals && !solver.BodyAllocator.IsEmpty)
		{
			RecordContactNormalsPass(in context, in gizmosContext);
		}
		if (xPBDDebug.DrawVisibleBodyAabbs && !solver.BodyAllocator.IsEmpty)
		{
			RecordVisibleBodyAabbsPass(in context, in gizmosContext);
		}
		if (xPBDDebug.DrawNormals && !solver.BodyAllocator.IsEmpty)
		{
			RecordDrawNormalsPass(in context, in gizmosContext);
		}
		if (xPBDDebug.DrawRestNormals && !solver.BodyAllocator.IsEmpty)
		{
			RecordDrawRestNormalsPass(in context, in gizmosContext);
		}
		if (xPBDDebug.DrawDeformedVertices && !solver.MeshDeformerAllocator.IsEmpty && solver.MeshDeformerAllocator.DeformableSkinnedVerticesSoA.Count > 0)
		{
			RecordDrawDeformedVerticesPass(in context, in gizmosContext);
		}
	}

	private static void RecordConstraintPass(in RecordContext context, in XPBDGizmosContext gizmosContext)
	{
		PassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<PassData>("XPBD Gizmos Constraints", out passData, ProfilingSampler.Get(XPBDGizmosProfileId.XPBDGizmosConstraints), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\RendererFeatures\\XPBDGizmos\\XPBDGizmosAfterRenderPass.cs", 128);
		passData.Solver = Owlcat.Runtime.Visual.XPBD.XPBD.Solver;
		passData.Gizmos = Owlcat.Runtime.Visual.XPBD.XPBD.Solver.SolverImpl.GizmosImpl;
		passData.GizmosMaterial = gizmosContext.GizmosMaterial;
		passData.PassIndex = gizmosContext.ConstraintsPass;
		passData.DrawConstraints = gizmosContext.XPBDDebug.DrawConstraints;
		SetRenderTarget(in context, unsafeRenderGraphBuilder, gizmosContext.XPBDDebug.UseDepthBuffer);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData data, UnsafeGraphContext context)
		{
			ExecuteConstraintsPass(context.cmd, data);
		});
	}

	private static void ExecuteConstraintsPass(UnsafeCommandBuffer cmd, PassData data)
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

	private static void DrawDistanceConstraints(UnsafeCommandBuffer cmd, PassData data)
	{
		GraphicsBufferWrapper distanceConstraintsMapBuffer = data.Gizmos.DistanceConstraintsMapBuffer;
		DrawConstraints(cmd, distanceConstraintsMapBuffer, Color.yellow, 1, 2, data.Gizmos, data.GizmosMaterial, data.PassIndex, data.Solver);
	}

	private static void DrawAngularConstraints(UnsafeCommandBuffer cmd, PassData data)
	{
		GraphicsBufferWrapper angularConstraintsMapBuffer = data.Gizmos.AngularConstraintsMapBuffer;
		DrawConstraints(cmd, angularConstraintsMapBuffer, Color.green, 1, 2, data.Gizmos, data.GizmosMaterial, data.PassIndex, data.Solver);
	}

	private static void DrawBendConstraints(UnsafeCommandBuffer cmd, PassData data)
	{
		GraphicsBufferWrapper bendConstraintsMapBuffer = data.Gizmos.BendConstraintsMapBuffer;
		DrawConstraints(cmd, bendConstraintsMapBuffer, Color.blue, 3, 3, data.Gizmos, data.GizmosMaterial, data.PassIndex, data.Solver);
	}

	private static void DrawConstraints(UnsafeCommandBuffer cmd, GraphicsBufferWrapper constraintsMap, Color color, int linesCount, int particlesCount, IGizmosImpl gizmos, Material gizmosMaterial, int constraintsPassIndex, Solver solver)
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

	private static void RecordParticlesPass(in RecordContext context, in XPBDGizmosContext gizmosContext)
	{
		PassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<PassData>("XPBD Gizmos Particles", out passData, ProfilingSampler.Get(XPBDGizmosProfileId.XPBDGizmosParticles), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\RendererFeatures\\XPBDGizmos\\XPBDGizmosAfterRenderPass.cs", 201);
		passData.Solver = Owlcat.Runtime.Visual.XPBD.XPBD.Solver;
		passData.Gizmos = Owlcat.Runtime.Visual.XPBD.XPBD.Solver.SolverImpl.GizmosImpl;
		passData.GizmosMaterial = gizmosContext.GizmosMaterial;
		passData.PassIndex = gizmosContext.ParticlesPass;
		SetRenderTarget(in context, unsafeRenderGraphBuilder, gizmosContext.XPBDDebug.UseDepthBuffer);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData data, UnsafeGraphContext context)
		{
			ExecuteParticlesPass(context.cmd, data);
		});
	}

	private static void ExecuteParticlesPass(UnsafeCommandBuffer cmd, PassData data)
	{
		cmd.SetGlobalFloat(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._ParticleHandleSize, 0.015f);
		cmd.SetGlobalColor(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._Color, new Color(1f, 0f, 0f, 1f));
		CommandBuffer nativeCommandBuffer = CommandBufferHelpers.GetNativeCommandBuffer(cmd);
		data.Gizmos.ParticlesMapBuffer?.SetGlobal(nativeCommandBuffer);
		data.Gizmos.ParticlePositionBuffer?.SetGlobal(nativeCommandBuffer);
		data.Gizmos.ParticleRadiusBuffer?.SetGlobal(nativeCommandBuffer);
		cmd.DrawProcedural(Matrix4x4.identity, data.GizmosMaterial, data.PassIndex, MeshTopology.Quads, 4, data.Solver.BodyAllocator.ParticleSoA.Count);
	}

	private static void RecordVelocitiesPass(in RecordContext context, in XPBDGizmosContext gizmosContext)
	{
		PassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<PassData>("XPBD Gizmos Velocities", out passData, ProfilingSampler.Get(XPBDGizmosProfileId.XPBDGizmosVelocities), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\RendererFeatures\\XPBDGizmos\\XPBDGizmosAfterRenderPass.cs", 231);
		passData.Solver = Owlcat.Runtime.Visual.XPBD.XPBD.Solver;
		passData.Gizmos = Owlcat.Runtime.Visual.XPBD.XPBD.Solver.SolverImpl.GizmosImpl;
		passData.GizmosMaterial = gizmosContext.GizmosMaterial;
		passData.PassIndex = gizmosContext.VelocitiesPass;
		SetRenderTarget(in context, unsafeRenderGraphBuilder, gizmosContext.XPBDDebug.UseDepthBuffer);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData data, UnsafeGraphContext context)
		{
			ExecuteVelocitiesPass(context.cmd, data);
		});
	}

	private static void ExecuteVelocitiesPass(UnsafeCommandBuffer cmd, PassData data)
	{
		cmd.SetGlobalColor(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._Color, new Color(1f, 0f, 0f, 1f));
		cmd.SetGlobalBuffer(data.Gizmos.ParticlesMapBuffer.NameId, data.Gizmos.ParticlesMapBuffer.Buffer);
		cmd.SetGlobalBuffer(data.Gizmos.ParticlePositionBuffer.NameId, data.Gizmos.ParticlePositionBuffer.Buffer);
		cmd.SetGlobalBuffer(data.Gizmos.ParticleVelocityBuffer.NameId, data.Gizmos.ParticleVelocityBuffer.Buffer);
		cmd.DrawProcedural(Matrix4x4.identity, data.GizmosMaterial, data.PassIndex, MeshTopology.Quads, 4, data.Solver.BodyAllocator.ParticleSoA.Count);
	}

	private static void RecordInertialForcesPass(in RecordContext context, in XPBDGizmosContext gizmosContext)
	{
		PassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<PassData>("XPBD Gizmos Inertial Forces", out passData, ProfilingSampler.Get(XPBDGizmosProfileId.XPBDGizmosInertialForces), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\RendererFeatures\\XPBDGizmos\\XPBDGizmosAfterRenderPass.cs", 259);
		passData.Solver = Owlcat.Runtime.Visual.XPBD.XPBD.Solver;
		passData.Gizmos = Owlcat.Runtime.Visual.XPBD.XPBD.Solver.SolverImpl.GizmosImpl;
		passData.GizmosMaterial = gizmosContext.GizmosMaterial;
		passData.PassIndex = gizmosContext.InertialForcesPass;
		SetRenderTarget(in context, unsafeRenderGraphBuilder, gizmosContext.XPBDDebug.UseDepthBuffer);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData data, UnsafeGraphContext context)
		{
			ExecuteInertialForcesPass(context.cmd, data);
		});
	}

	private static void ExecuteInertialForcesPass(UnsafeCommandBuffer cmd, PassData data)
	{
	}

	private static void RecordColliderAabbPass(in RecordContext context, in XPBDGizmosContext gizmosContext)
	{
		PassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<PassData>("XPBD Gizmos Collider Aabb", out passData, ProfilingSampler.Get(XPBDGizmosProfileId.XPBDGizmosColliderAabb), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\RendererFeatures\\XPBDGizmos\\XPBDGizmosAfterRenderPass.cs", 314);
		passData.Solver = Owlcat.Runtime.Visual.XPBD.XPBD.Solver;
		passData.Gizmos = Owlcat.Runtime.Visual.XPBD.XPBD.Solver.SolverImpl.GizmosImpl;
		passData.GizmosMaterial = gizmosContext.GizmosMaterial;
		passData.PassIndex = gizmosContext.AabbPass;
		SetRenderTarget(in context, unsafeRenderGraphBuilder, gizmosContext.XPBDDebug.UseDepthBuffer);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData data, UnsafeGraphContext context)
		{
			ExecuteColliderAabbPass(context.cmd, data);
		});
	}

	private static void ExecuteColliderAabbPass(UnsafeCommandBuffer cmd, PassData data)
	{
		cmd.SetGlobalBuffer(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._XpbdDebugAabbBuffer, data.Solver.SolverImpl.GizmosImpl.ColliderAabbBuffer);
		cmd.SetGlobalBuffer(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._XpbdDebugAabbMap, data.Solver.SolverImpl.GizmosImpl.CollidersMapBuffer);
		cmd.SetGlobalFloat(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._XpbdDebugConstructAabbFromConstraintParameters, 0f);
		cmd.SetGlobalColor(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._Color, Color.cyan);
		CommandBufferHelpers.GetNativeCommandBuffer(cmd).DrawMeshInstancedProcedural(XPBDMeshUtils.CubeMeshWithUvAndNormals, 0, data.GizmosMaterial, data.PassIndex, data.Solver.ColliderWorld.EntityAllocationMap.Count);
	}

	private static void RecordSimplexAabbPass(in RecordContext context, in XPBDGizmosContext gizmosContext)
	{
		PassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<PassData>("XPBD Gizmos Simplex Aabb", out passData, ProfilingSampler.Get(XPBDGizmosProfileId.XPBDGizmosSimplexAabb), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\RendererFeatures\\XPBDGizmos\\XPBDGizmosAfterRenderPass.cs", 343);
		passData.Solver = Owlcat.Runtime.Visual.XPBD.XPBD.Solver;
		passData.Gizmos = Owlcat.Runtime.Visual.XPBD.XPBD.Solver.SolverImpl.GizmosImpl;
		passData.GizmosMaterial = gizmosContext.GizmosMaterial;
		passData.PassIndex = gizmosContext.AabbPass;
		SetRenderTarget(in context, unsafeRenderGraphBuilder, gizmosContext.XPBDDebug.UseDepthBuffer);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData data, UnsafeGraphContext context)
		{
			ExecuteSimplexAabbPass(context.cmd, data);
		});
	}

	private static void ExecuteSimplexAabbPass(UnsafeCommandBuffer cmd, PassData data)
	{
		cmd.SetGlobalBuffer(data.Gizmos.ConstraintsParameters0Buffer.NameId, data.Gizmos.ConstraintsParameters0Buffer.Buffer);
		cmd.SetGlobalBuffer(data.Gizmos.ConstraintsParameters1Buffer.NameId, data.Gizmos.ConstraintsParameters1Buffer.Buffer);
		cmd.SetGlobalBuffer(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._XpbdDebugAabbMap, data.Gizmos.SimplexMapBuffer);
		cmd.SetGlobalFloat(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._XpbdDebugConstructAabbFromConstraintParameters, 1f);
		cmd.SetGlobalColor(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._Color, new Color32(145, 66, 26, byte.MaxValue));
		CommandBufferHelpers.GetNativeCommandBuffer(cmd).DrawMeshInstancedProcedural(XPBDMeshUtils.CubeMeshWithUvAndNormals, 0, data.GizmosMaterial, data.PassIndex, data.Solver.SolverImpl.GizmosImpl.SimplexMapBuffer.Buffer.count);
	}

	private static void RecordColliderContactsPass(in RecordContext context, in XPBDGizmosContext gizmosContext)
	{
		PassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<PassData>("XPBD Gizmos Collider Contacts", out passData, ProfilingSampler.Get(XPBDGizmosProfileId.XPBDGizmosColliderContacts), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\RendererFeatures\\XPBDGizmos\\XPBDGizmosAfterRenderPass.cs", 373);
		passData.Solver = Owlcat.Runtime.Visual.XPBD.XPBD.Solver;
		passData.Gizmos = Owlcat.Runtime.Visual.XPBD.XPBD.Solver.SolverImpl.GizmosImpl;
		passData.GizmosMaterial = gizmosContext.GizmosMaterial;
		passData.PassIndex = gizmosContext.ColliderContactsPass;
		SetRenderTarget(in context, unsafeRenderGraphBuilder, gizmosContext.XPBDDebug.UseDepthBuffer);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData data, UnsafeGraphContext context)
		{
			ExecuteColliderContactsPass(context.cmd, data);
		});
	}

	private static void ExecuteColliderContactsPass(UnsafeCommandBuffer cmd, PassData data)
	{
		CommandBuffer nativeCommandBuffer = CommandBufferHelpers.GetNativeCommandBuffer(cmd);
		data.Gizmos.ContactsBuffer.SetGlobal(nativeCommandBuffer);
		cmd.SetGlobalBuffer(data.Gizmos.ConstraintsParameters0Buffer.NameId, data.Gizmos.ConstraintsParameters0Buffer.Buffer);
		cmd.SetGlobalBuffer(data.Gizmos.ConstraintsParameters1Buffer.NameId, data.Gizmos.ConstraintsParameters1Buffer.Buffer);
		cmd.SetGlobalBuffer(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._XpbdDebugAabbBuffer, data.Solver.SolverImpl.GizmosImpl.ColliderAabbBuffer);
		cmd.SetGlobalColor(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._Color, new Color32(byte.MaxValue, 0, byte.MaxValue, byte.MaxValue));
		cmd.DrawMeshInstancedIndirect(XPBDMeshUtils.CubeMeshWithUvAndNormals, 0, data.GizmosMaterial, data.PassIndex, data.Gizmos.ColliderContactsIndirectArgsBuffer.Buffer, 0);
	}

	private static void RecordContactNormalsPass(in RecordContext context, in XPBDGizmosContext gizmosContext)
	{
		PassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<PassData>("XPBD Gizmos Contact Normals", out passData, ProfilingSampler.Get(XPBDGizmosProfileId.XPBDGizmosContactNormals), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\RendererFeatures\\XPBDGizmos\\XPBDGizmosAfterRenderPass.cs", 403);
		passData.Solver = Owlcat.Runtime.Visual.XPBD.XPBD.Solver;
		passData.Gizmos = Owlcat.Runtime.Visual.XPBD.XPBD.Solver.SolverImpl.GizmosImpl;
		passData.GizmosMaterial = gizmosContext.GizmosMaterial;
		passData.PassIndex = gizmosContext.ContactNormalsPass;
		SetRenderTarget(in context, unsafeRenderGraphBuilder, gizmosContext.XPBDDebug.UseDepthBuffer);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData data, UnsafeGraphContext context)
		{
			ExecuteContactNormalsPass(context.cmd, data);
		});
	}

	private static void ExecuteContactNormalsPass(UnsafeCommandBuffer cmd, PassData data)
	{
		cmd.SetGlobalColor(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._Color, new Color32(byte.MaxValue, 0, byte.MaxValue, byte.MaxValue));
		cmd.SetGlobalFloat(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._XpbdParticlesCount, data.Solver.BodyAllocator.ParticleSoA.Count);
		CommandBuffer nativeCommandBuffer = CommandBufferHelpers.GetNativeCommandBuffer(cmd);
		data.Gizmos.ContactsBuffer.SetGlobal(nativeCommandBuffer);
		data.Gizmos.ParticlePositionBuffer.SetGlobal(nativeCommandBuffer);
		cmd.DrawMeshInstancedIndirect(RenderingUtils.QuadMesh, 0, data.GizmosMaterial, data.PassIndex, data.Gizmos.SimplexContactsIndirectArgsBuffer.Buffer, 0);
	}

	private static void RecordVisibleBodyAabbsPass(in RecordContext context, in XPBDGizmosContext gizmosContext)
	{
		PassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<PassData>("XPBD Gizmos Visible Body Aabbs", out passData, ProfilingSampler.Get(XPBDGizmosProfileId.XPBDGizmosVisibleBodyAabbs), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\RendererFeatures\\XPBDGizmos\\XPBDGizmosAfterRenderPass.cs", 433);
		passData.Solver = Owlcat.Runtime.Visual.XPBD.XPBD.Solver;
		passData.Gizmos = Owlcat.Runtime.Visual.XPBD.XPBD.Solver.SolverImpl.GizmosImpl;
		passData.GizmosMaterial = gizmosContext.GizmosMaterial;
		passData.PassIndex = gizmosContext.AabbPass;
		SetRenderTarget(in context, unsafeRenderGraphBuilder, gizmosContext.XPBDDebug.UseDepthBuffer);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData data, UnsafeGraphContext context)
		{
			ExecuteVisibleBodyAabbsPass(context.cmd, data);
		});
	}

	private static void ExecuteVisibleBodyAabbsPass(UnsafeCommandBuffer cmd, PassData data)
	{
		cmd.SetGlobalBuffer(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._XpbdDebugAabbBuffer, data.Gizmos.BodyAabbBuffer);
		cmd.SetGlobalBuffer(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._XpbdDebugAabbMap, data.Gizmos.BodiesMapBuffer);
		cmd.SetGlobalBuffer(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._XpbdDebugAabbVisibilityBuffer, data.Gizmos.BodyVisibilityBuffer);
		cmd.SetGlobalFloat(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._XpbdDebugConstructAabbFromConstraintParameters, 0f);
		cmd.SetGlobalColor(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._Color, Color.green);
		cmd.SetGlobalColor(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._ColorSecondary, Color.red);
		cmd.SetGlobalFloat(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._XpbdDebugUseVisibilityBuffer, 1f);
		CommandBufferHelpers.GetNativeCommandBuffer(cmd).DrawMeshInstancedProcedural(XPBDMeshUtils.CubeMeshWithUvAndNormals, 0, data.GizmosMaterial, data.PassIndex, data.Solver.BodyAllocator.EntityAllocationMap.Count);
		cmd.SetGlobalFloat(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._XpbdDebugUseVisibilityBuffer, 0f);
	}

	private static void RecordDrawNormalsPass(in RecordContext context, in XPBDGizmosContext gizmosContext)
	{
		PassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<PassData>("XPBD Gizmos Normals", out passData, ProfilingSampler.Get(XPBDGizmosProfileId.XPBDGizmosDrawNormals), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\RendererFeatures\\XPBDGizmos\\XPBDGizmosAfterRenderPass.cs", 467);
		passData.Solver = Owlcat.Runtime.Visual.XPBD.XPBD.Solver;
		passData.Gizmos = Owlcat.Runtime.Visual.XPBD.XPBD.Solver.SolverImpl.GizmosImpl;
		passData.GizmosMaterial = gizmosContext.GizmosMaterial;
		passData.PassIndex = gizmosContext.NormalsPass;
		SetRenderTarget(in context, unsafeRenderGraphBuilder, gizmosContext.XPBDDebug.UseDepthBuffer);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData data, UnsafeGraphContext context)
		{
			ExecuteDrawNormalsPass(context.cmd, data);
		});
	}

	private static void ExecuteDrawNormalsPass(UnsafeCommandBuffer cmd, PassData data)
	{
		CommandBuffer nativeCommandBuffer = CommandBufferHelpers.GetNativeCommandBuffer(cmd);
		cmd.SetGlobalColor(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._Color, new Color(0f, 1f, 0f, 1f));
		data.Gizmos.MeshVerticesBuffer.SetGlobal(nativeCommandBuffer);
		cmd.SetGlobalFloat(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._XpbdDebugDrawRestNormals, 0f);
		cmd.DrawProcedural(Matrix4x4.identity, data.GizmosMaterial, data.PassIndex, MeshTopology.Quads, 4, data.Solver.BodyAllocator.VerticesSoA.Count);
	}

	private static void RecordDrawRestNormalsPass(in RecordContext context, in XPBDGizmosContext gizmosContext)
	{
		PassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<PassData>("XPBD Gizmos Rest Normals", out passData, ProfilingSampler.Get(XPBDGizmosProfileId.XPBDGizmosDrawRestNormals), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\RendererFeatures\\XPBDGizmos\\XPBDGizmosAfterRenderPass.cs", 495);
		passData.Solver = Owlcat.Runtime.Visual.XPBD.XPBD.Solver;
		passData.Gizmos = Owlcat.Runtime.Visual.XPBD.XPBD.Solver.SolverImpl.GizmosImpl;
		passData.GizmosMaterial = gizmosContext.GizmosMaterial;
		passData.PassIndex = gizmosContext.NormalsPass;
		SetRenderTarget(in context, unsafeRenderGraphBuilder, gizmosContext.XPBDDebug.UseDepthBuffer);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData data, UnsafeGraphContext context)
		{
			ExecuteDrawRestNormalsPass(context.cmd, data);
		});
	}

	private static void ExecuteDrawRestNormalsPass(UnsafeCommandBuffer cmd, PassData data)
	{
		cmd.SetGlobalColor(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._Color, new Color(0f, 0f, 1f, 1f));
		cmd.SetGlobalFloat(Owlcat.Runtime.Visual.XPBD.Shaders.ShaderPropertyId._XpbdDebugDrawRestNormals, 1f);
		CommandBuffer nativeCommandBuffer = CommandBufferHelpers.GetNativeCommandBuffer(cmd);
		data.Gizmos.MeshVerticesBuffer.SetGlobal(nativeCommandBuffer);
		cmd.DrawProcedural(Matrix4x4.identity, data.GizmosMaterial, data.PassIndex, MeshTopology.Quads, 4, data.Solver.BodyAllocator.VerticesSoA.Count);
	}

	private static void RecordDrawDeformedVerticesPass(in RecordContext context, in XPBDGizmosContext gizmosContext)
	{
		PassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<PassData>("XPBD Gizmos Deformed Vertices", out passData, ProfilingSampler.Get(XPBDGizmosProfileId.XPBDGizmosDrawDeformedVertices), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\RendererFeatures\\XPBDGizmos\\XPBDGizmosAfterRenderPass.cs", 523);
		passData.Solver = Owlcat.Runtime.Visual.XPBD.XPBD.Solver;
		passData.Gizmos = Owlcat.Runtime.Visual.XPBD.XPBD.Solver.SolverImpl.GizmosImpl;
		passData.GizmosMaterial = gizmosContext.GizmosMaterial;
		passData.PassIndex = gizmosContext.DrawDeformedVerticesPass;
		SetRenderTarget(in context, unsafeRenderGraphBuilder, gizmosContext.XPBDDebug.UseDepthBuffer);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData data, UnsafeGraphContext context)
		{
			ExecuteDrawDeformedVerticesPass(context.cmd, data);
		});
	}

	private static void ExecuteDrawDeformedVerticesPass(UnsafeCommandBuffer cmd, PassData data)
	{
		CommandBuffer nativeCommandBuffer = CommandBufferHelpers.GetNativeCommandBuffer(cmd);
		data.Gizmos.DeformableGizmosVerticesBuffer.SetGlobal(nativeCommandBuffer);
		cmd.DrawProcedural(Matrix4x4.identity, data.GizmosMaterial, data.PassIndex, MeshTopology.Quads, 4, data.Solver.MeshDeformerAllocator.DeformableSkinnedVerticesSoA.Count * 3);
	}

	private static void RecordDrawSimplexContactsPass(in RecordContext context, in XPBDGizmosContext gizmosContext)
	{
		PassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<PassData>("XPBD Gizmos Simplex Contacts", out passData, ProfilingSampler.Get(XPBDGizmosProfileId.XPBDGizmosDrawSimplexContacts), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\RendererFeatures\\XPBDGizmos\\XPBDGizmosAfterRenderPass.cs", 549);
		passData.Solver = Owlcat.Runtime.Visual.XPBD.XPBD.Solver;
		passData.Gizmos = Owlcat.Runtime.Visual.XPBD.XPBD.Solver.SolverImpl.GizmosImpl;
		passData.GizmosMaterial = gizmosContext.GizmosMaterial;
		passData.PassIndex = gizmosContext.DrawSimplexContactsPass;
		SetRenderTarget(in context, unsafeRenderGraphBuilder, gizmosContext.XPBDDebug.UseDepthBuffer);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData data, UnsafeGraphContext context)
		{
			ExecuteDrawSimplexContactsPass(context.cmd, data);
		});
	}

	private static void ExecuteDrawSimplexContactsPass(UnsafeCommandBuffer cmd, PassData data)
	{
		CommandBuffer nativeCommandBuffer = CommandBufferHelpers.GetNativeCommandBuffer(cmd);
		data.Gizmos.ContactsBuffer.SetGlobal(nativeCommandBuffer);
		data.Gizmos.ParticlePositionBuffer.SetGlobal(nativeCommandBuffer);
		data.Gizmos.ContactsBuffer.SetGlobal(nativeCommandBuffer);
		cmd.DrawMeshInstancedIndirect(RenderingUtils.QuadMesh, 0, data.GizmosMaterial, data.PassIndex, data.Gizmos.SimplexContactsIndirectArgsBuffer.Buffer, 0);
	}

	private static void SetRenderTarget(in RecordContext context, IUnsafeRenderGraphBuilder builder, bool useDepthBuffer)
	{
		if (useDepthBuffer)
		{
			builder.SetRenderAttachmentDepth(context.FrameResources.CameraStackTargets.Depth, AccessFlags.ReadWrite);
		}
		builder.SetRenderAttachment(context.FrameResources.CameraStackTargets.Color, 0);
	}
}
