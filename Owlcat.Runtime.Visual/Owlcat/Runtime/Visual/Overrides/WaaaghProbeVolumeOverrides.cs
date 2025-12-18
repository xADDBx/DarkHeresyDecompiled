using System;
using Owlcat.Runtime.Visual.Waaagh.Data;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Overrides;

[Serializable]
[VolumeComponentMenu("Lighting/Waaagh/Adaptive Probe Volumes Overrides")]
[SupportedOnRenderPipeline(typeof(WaaaghPipelineAsset))]
public sealed class WaaaghProbeVolumeOverrides : VolumeComponent
{
	public ClampedFloatParameter IntensityMultiplier = new ClampedFloatParameter(1f, 0f, 10f);

	public WaaaghProbeVolumeOverrides()
	{
		base.displayName = "Waaagh Adaptive Probe Volumes Overrides";
	}
}
