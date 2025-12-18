using Owlcat.Runtime.Visual.XPBD.Constraints;
using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Owlcat.Runtime.Visual.XPBD.Shaders;
using Owlcat.Runtime.Visual.XPBD.Solvers;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.XPBD.Authoring;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class SkinnedMeshBody : MeshBodyBase
{
	private SkinnedMeshRenderer m_SkinnedMeshRenderer;

	private AuthoringPropertyBlock m_PropertyBlock;

	private GraphicsBuffer m_VertexBuffer;

	[SerializeField]
	[Tooltip("Copies animated vertices from mesh into simulation. Useful for skinned animated characters and objects.")]
	public bool NeedUpdateSkin = true;

	[SerializeField]
	[Tooltip("Recalculates normals after skinning. Produces more acccurate normals for Shape Constraints.")]
	public bool NeedRecalculateSkinNormals;

	protected override Renderer Renderer => m_SkinnedMeshRenderer;

	public override Transform GetTransform()
	{
		if (m_SkinnedMeshRenderer != null)
		{
			return m_SkinnedMeshRenderer.rootBone;
		}
		return base.GetTransform();
	}

	protected override bool TryInitialize()
	{
		m_SkinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
		m_PropertyBlock = new AuthoringPropertyBlock(m_SkinnedMeshRenderer);
		if (m_SkinnedMeshRenderer != null && base.Layout != null)
		{
			return true;
		}
		return false;
	}

	protected override void OnUnregister()
	{
		DisableShader();
		DisposeVertexBuffer();
	}

	protected override void OnAfterAwake()
	{
		base.OnAfterAwake();
	}

	protected override void OnAfterDestroy()
	{
		DisposeVertexBuffer();
	}

	private void EnsureVertexBuffer()
	{
		if (base.isActiveAndEnabled && NeedUpdateSkin)
		{
			if (m_VertexBuffer == null || !m_VertexBuffer.IsValid())
			{
				m_VertexBuffer = m_SkinnedMeshRenderer.GetVertexBuffer();
			}
		}
		else
		{
			DisposeVertexBuffer();
		}
	}

	private void DisposeVertexBuffer()
	{
		m_VertexBuffer?.Dispose();
		m_VertexBuffer = null;
	}

	internal override void AfterEnabledStateSync(Solver solver)
	{
		base.AfterEnabledStateSync(solver);
		if (base.isActiveAndEnabled)
		{
			SetupShader(solver);
			return;
		}
		DisableShader();
		DisposeVertexBuffer();
	}

	protected override void BeginStepInternal(Solver solver)
	{
		base.BeginStepInternal(solver);
		EnsureVertexBuffer();
		if (NeedUpdateSkin && base.isActiveAndEnabled)
		{
			bool recalculateNormals = NeedRecalculateSkinNormals && base.ConstraintSettings[ConstraintType.Shape].Enabled;
			solver.UpdateSkin(this, m_VertexBuffer, recalculateNormals);
		}
	}

	protected override void EndStepInternal(Solver solver)
	{
		base.EndStepInternal(solver);
		SetupShader(solver);
		UpdateRendererAabb();
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

	private void UpdateRendererAabb()
	{
		Transform transform = GetTransform();
		if (!(transform == null))
		{
			Aabb bodyAabb = m_BodyAabb;
			Matrix4x4 worldToLocalMatrix = transform.worldToLocalMatrix;
			bodyAabb.Transform(in worldToLocalMatrix);
			if (!base.Layout.BodyStructure.AllParticlesInUse && m_SkinnedMeshRenderer.sharedMesh != null)
			{
				Bounds bounds = m_SkinnedMeshRenderer.sharedMesh.bounds;
				float3 min = bounds.min;
				float3 max = bounds.max;
				Aabb other = new Aabb(in min, in max);
				bodyAabb.Encapsulate(in other);
			}
			m_SkinnedMeshRenderer.localBounds = bodyAabb.ToBounds();
		}
	}
}
