using System.Collections.Generic;
using UnityEngine;

namespace Owlcat.Runtime.Visual.XPBD.Layouts.MeshSkinning;

[CreateAssetMenu(fileName = "TriangleSkinmap", menuName = "XPBD/TriangleSkinmap", order = 123)]
public class TriangleSkinmap : ScriptableObject
{
	[Range(0f, 1f)]
	[HideInInspector]
	public float BarycentricWeight = 1f;

	[Range(0f, 1f)]
	[HideInInspector]
	public float NormalAlignmentWeight = 1f;

	[Range(0f, 1f)]
	[HideInInspector]
	public float ElevationWeight = 1f;

	[Min(0.001f)]
	[HideInInspector]
	public float AutoFillRadius = 0.25f;

	[HideInInspector]
	[SerializeField]
	private MeshLayout m_Master;

	[HideInInspector]
	[SerializeField]
	private Mesh m_Slave;

	public SkinTransform MasterTransform;

	public SkinTransform SlaveTransform;

	[SerializeField]
	private List<SlaveVertex> m_SkinnedVertices = new List<SlaveVertex>();

	public MeshLayout Master
	{
		get
		{
			return m_Master;
		}
		set
		{
			if (m_Master != value)
			{
				m_Master = value;
				MasterTransform = new SkinTransform(Vector3.zero, Quaternion.identity, Vector3.one);
			}
		}
	}

	public Mesh Slave
	{
		get
		{
			return m_Slave;
		}
		set
		{
			if (m_Slave != value)
			{
				m_Slave = value;
				SlaveTransform = new SkinTransform(Vector3.zero, Quaternion.identity, Vector3.one);
			}
		}
	}

	public List<SlaveVertex> SkinnedVertices => m_SkinnedVertices;

	public void SetSkinnedVertices(List<SlaveVertex> vertices)
	{
		m_SkinnedVertices = vertices;
	}

	public TriangleSkinmap GetCopy()
	{
		TriangleSkinmap triangleSkinmap = ScriptableObject.CreateInstance<TriangleSkinmap>();
		triangleSkinmap.BarycentricWeight = BarycentricWeight;
		triangleSkinmap.NormalAlignmentWeight = NormalAlignmentWeight;
		triangleSkinmap.ElevationWeight = ElevationWeight;
		triangleSkinmap.Master = m_Master;
		triangleSkinmap.Slave = m_Slave;
		triangleSkinmap.m_SkinnedVertices = m_SkinnedVertices;
		return triangleSkinmap;
	}
}
