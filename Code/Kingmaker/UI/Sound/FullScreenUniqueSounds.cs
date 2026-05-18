using System;
using Owlcat.Runtime.Core.Utility.Locator;
using UnityEngine;

namespace Kingmaker.UI.Sound;

[Serializable]
public class FullScreenUniqueSounds
{
	[Serializable]
	public class UISoundSettings
	{
		[field: SerializeField]
		public UISound Open { get; private set; }

		[field: SerializeField]
		public UISound Close { get; private set; }

		[field: SerializeField]
		public UISound KeyBindingOpen { get; private set; }

		[field: SerializeField]
		public UISound KeyBindingClose { get; private set; }

		[field: SerializeField]
		public UISound SliderMove { get; private set; }
	}

	[Serializable]
	public class UISoundLoadingScreen
	{
		[field: SerializeField]
		public UISound FinishGlitch { get; private set; }

		[field: SerializeField]
		public UISound WaitForUserInputShow { get; private set; }

		[field: SerializeField]
		public UISound WaitForUserInputHide { get; private set; }
	}

	[Serializable]
	public class UISoundMainMenu
	{
		[field: SerializeField]
		public UISound ButtonsFirstLaunchFxAnimation { get; private set; }

		[field: SerializeField]
		public UISound ButtonsFxAnimation { get; private set; }

		[field: SerializeField]
		public UISound MessageOfTheDayShow { get; private set; }
	}

	[Serializable]
	public class UISoundEscMenu
	{
		[field: SerializeField]
		public UISound Open { get; private set; }

		[field: SerializeField]
		public UISound Close { get; private set; }
	}

	public static FullScreenUniqueSounds Instance => Services.GetInstance<UISounds>().Sounds.FullScreenUniqueSounds;

	[field: SerializeField]
	public UISoundSettings Settings { get; private set; }

	[field: SerializeField]
	public UISoundLoadingScreen LoadingScreen { get; private set; }

	[field: SerializeField]
	public UISoundMainMenu MainMenu { get; private set; }

	[field: SerializeField]
	public UISoundEscMenu EscMenu { get; private set; }
}
