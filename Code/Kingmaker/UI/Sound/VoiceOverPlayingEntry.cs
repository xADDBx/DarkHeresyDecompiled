using System;
using Kingmaker.UI.Sound.Base;
using UnityEngine;

namespace Kingmaker.UI.Sound;

[Serializable]
public class VoiceOverPlayingEntry
{
	[Serializable]
	public class StaticData
	{
		public VoiceOverType Type;

		public string VoId;

		public string EventId;

		public GameObject Target;
	}

	public StaticData Data;

	public VoiceOverStatus Status;
}
