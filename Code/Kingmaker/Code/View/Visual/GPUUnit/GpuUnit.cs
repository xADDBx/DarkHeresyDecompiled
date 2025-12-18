using System.Linq;
using UnityEngine;

namespace Kingmaker.Code.View.Visual.GPUUnit;

public class GpuUnit : MonoBehaviour
{
	public string m_PrefabId = string.Empty;

	[SerializeReference]
	public AnimationClip[] m_Animations = new AnimationClip[4];

	private MeshFilter m_MeshFilter;

	private MeshRenderer m_MeshRenderer;

	private bool m_Initialized;

	private void Start()
	{
		SetRandomAnimation();
	}

	private void OnEnable()
	{
		SetRandomAnimation();
	}

	public void SetRandomAnimation()
	{
		InitComponents();
		m_MeshRenderer.sharedMaterial.SetInt("_ClipIndex0", Random.Range(0, m_Animations.Count((AnimationClip item) => item != null)));
	}

	public void InitComponents()
	{
		if (!m_Initialized)
		{
			m_MeshFilter = GetComponent<MeshFilter>();
			m_MeshRenderer = GetComponent<MeshRenderer>();
			m_Initialized = true;
		}
	}

	public void SetMesh(Mesh mesh)
	{
		InitComponents();
		if (m_MeshFilter != null)
		{
			m_MeshFilter.mesh = mesh;
		}
	}

	public void SetMaterial(Material material)
	{
		InitComponents();
		if (m_MeshRenderer != null)
		{
			m_MeshRenderer.material = material;
		}
	}
}
