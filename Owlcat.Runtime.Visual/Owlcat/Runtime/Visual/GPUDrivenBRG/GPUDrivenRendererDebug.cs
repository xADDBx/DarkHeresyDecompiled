using Owlcat.Runtime.Core.ObjectTracking;
using UnityEngine;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

[ExecuteAlways]
public class GPUDrivenRendererDebug : MonoBehaviour
{
	public bool OccludeOnly;

	private void OnEnable()
	{
		MarkRendererDirty();
	}

	private void OnDisable()
	{
		MarkRendererDirty();
	}

	private void OnValidate()
	{
		MarkRendererDirty();
	}

	private void MarkRendererDirty()
	{
		if (TryGetComponent<MeshRenderer>(out var component))
		{
			UnityObjectUtils.MarkDirty(component);
		}
	}
}
