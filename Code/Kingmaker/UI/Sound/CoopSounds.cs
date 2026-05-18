using System;
using Owlcat.Runtime.Core.Utility.Locator;
using UnityEngine;

namespace Kingmaker.UI.Sound;

[Serializable]
public class CoopSounds
{
	[Serializable]
	public class UISoundPings
	{
		[field: SerializeField]
		public UISound GroundPing { get; private set; }

		[field: SerializeField]
		public UISound MobPing { get; private set; }

		[field: SerializeField]
		public UISound DialogVotePing { get; private set; }

		[field: SerializeField]
		public UISound ActionBarAbilityPing { get; private set; }
	}

	public static CoopSounds Instance => Services.GetInstance<UISounds>().Sounds.CoopSounds;

	[field: SerializeField]
	public UISoundPings Pings { get; private set; }
}
