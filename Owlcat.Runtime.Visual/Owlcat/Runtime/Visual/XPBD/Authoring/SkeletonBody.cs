using System;
using System.Collections.Generic;
using System.Linq;
using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Owlcat.Runtime.Visual.XPBD.Layouts;
using Owlcat.Runtime.Visual.XPBD.Shaders;
using Owlcat.Runtime.Visual.XPBD.Solvers;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace Owlcat.Runtime.Visual.XPBD.Authoring;

public class SkeletonBody : AuthoringBase<SkeletonLayout>
{
	private static Stack<Transform> s_Stack = new Stack<Transform>();

	private Transform[] m_Bones;

	private TransformAccessArray m_TransformAccessArray;

	private List<MeshRenderer> m_Renderers = new List<MeshRenderer>();

	private List<AuthoringPropertyBlock> m_MaterialProperties;

	private Dictionary<MeshRenderer, int> m_RendererDataMap = new Dictionary<MeshRenderer, int>();

	protected override void OnRegister()
	{
		m_Renderers.Clear();
		base.gameObject.GetComponentsInChildren(m_Renderers);
		InitMaterialPropertiesIfNeed();
		InitBonesIfNeed();
	}

	protected override void OnUnregister()
	{
		DisableShader();
		DisposeTransformArray();
	}

	protected override void OnAfterDestroy()
	{
		DisableShader();
		DisposeTransformArray();
	}

	private void InitMaterialPropertiesIfNeed()
	{
		if (m_MaterialProperties != null)
		{
			return;
		}
		m_MaterialProperties = new List<AuthoringPropertyBlock>();
		m_RendererDataMap = new Dictionary<MeshRenderer, int>();
		for (int i = 0; i < base.Layout.BodyStructure.SkeletonMeshNames.Count; i++)
		{
			string meshName = base.Layout.BodyStructure.SkeletonMeshNames[i];
			MeshRenderer meshRenderer = m_Renderers.First((MeshRenderer r) => r.GetComponent<MeshFilter>().sharedMesh.name == meshName);
			m_RendererDataMap.Add(meshRenderer, i);
			m_MaterialProperties.Add(new AuthoringPropertyBlock(meshRenderer));
		}
	}

	private void InitBonesIfNeed()
	{
		if (m_Bones == null)
		{
			m_Bones = new Transform[base.Layout.BodyStructure.BoneNames.Count];
			int i;
			for (i = 0; i < m_Bones.Length; i++)
			{
				m_Bones[i] = FindRecursive(base.transform, (Transform t) => t.name == base.Layout.BodyStructure.BoneNames[i]);
			}
		}
		if (!m_TransformAccessArray.isCreated)
		{
			m_TransformAccessArray = new TransformAccessArray(m_Bones);
		}
	}

	private Transform FindRecursive(Transform root, Func<Transform, bool> predicate)
	{
		s_Stack.Clear();
		s_Stack.Push(root);
		while (s_Stack.Count > 0)
		{
			Transform transform = s_Stack.Pop();
			if (predicate(transform))
			{
				return transform;
			}
			int childCount = transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				s_Stack.Push(transform.GetChild(i));
			}
		}
		return null;
	}

	internal override void AfterEnabledStateSync(Solver solver)
	{
		base.AfterEnabledStateSync(solver);
		if (base.isActiveAndEnabled)
		{
			InitBonesIfNeed();
			return;
		}
		DisposeTransformArray();
		DisableShader();
	}

	private void DisposeTransformArray()
	{
		if (m_TransformAccessArray.isCreated)
		{
			m_TransformAccessArray.Dispose();
		}
	}

	protected override void BeginStepInternal(Solver solver)
	{
		base.BeginStepInternal(solver);
		if (base.isActiveAndEnabled)
		{
			solver.UpdateBoneTranforms(this, m_TransformAccessArray);
		}
	}

	protected override void EndStepInternal(Solver solver)
	{
		base.EndStepInternal(solver);
		int2 @int = solver.BodyAllocator.BodyDescriptorSoA.BoneIndicesMapRangesRange[base.IndexInSolver];
		int2 int2 = solver.BodyAllocator.BodyDescriptorSoA.BoneIndicesMapRange[base.IndexInSolver];
		int2 int3 = solver.BodyAllocator.BodyDescriptorSoA.BonesRange[base.IndexInSolver];
		foreach (KeyValuePair<MeshRenderer, int> item in m_RendererDataMap)
		{
			AuthoringPropertyBlock authoringPropertyBlock = m_MaterialProperties[item.Value];
			authoringPropertyBlock.SetFloat(ShaderPropertyId._XpbdEnabledLocal, 1f);
			int2 int4 = solver.BodyAllocator.BoneIndicesMapRangesSoA[@int.x + item.Value];
			authoringPropertyBlock.SetFloat(ShaderPropertyId._XpbdBoneIndicesOffset, int2.x + int4.x);
			authoringPropertyBlock.SetFloat(ShaderPropertyId._XbdBonesOffset, int3.x);
			authoringPropertyBlock.Apply();
			UpdateRenderAabb(solver, item.Key);
		}
	}

	private void DisableShader()
	{
		foreach (KeyValuePair<MeshRenderer, int> item in m_RendererDataMap)
		{
			AuthoringPropertyBlock authoringPropertyBlock = m_MaterialProperties[item.Value];
			authoringPropertyBlock.SetFloat(ShaderPropertyId._XpbdEnabledLocal, 0f);
			authoringPropertyBlock.Apply();
		}
	}

	private void UpdateRenderAabb(Solver solver, MeshRenderer renderer)
	{
		if (!(renderer == null) && renderer.TryGetComponent<MeshFilter>(out var component))
		{
			Bounds bounds = component.sharedMesh.bounds;
			Aabb bodyAabb = m_BodyAabb;
			Matrix4x4 worldToLocalMatrix = renderer.transform.worldToLocalMatrix;
			bodyAabb.Transform(in worldToLocalMatrix);
			bounds.Encapsulate(new Bounds((Vector3)bodyAabb.Center, (Vector3)bodyAabb.Size));
			renderer.localBounds = bounds;
		}
	}
}
