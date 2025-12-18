using UnityEngine;

namespace Kingmaker.Visual.OcclusionGeometryClip;

public abstract class OcclusionGeometryClipLinkProxy : MonoBehaviour
{
	internal OcclusionGeometryClipLinkVolumeProxy LinkedVolume;

	public abstract void SetOpacity(float value);

	private void Start()
	{
		OcclusionGeometryClipLinkSystem.AddObject(this);
	}

	private void OnDestroy()
	{
		OcclusionGeometryClipLinkSystem.RemoveObject(this);
	}
}
