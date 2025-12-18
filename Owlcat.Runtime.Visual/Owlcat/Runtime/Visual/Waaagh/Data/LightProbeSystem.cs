using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.Data;

public enum LightProbeSystem
{
	[InspectorName("Light Probe Groups")]
	LegacyLightProbes,
	[InspectorName("Adaptive Probe Volumes")]
	ProbeVolumes
}
