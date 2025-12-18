using JetBrains.Annotations;
using Owlcat.Runtime.Visual.Experimental.Geometry;
using UnityEngine;

namespace Owlcat.Runtime.Visual.OcclusionClipping;

[DisallowMultipleComponent]
[DefaultExecutionOrder(101)]
internal sealed class ClippingTrigger : MonoBehaviour
{
	[UsedImplicitly]
	private void OnEnable()
	{
		Transform transform = base.transform;
		ClippingSystem.CreateTrigger(GetInstanceID(), new Obb(transform.position, transform.lossyScale * 0.5f, transform.rotation));
	}

	[UsedImplicitly]
	private void OnDisable()
	{
		ClippingSystem.DestroyTrigger(GetInstanceID());
	}
}
