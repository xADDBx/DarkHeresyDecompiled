using UnityEngine;
using UnityEngine.Polybrush;

namespace Owlcat.Runtime.Visual.VertexPaint.PolybrushUtils;

[ExecuteAlways]
public sealed class TransientPolybrushMesh : MonoBehaviour, ISerializationCallbackReceiver
{
	private PolybrushMesh m_Mesh;

	internal PolybrushMesh Mesh => m_Mesh;

	private void OnEnable()
	{
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
	}
}
