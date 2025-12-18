using JetBrains.Annotations;
using Owlcat.Runtime.Visual.Experimental.Geometry;
using UnityEngine;

namespace Owlcat.Runtime.Visual.OcclusionClipping;

[DefaultExecutionOrder(103)]
[DisallowMultipleComponent]
internal abstract class ClippingRendererBase : MonoBehaviour, IClippingRenderer
{
	[UsedImplicitly]
	protected virtual void OnEnable()
	{
		Transform transform = base.transform;
		ClippingSystem.CreateRenderer(GetInstanceID(), new Obb(transform.position, transform.lossyScale * 0.5f, transform.rotation), this);
	}

	[UsedImplicitly]
	protected virtual void OnDisable()
	{
		ClippingSystem.DestroyRenderer(GetInstanceID());
	}

	public void SetOpacity(float newOpacity)
	{
		OnOpacityChanged(newOpacity);
	}

	protected abstract void OnOpacityChanged(float opacity);
}
