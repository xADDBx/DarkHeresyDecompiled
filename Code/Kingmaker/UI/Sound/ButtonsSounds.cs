using System;
using Owlcat.Runtime.Core.Utility.Locator;
using UnityEngine;

namespace Kingmaker.UI.Sound;

[Serializable]
public class ButtonsSounds
{
	[Serializable]
	public class ButtonSoundPair
	{
		[field: SerializeField]
		public UISound Hover { get; private set; }

		[field: SerializeField]
		public UISound Click { get; private set; }
	}

	public static ButtonsSounds Instance => Services.GetInstance<UISounds>().Sounds.ButtonsSounds;

	[field: SerializeField]
	public ButtonSoundPair Default { get; private set; }

	[field: SerializeField]
	public ButtonSoundPair PlasticButton { get; private set; }

	[field: SerializeField]
	public ButtonSoundPair ExitToWarpButton { get; private set; }

	[field: SerializeField]
	public ButtonSoundPair FinishChargenButton { get; private set; }

	[field: SerializeField]
	public ButtonSoundPair LootCollectAllButton { get; private set; }

	[field: SerializeField]
	public ButtonSoundPair DoctrineNextButton { get; private set; }

	[field: SerializeField]
	public ButtonSoundPair PaperComponentSound { get; private set; }

	[field: SerializeField]
	public ButtonSoundPair AnalogButton { get; private set; }

	[field: SerializeField]
	public ButtonSoundPair PaperButton { get; private set; }

	[field: SerializeField]
	public ButtonSoundPair MajorPaperButton { get; private set; }
}
