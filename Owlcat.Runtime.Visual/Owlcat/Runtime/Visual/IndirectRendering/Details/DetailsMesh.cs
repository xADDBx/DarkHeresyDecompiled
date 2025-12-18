using System;
using System.Collections.Generic;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using Owlcat.Runtime.Visual.GPUDrivenBRG;
using Owlcat.Runtime.Visual.Lighting;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Owlcat.Runtime.Visual.IndirectRendering.Details;

[ExecuteInEditMode]
public class DetailsMesh : MonoBehaviour, IIndirectMesh
{
	public enum ShapeCullingMode
	{
		Disabled,
		CullInside,
		CullOutside
	}

	[SerializeField]
	private Mesh m_Mesh;

	[SerializeField]
	private List<Material> m_Materials = new List<Material>();

	[SerializeField]
	private LightLayerEnum m_RenderingLayerMask = LightLayerEnum.LightLayerDefault;

	[SerializeField]
	private DetailsData m_Data;

	[SerializeField]
	[MinMaxSlider(0.01f, 2f)]
	private Vector2 m_ScaleRange = Vector2.one;

	[SerializeField]
	[MinMaxSlider(-180f, 180f)]
	private Vector2 m_RotationRange = new Vector2(-180f, 180f);

	[SerializeField]
	[HideInInspector]
	private string m_Guid;

	private readonly NativeListWrapper<IndirectInstanceData> m_RuntimeInstanceData = new NativeListWrapper<IndirectInstanceData>(Allocator.Persistent);

	private readonly NativeListWrapper<GPUDrivenInstanceID> m_RuntimeInstanceIDs = new NativeListWrapper<GPUDrivenInstanceID>(Allocator.Persistent);

	private bool m_IsDirty;

	private IndirectInstancingMeshFlags m_MeshFlags;

	public Mesh Mesh
	{
		get
		{
			return m_Mesh;
		}
		set
		{
			m_Mesh = value;
		}
	}

	public List<Material> Materials
	{
		get
		{
			return m_Materials;
		}
		set
		{
			m_Materials = value;
		}
	}

	public DetailsData Data
	{
		get
		{
			return m_Data;
		}
		set
		{
			m_Data = value;
		}
	}

	public bool IsDynamic => false;

	public int MaxDynamicInstances => 0;

	public string Guid => m_Guid;

	public Vector3 Position => base.transform.position;

	public Vector2 ScaleRange
	{
		get
		{
			return m_ScaleRange;
		}
		set
		{
			m_ScaleRange = value;
		}
	}

	public Vector2 RotationRange
	{
		get
		{
			return m_RotationRange;
		}
		set
		{
			m_RotationRange = value;
		}
	}

	public LightLayerEnum RenderingLayerMask
	{
		get
		{
			return m_RenderingLayerMask;
		}
		set
		{
			m_RenderingLayerMask = value;
		}
	}

	public Scene Scene => base.gameObject.scene;

	public ulong SceneCullingMask => base.gameObject.sceneCullingMask;

	public GameObject GameObject => base.gameObject;

	public IndirectInstancingMeshFlags Flags => m_MeshFlags;

	public ShapeCullingMode ShapeCulling
	{
		get
		{
			if ((m_MeshFlags & IndirectInstancingMeshFlags.ShapeCullingEnabled) != 0)
			{
				if ((m_MeshFlags & IndirectInstancingMeshFlags.ShapeCullingOutside) == 0)
				{
					return ShapeCullingMode.CullInside;
				}
				return ShapeCullingMode.CullOutside;
			}
			return ShapeCullingMode.Disabled;
		}
		set
		{
			IndirectInstancingMeshFlags indirectInstancingMeshFlags = m_MeshFlags & ~(IndirectInstancingMeshFlags.ShapeCullingEnabled | IndirectInstancingMeshFlags.ShapeCullingOutside);
			switch (value)
			{
			case ShapeCullingMode.CullInside:
				indirectInstancingMeshFlags |= IndirectInstancingMeshFlags.ShapeCullingEnabled;
				break;
			case ShapeCullingMode.CullOutside:
				indirectInstancingMeshFlags |= IndirectInstancingMeshFlags.ShapeCullingEnabled | IndirectInstancingMeshFlags.ShapeCullingOutside;
				break;
			}
			if (m_MeshFlags != indirectInstancingMeshFlags)
			{
				m_MeshFlags = indirectInstancingMeshFlags;
				IndirectRenderingSystem.Instance.SetMeshFlagsDirty();
			}
		}
	}

	private void OnDestroy()
	{
		m_RuntimeInstanceData.Dispose();
		m_RuntimeInstanceIDs.Dispose();
	}

	private void OnEnable()
	{
		if (string.IsNullOrEmpty(m_Guid))
		{
			m_Guid = System.Guid.NewGuid().ToString();
		}
		IndirectRenderingSystem.Instance.RegisterMesh(this);
		m_IsDirty = true;
	}

	private void OnDisable()
	{
		IndirectRenderingSystem.Instance.UnregisterMesh(this);
	}

	private void Update()
	{
		if (m_IsDirty)
		{
			UpdateInstances();
			m_IsDirty = false;
		}
	}

	public void UpdateInstances()
	{
		if (!(Data != null))
		{
			return;
		}
		m_RuntimeInstanceData.Clear();
		if (Application.IsPlaying(this))
		{
			Data.SortInstancesByMortonCode();
		}
		bool flag = IndirectRenderingSystem.Instance.RequiresInstanceIDs(this);
		foreach (DetailInstanceData instance in Data.Instances)
		{
			Matrix4x4 objectToWorld = Matrix4x4.TRS(instance.Position, Quaternion.Euler(0f, instance.Rotation, 0f), Vector3.one * instance.Scale);
			m_RuntimeInstanceData.Add(new IndirectInstanceData
			{
				objectToWorld = objectToWorld,
				worldToObject = objectToWorld.inverse,
				meshID = 0u,
				tintColor = instance.TintColor,
				shadowmask = instance.Shadowmask
			});
			if (flag && m_RuntimeInstanceData.Length > m_RuntimeInstanceIDs.Length)
			{
				m_RuntimeInstanceIDs.Add(GPUDrivenInstanceCommandQueue.GetUniqueCustomInstanceID());
			}
		}
		IndirectRenderingSystem.Instance.SetMeshInstances(this, m_RuntimeInstanceData, flag ? m_RuntimeInstanceIDs : null);
	}

	private void OnDrawGizmosSelected()
	{
	}
}
