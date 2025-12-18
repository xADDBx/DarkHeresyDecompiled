using System.Collections.Generic;
using Owlcat.Runtime.Visual.XPBD.Authoring;
using Owlcat.Runtime.Visual.XPBD.Collisions;
using Owlcat.Runtime.Visual.XPBD.Solvers;
using Owlcat.Runtime.Visual.XPBD.Utilities;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD;

public class Simulation
{
	private readonly XPBDConfig m_Config;

	private Updater m_Updater;

	private Solver m_Solver;

	internal Solver Solver => m_Solver;

	public Simulation(XPBDConfig config)
	{
		m_Config = config;
		m_Solver = new Solver(config);
		m_Updater = new Updater(m_Solver);
		InsertToPlayerLoop();
	}

	private void InsertToPlayerLoop()
	{
		PlayerLoopHelper.Insert(typeof(FixedUpdate), typeof(XPBD.PreFixedUpdate), PreFixedUpdate, 0);
		PlayerLoopHelper.Insert(typeof(PostLateUpdate), typeof(XPBD.PostLateUpdate), PostLateUpdate);
		RenderPipelineManager.beginContextRendering += OnBeginRendering;
	}

	public void Dispose()
	{
		m_Solver.Dispose();
		RemoveFromPlayerLoop();
	}

	private void RemoveFromPlayerLoop()
	{
		PlayerLoopHelper.Remove(typeof(FixedUpdate), typeof(XPBD.PreFixedUpdate), PreFixedUpdate);
		PlayerLoopHelper.Remove(typeof(PostLateUpdate), typeof(XPBD.PostLateUpdate), PostLateUpdate);
		RenderPipelineManager.beginContextRendering -= OnBeginRendering;
	}

	internal void RegisterAuthoring(AuthoringBase authoring)
	{
		m_Solver.RegisterAuthoring(authoring);
	}

	internal void UnregisterAuthoring(AuthoringBase authoring)
	{
		m_Solver.UnregisterAuthoring(authoring);
	}

	internal void SyncAuthoringEnabledState(AuthoringBase authoringBase)
	{
		Solver.SyncAuthoringEnabledState(authoringBase);
	}

	internal void ResetAuthoring(AuthoringBase authoring)
	{
		m_Solver.ResetAuthoring(authoring);
	}

	internal void RegisterParticleAttachment(ParticleAttachment particleAttachment)
	{
		m_Solver.RegisterParticleAttachment(particleAttachment);
	}

	internal void UnregisterParticleAttachment(ParticleAttachment particleAttachment)
	{
		m_Solver.UnregisterParticleAttachment(particleAttachment);
	}

	internal void RegisterCollider(XpbdCollider xpbdCollider)
	{
		m_Solver.RegisterCollider(xpbdCollider);
	}

	internal void UnregisterCollider(XpbdCollider xpbdCollider)
	{
		m_Solver.UnregisterCollider(xpbdCollider);
	}

	internal void RegisterMeshDeformer(MeshDeformer meshDeformer)
	{
		m_Solver.RegisterMeshDeformer(meshDeformer);
	}

	internal void UnregisterMeshDeformer(MeshDeformer meshDeformer)
	{
		m_Solver.UnregisterMeshDeformer(meshDeformer);
	}

	internal void GetColliderDesc(XpbdCollider xpbdCollider, out ColliderDescriptor desc)
	{
		int index = m_Solver.ColliderWorld.EntityAllocationMap[xpbdCollider];
		desc = m_Solver.ColliderWorld.Allocations[index];
	}

	internal void PreFixedUpdate()
	{
		if (Application.isPlaying)
		{
			m_Updater.FixedUpdate();
		}
	}

	internal void PostLateUpdate()
	{
		if (Application.isPlaying)
		{
			m_Updater.PostLateUpdate();
		}
	}

	internal void TickByScript()
	{
		if (Application.isPlaying)
		{
			m_Updater.TickBySript();
		}
	}

	internal void OnBeginRendering(ScriptableRenderContext context, List<Camera> cameras)
	{
		if (Application.isPlaying)
		{
			m_Updater.OnBeginRendering(context, cameras);
		}
	}
}
