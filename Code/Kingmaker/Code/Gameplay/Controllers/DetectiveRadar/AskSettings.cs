using System;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Controllers.DetectiveRadar;

[Serializable]
public class AskSettings
{
	public ProximityAskType AskType;

	[Tooltip("If anyone from party closer than that - trigger Ask")]
	public float DistanceToAsk = 2f;

	[Tooltip("Triggering once")]
	public bool Once;

	[Tooltip("Triggering once")]
	public bool HasCooldown;

	[ShowIf("HasCooldown")]
	[Tooltip("Can be triggered only after 'Cooldown' seconds passed")]
	public float Cooldown = 2f;
}
