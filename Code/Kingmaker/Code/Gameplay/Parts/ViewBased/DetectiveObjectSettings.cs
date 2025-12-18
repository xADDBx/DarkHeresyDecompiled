using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.Gameplay.Parts.ViewBased;

[Serializable]
public class DetectiveObjectSettings
{
	public enum LocalMapMarkerType
	{
		None,
		Lens,
		Traces
	}

	[FormerlySerializedAs("DefaultState")]
	public bool DefaultRevealedState;

	public LocalMapMarkerType LocalMapMarker;

	public GameObject GroundHighlight;
}
