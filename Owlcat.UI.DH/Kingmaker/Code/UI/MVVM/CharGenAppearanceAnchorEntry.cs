using System;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Localization;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

[Serializable]
public class CharGenAppearanceAnchorEntry
{
	public CharGenAppearancePageType PageType;

	public CharGenAppearancePageComponent ScrollTo;

	public Sprite Icon;

	public LocalizedString HintString;
}
