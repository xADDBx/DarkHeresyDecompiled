using System;
using Kingmaker.Blueprints.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints;

[Serializable]
[ComponentName("Root/DetectiveClueSignalRoot")]
[TypeId("8dad4186f4554a7ba1b61b98b9538bc3")]
public class DetectiveClueSignalRoot : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<DetectiveClueSignalRoot>
	{
	}

	public class DetectiveRadarRootReference : BlueprintReference<DetectiveClueSignalRoot>
	{
		public DetectiveRadarRootReference()
		{
			guid = "e77158264f52430cb60504dc8cd68db6";
		}
	}

	private static readonly DetectiveRadarRootReference s_Instance = new DetectiveRadarRootReference();

	public float MinimalDistance = 1f;

	public float MinFrequency;

	public float MaxFrequency;

	public AnimationCurve FrequencyCurve;

	public SignalUISettings UISettings;

	public static DetectiveClueSignalRoot Instance => s_Instance;

	public float GetFrequencyForSignalPower(float power)
	{
		return FrequencyCurve.Evaluate(power) * (MaxFrequency - MinFrequency) + MinFrequency;
	}
}
