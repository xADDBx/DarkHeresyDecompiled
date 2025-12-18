using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.OcclusionClipping;

[DisallowMultipleComponent]
[DefaultExecutionOrder(101)]
internal sealed class ClippingProbe : MonoBehaviour, IClippingProbe
{
	[SerializeField]
	[Min(0.1f)]
	private float m_Radius = 1f;

	public float4 BoundingSphere => new float4(base.transform.position, m_Radius);

	[UsedImplicitly]
	private void OnEnable()
	{
		ClippingSystem.CreateProbe(GetInstanceID(), this);
	}

	[UsedImplicitly]
	private void OnDisable()
	{
		ClippingSystem.DestroyProbe(GetInstanceID());
	}

	private void OnDrawGizmos()
	{
		Transform obj = base.transform;
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(obj.position, m_Radius);
	}
}
