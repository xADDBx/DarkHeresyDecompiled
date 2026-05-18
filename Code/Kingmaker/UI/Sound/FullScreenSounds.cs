using System;
using Owlcat.Runtime.Core.Utility.Locator;
using UnityEngine;

namespace Kingmaker.UI.Sound;

[Serializable]
public class FullScreenSounds
{
	[Serializable]
	public class UISoundDialogue
	{
		[field: SerializeField]
		public UISound Open { get; private set; }

		[field: SerializeField]
		public UISound Close { get; private set; }

		[field: SerializeField]
		public UISound BookPageTurn { get; private set; }

		[field: SerializeField]
		public UISound BookOpen { get; private set; }

		[field: SerializeField]
		public UISound BookClose { get; private set; }
	}

	[Serializable]
	public class UISoundChargen
	{
		[field: SerializeField]
		public UISound ChargenPortraitChange { get; private set; }

		[field: SerializeField]
		public UISound ChargenCompleteClick { get; private set; }

		[field: SerializeField]
		public UISound ChargenTabSwitch { get; private set; }
	}

	[Serializable]
	public class UISoundFormation
	{
		[field: SerializeField]
		public UISound Open { get; private set; }

		[field: SerializeField]
		public UISound Close { get; private set; }
	}

	[Serializable]
	public class UISoundVendor : ServiceWindowUISound
	{
		[field: SerializeField]
		public UISound MoveFromVendorToTrade { get; private set; }

		[field: SerializeField]
		public UISound DealButtonNormal { get; private set; }

		[field: SerializeField]
		public UISound DealButtonSell { get; private set; }

		[field: SerializeField]
		public UISound DealButtonBuy { get; private set; }
	}

	public static FullScreenSounds Instance => Services.GetInstance<UISounds>().Sounds.FullScreenSounds;

	[field: SerializeField]
	public UISoundDialogue Dialogue { get; private set; }

	[field: SerializeField]
	public UISoundChargen Chargen { get; private set; }

	[field: SerializeField]
	public UISoundFormation Formation { get; private set; }

	[field: SerializeField]
	public UISoundVendor Vendor { get; private set; }
}
