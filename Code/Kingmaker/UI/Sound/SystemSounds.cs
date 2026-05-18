using System;
using System.Collections.Generic;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Owlcat.Runtime.Core.Utility.Locator;
using UnityEngine;

namespace Kingmaker.UI.Sound;

[Serializable]
public class SystemSounds
{
	[Serializable]
	public class UISoundCommonControls
	{
		[field: SerializeField]
		public UISound ZoomInButton { get; private set; }

		[field: SerializeField]
		public UISound ZoomOutButton { get; private set; }

		[field: SerializeField]
		public UISound ResetZoomAndPositionButton { get; private set; }

		[field: SerializeField]
		public UISound ZoomInScroll { get; private set; }

		[field: SerializeField]
		public UISound ZoomOutScroll { get; private set; }

		[field: SerializeField]
		public UISound ZoomInWheel { get; private set; }

		[field: SerializeField]
		public UISound ZoomOutWheel { get; private set; }
	}

	[Serializable]
	public class UISoundSystems
	{
		[field: SerializeField]
		public UISound PauseSound { get; private set; }

		[field: SerializeField]
		public UISound BlinkAttentionMark { get; private set; }
	}

	[Serializable]
	public class UISoundConsoleHints
	{
		[field: SerializeField]
		public UISound Click { get; private set; }

		[field: SerializeField]
		public UISound HoldStart { get; private set; }

		[field: SerializeField]
		public UISound HoldStop { get; private set; }
	}

	[Serializable]
	public class UISoundSelector
	{
		[field: SerializeField]
		public UISound Start { get; private set; }

		[field: SerializeField]
		public UISound LoopStart { get; private set; }

		[field: SerializeField]
		public UISound LoopStop { get; private set; }

		[field: SerializeField]
		public UISound Stop { get; private set; }
	}

	[Serializable]
	public class UISoundPantograph
	{
		[field: SerializeField]
		public UISound Start { get; private set; }

		[field: SerializeField]
		public UISound LoopStart { get; private set; }

		[field: SerializeField]
		public UISound LoopStop { get; private set; }

		[field: SerializeField]
		public UISound Stop { get; private set; }
	}

	[Serializable]
	public class UISoundHint
	{
		[field: SerializeField]
		public UISound Show { get; private set; }

		[field: SerializeField]
		public UISound Hide { get; private set; }
	}

	[Serializable]
	public class UISoundScrambledText
	{
		[field: SerializeField]
		public UISound LoopStart { get; private set; }

		[field: SerializeField]
		public UISound LoopStop { get; private set; }
	}

	[Serializable]
	public class UISoundDropdownMenu
	{
		[field: SerializeField]
		public UISound Show { get; private set; }

		[field: SerializeField]
		public UISound Hide { get; private set; }
	}

	[Serializable]
	public class UIInteractionSounds
	{
		[field: SerializeField]
		public UISound Open { get; private set; }

		[field: SerializeField]
		public UISound Close { get; private set; }

		[field: SerializeField]
		public List<InteractionSoundEntry> Interactions { get; private set; }

		public UISound GetInteractionSound(UIInteractionType type, bool isSuccess)
		{
			foreach (InteractionSoundEntry interaction in Interactions)
			{
				if (interaction.Type == type)
				{
					return isSuccess ? interaction.SoundSuccess : interaction.SoundFail;
				}
			}
			return null;
		}
	}

	[Serializable]
	public class InteractionSoundEntry
	{
		[field: SerializeField]
		public UIInteractionType Type { get; private set; }

		[field: SerializeField]
		public UISound SoundSuccess { get; private set; }

		[field: SerializeField]
		public UISound SoundFail { get; private set; }
	}

	public static SystemSounds Instance => Services.GetInstance<UISounds>().Sounds.SystemSounds;

	[field: SerializeField]
	public UISoundCommonControls Controls { get; private set; }

	[field: SerializeField]
	public UISoundSystems Systems { get; private set; }

	[field: SerializeField]
	public UISoundSelector Selector { get; private set; }

	[field: SerializeField]
	public UISoundConsoleHints ConsoleHints { get; private set; }

	[field: SerializeField]
	public UISoundPantograph Pantograph { get; private set; }

	[field: SerializeField]
	public UISoundHint Hint { get; private set; }

	[field: SerializeField]
	public UISoundScrambledText ScrambledText { get; private set; }

	[field: SerializeField]
	public UISoundDropdownMenu DropdownMenu { get; private set; }

	[field: SerializeField]
	public UIInteractionSounds InteractionSounds { get; private set; }
}
