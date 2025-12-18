using System;
using UnityEngine;

namespace Kingmaker.GPUCrowd;

[Serializable]
public struct GpuCrowdSoundInfo
{
	public Vector3 Position;

	public bool ConsiderInSoundComputation;
}
