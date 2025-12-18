using UnityEngine;

namespace Kingmaker.GPUCrowd;

[ExecuteInEditMode]
public class GpuCrowdLocator : MonoBehaviour
{
	public GpuCrowd gpuCrowd;

	public bool DrawGizmos;

	public Vector3 InitPosition;

	public bool RealtimeUpdate;

	[Space(4f)]
	public bool ConsiderInSoundComputation = true;
}
