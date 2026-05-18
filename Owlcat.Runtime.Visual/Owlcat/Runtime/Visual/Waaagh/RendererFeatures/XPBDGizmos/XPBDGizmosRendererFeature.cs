using System;
using Owlcat.Runtime.Visual.XPBD;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.XPBDGizmos;

internal sealed class XPBDGizmosRendererFeature : IRendererFeature, IDisposable
{
	private readonly XPBDGizmosContext m_GizmosContext;

	public XPBDGizmosRendererFeature()
	{
		XPBDGizmosFeatureResources renderPipelineSettings = GraphicsSettings.GetRenderPipelineSettings<XPBDGizmosFeatureResources>();
		Material material = CoreUtils.CreateEngineMaterial(renderPipelineSettings.GizmosShader);
		m_GizmosContext = new XPBDGizmosContext
		{
			XPBDDebug = WaaaghPipeline.Asset.DebugData.XPBDDebug,
			GizmosMaterial = CoreUtils.CreateEngineMaterial(renderPipelineSettings.GizmosShader),
			ParticlesRadiusPass = material.FindPass("PARTICLES RADIUS"),
			ParticlesPass = material.FindPass("PARTICLES"),
			ConstraintsPass = material.FindPass("CONSTRAINTS"),
			VelocitiesPass = material.FindPass("VELOCITIES"),
			InertialForcesPass = material.FindPass("INERTIAL FORCES"),
			AabbPass = material.FindPass("AABB"),
			ColliderContactsPass = material.FindPass("COLLIDER CONTACTS"),
			ContactNormalsPass = material.FindPass("CONTACT NORMALS"),
			NormalsPass = material.FindPass("NORMALS"),
			DrawDeformedVerticesPass = material.FindPass("DEFORMABLE VERTICES"),
			DrawSimplexContactsPass = material.FindPass("SIMPLEX CONTACTS")
		};
	}

	public void Dispose()
	{
		CoreUtils.Destroy(m_GizmosContext.GizmosMaterial);
	}

	public void RegisterExtensions(RendererFeatureExtensionRegistry registry)
	{
		registry.AddRecordDelegate(RecordExtensionPoint.BeforeDeferredLighting, OnBeforeDeferredLighting);
		registry.AddRecordDelegate(RecordExtensionPoint.AfterRendering, OnAfterRendering);
	}

	private void OnBeforeDeferredLighting(in RecordContext context)
	{
		if (ShouldDrawGizmos())
		{
			XPBDGizmosGBufferPass.Record(in context, in m_GizmosContext);
		}
	}

	private void OnAfterRendering(in RecordContext context)
	{
		if (ShouldDrawGizmos())
		{
			XPBDGizmosAfterRenderPass.Record(in context, in m_GizmosContext);
		}
	}

	private bool ShouldDrawGizmos()
	{
		if (Application.isPlaying && m_GizmosContext.XPBDDebug.GizmosEnabled && !Owlcat.Runtime.Visual.XPBD.XPBD.IsEmpty)
		{
			return Owlcat.Runtime.Visual.XPBD.XPBD.Solver.SolverImpl.GizmosImpl.IsValid;
		}
		return false;
	}
}
