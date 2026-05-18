using System;
using System.Collections.Generic;
using System.Linq;
using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Owlcat.Runtime.Visual.XPBD.Layouts.MeshSkinning;
using Owlcat.Runtime.Visual.XPBD.Shaders;
using Owlcat.Runtime.Visual.XPBD.Solvers;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.XPBD.Authoring;

public class MeshDeformer : XPBDEntity
{
	private class VertexComparer : IEqualityComparer<SlaveVertex>
	{
		public bool Equals(SlaveVertex x, SlaveVertex y)
		{
			return x.SlaveIndex == y.SlaveIndex;
		}

		public int GetHashCode(SlaveVertex obj)
		{
			return obj.SlaveIndex.GetHashCode();
		}
	}

	private AuthoringPropertyBlock m_PropertyBlock;

	private Renderer m_Renderer;

	private Mesh m_Mesh;

	private Matrix4x4 m_PrevWorldToLocal;

	private int[] m_VertexToSkinnedVertexMap;

	private int m_DeformableVerticesCount;

	private SkinnedMeshRenderer m_SkinnedMeshRenderer;

	[SerializeField]
	private List<MeshDeformerBinding> m_Bindings = new List<MeshDeformerBinding>();

	public static Action<MeshDeformer> MasterIndexInSolverChanged;

	public List<MeshDeformerBinding> Bindings => m_Bindings;

	public int[] VertexToSkinnedVertexMap => m_VertexToSkinnedVertexMap;

	public int DeformableVerticesCount => m_DeformableVerticesCount;

	private void OnEnable()
	{
		FindMeshAndRenderer();
		if (m_Mesh != null && m_Renderer != null)
		{
			SubscribeOnMasters();
			EnableDeformer();
			m_PropertyBlock = new AuthoringPropertyBlock(m_Renderer);
		}
	}

	private void OnDisable()
	{
		UnsubscribeOnMasters();
		DisableDeformer();
	}

	public override Transform GetTransform()
	{
		if (base.IsValid)
		{
			if (m_SkinnedMeshRenderer != null)
			{
				return m_SkinnedMeshRenderer.rootBone;
			}
			return base.transform;
		}
		return null;
	}

	public void AddBinding(MeshDeformerBinding binding)
	{
		UnsubscribeOnMasters();
		DisableDeformer();
		m_Bindings.Add(binding);
		SubscribeOnMasters();
		EnableDeformer();
	}

	private void FindMeshAndRenderer()
	{
		m_SkinnedMeshRenderer = null;
		SkinnedMeshRenderer component3;
		if (TryGetComponent<MeshRenderer>(out var component))
		{
			m_Renderer = component;
			if (TryGetComponent<MeshFilter>(out var component2))
			{
				m_Mesh = component2.sharedMesh;
			}
		}
		else if (TryGetComponent<SkinnedMeshRenderer>(out component3))
		{
			m_Renderer = component3;
			m_SkinnedMeshRenderer = component3;
			m_Mesh = component3.sharedMesh;
		}
	}

	private void EnableDeformer()
	{
		if (ValidateBindings())
		{
			BuildVertexMapIfNeed();
			XPBD.RegisterMeshDeformer(this);
		}
	}

	private void DisableDeformer()
	{
		XPBD.UnregisterMeshDeformer(this);
		m_PropertyBlock.SetFloat(ShaderPropertyId._XpbdEnabledLocal, 0f);
		m_PropertyBlock.Apply();
	}

	private void SubscribeOnMasters()
	{
		foreach (MeshDeformerBinding binding in m_Bindings)
		{
			if (binding.Master != null)
			{
				MeshBodyBase master = binding.Master;
				master.IndexInSolverChanged = (Action<XPBDEntity>)Delegate.Combine(master.IndexInSolverChanged, new Action<XPBDEntity>(OnMasterIndexInSolverChanged));
			}
		}
	}

	private void UnsubscribeOnMasters()
	{
		foreach (MeshDeformerBinding binding in m_Bindings)
		{
			if (binding.Master != null)
			{
				MeshBodyBase master = binding.Master;
				master.IndexInSolverChanged = (Action<XPBDEntity>)Delegate.Remove(master.IndexInSolverChanged, new Action<XPBDEntity>(OnMasterIndexInSolverChanged));
			}
		}
	}

	private void OnMasterIndexInSolverChanged(XPBDEntity master)
	{
		if (master.IsActiveInSimulation)
		{
			if (ValidateBindings())
			{
				if (!base.IsActiveInSimulation)
				{
					EnableDeformer();
				}
				else
				{
					MasterIndexInSolverChanged?.Invoke(this);
				}
			}
		}
		else if (base.IsActiveInSimulation)
		{
			DisableDeformer();
		}
	}

	internal void EndStep(Solver solver)
	{
		if (base.IsValid)
		{
			EndStepInternal(solver);
		}
	}

	private void EndStepInternal(Solver solver)
	{
		int2 @int = solver.MeshDeformerAllocator.DescriptorsSoA.SkinnedVerticesRange[base.IndexInSolver];
		int2 int2 = solver.MeshDeformerAllocator.DescriptorsSoA.VertexToSkinnedVertexMapRange[base.IndexInSolver];
		m_PropertyBlock.SetMatrix(ShaderPropertyId._XpbdBodyWorldToLocal, m_PrevWorldToLocal);
		m_PropertyBlock.SetFloat(ShaderPropertyId._XpbdEnabledLocal, 1f);
		m_PropertyBlock.SetFloat(ShaderPropertyId._XpbdDeformableSkinnedVerticesOffset, @int.x);
		m_PropertyBlock.SetFloat(ShaderPropertyId._XpbdVertexToDeformableVertexMapOffset, int2.x);
		m_PropertyBlock.Apply();
		UpdateAabb(solver);
	}

	private void UpdateAabb(Solver solver)
	{
		Bounds bounds = m_Mesh.bounds;
		Matrix4x4 worldToLocalMatrix = m_Renderer.transform.worldToLocalMatrix;
		for (int i = 0; i < m_Bindings.Count; i++)
		{
			Aabb bodyAabb = m_Bindings[i].Master.BodyAabb;
			bodyAabb.Transform(in worldToLocalMatrix);
			bounds.Encapsulate(new Bounds((Vector3)bodyAabb.Center, (Vector3)bodyAabb.Size));
		}
		m_Renderer.localBounds = bounds;
	}

	private void BuildVertexMapIfNeed()
	{
		if (m_VertexToSkinnedVertexMap != null && m_VertexToSkinnedVertexMap.Length != 0 && m_VertexToSkinnedVertexMap.Length == m_Mesh.vertexCount)
		{
			return;
		}
		m_VertexToSkinnedVertexMap = new int[m_Mesh.vertexCount];
		for (int i = 0; i < m_VertexToSkinnedVertexMap.Length; i++)
		{
			m_VertexToSkinnedVertexMap[i] = -1;
		}
		m_DeformableVerticesCount = 0;
		foreach (MeshDeformerBinding binding in m_Bindings)
		{
			for (int j = 0; j < binding.Skinmap.SkinnedVertices.Count; j++)
			{
				SlaveVertex slaveVertex = binding.Skinmap.SkinnedVertices[j];
				if (m_VertexToSkinnedVertexMap[slaveVertex.SlaveIndex] > -1)
				{
					throw new ArgumentException($"Slave index {slaveVertex.SlaveIndex} already exists");
				}
				m_VertexToSkinnedVertexMap[slaveVertex.SlaveIndex] = m_DeformableVerticesCount;
				m_DeformableVerticesCount++;
			}
		}
	}

	private bool ValidateBindings()
	{
		if (m_Bindings == null || m_Bindings.Count == 0 || m_Bindings.Any((MeshDeformerBinding b) => b == null || b.Master == null || b.Skinmap == null))
		{
			return false;
		}
		foreach (MeshDeformerBinding binding in m_Bindings)
		{
			if (!binding.Master.IsActiveInSimulation)
			{
				return false;
			}
		}
		new VertexComparer();
		return true;
	}
}
