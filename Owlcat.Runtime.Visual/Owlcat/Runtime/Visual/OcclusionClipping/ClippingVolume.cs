using JetBrains.Annotations;
using Owlcat.Runtime.Visual.Experimental.Geometry;
using UnityEngine;

namespace Owlcat.Runtime.Visual.OcclusionClipping;

[DisallowMultipleComponent]
[DefaultExecutionOrder(102)]
internal sealed class ClippingVolume : MonoBehaviour
{
	[UsedImplicitly]
	private void OnEnable()
	{
		Transform transform = base.transform;
		ClippingSystem.CreateVolume(GetInstanceID(), new Obb(transform.position, transform.lossyScale * 0.5f, transform.rotation));
	}

	[UsedImplicitly]
	private void OnDisable()
	{
		ClippingSystem.DestroyVolume(GetInstanceID());
	}
}
