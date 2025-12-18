using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Owlcat.Runtime.Visual.XPBD.Shaders;
using Owlcat.Runtime.Visual.XPBD.Solvers;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.XPBD.Authoring;

[RequireComponent(typeof(MeshRenderer))]
public class MeshBody : MeshBodyBase
{
	private AuthoringPropertyBlock m_PropertyBlock;

	private MeshRenderer m_Renderer;

	private MeshFilter m_MeshFilter;

	protected override Renderer Renderer => m_Renderer;

	protected override bool TryInitialize()
	{
		m_Renderer = GetComponent<MeshRenderer>();
		m_MeshFilter = GetComponent<MeshFilter>();
		m_PropertyBlock = new AuthoringPropertyBlock(m_Renderer);
		if (m_Renderer != null && base.Layout != null)
		{
			return true;
		}
		return false;
	}

	protected override void OnUnregister()
	{
		DisableShader();
	}

	internal override void AfterEnabledStateSync(Solver solver)
	{
		base.AfterEnabledStateSync(solver);
		if (base.isActiveAndEnabled)
		{
			SetupShader(solver);
		}
		else
		{
			DisableShader();
		}
	}

	protected override void BeginStepInternal(Solver solver)
	{
		base.BeginStepInternal(solver);
		if (base.isActiveAndEnabled)
		{
			solver.UpdateMeshBasePositions(this);
		}
	}

	protected override void EndStepInternal(Solver solver)
	{
		base.EndStepInternal(solver);
		SetupShader(solver);
		UpdateAabb();
	}

	private void SetupShader(Solver solver)
	{
		int2 @int = solver.BodyAllocator.BodyDescriptorSoA.VertexToParticleRange[base.IndexInSolver];
		int2 int2 = solver.BodyAllocator.BodyDescriptorSoA.VerticesRange[base.IndexInSolver];
		m_PropertyBlock.SetFloat(ShaderPropertyId._XpbdEnabledLocal, 1f);
		m_PropertyBlock.SetFloat(ShaderPropertyId._XpbdVertexToParticleMapOffset, @int.x);
		m_PropertyBlock.SetFloat(ShaderPropertyId._XpbdVertexOffset, int2.x);
		m_PropertyBlock.Apply();
	}

	private void DisableShader()
	{
		m_PropertyBlock.SetFloat(ShaderPropertyId._XpbdEnabledLocal, 0f);
		m_PropertyBlock.Apply();
	}

	private void UpdateAabb()
	{
		Transform transform = GetTransform();
		if (!(transform == null))
		{
			Aabb bodyAabb = m_BodyAabb;
			Matrix4x4 worldToLocalMatrix = transform.worldToLocalMatrix;
			bodyAabb.Transform(in worldToLocalMatrix);
			if (m_MeshFilter != null && m_MeshFilter.sharedMesh != null && !base.Layout.BodyStructure.AllParticlesInUse)
			{
				Bounds bounds = m_MeshFilter.sharedMesh.bounds;
				float3 min = bounds.min;
				float3 max = bounds.max;
				Aabb other = new Aabb(in min, in max);
				bodyAabb.Encapsulate(in other);
			}
			m_Renderer.localBounds = bodyAabb.ToBounds();
		}
	}
}
