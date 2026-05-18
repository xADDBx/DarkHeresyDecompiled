using System;
using Kingmaker.ElementsSystem;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;

namespace Kingmaker.View.MapObjects.InteractionRestrictions;

[Serializable]
public class ConditionalRestrictionSettings
{
	public ConditionsReference Condition;

	public bool HideBark;

	[StringCreateWindow(StringCreateWindowAttribute.StringType.Bark)]
	public LocalizedString RestrictedBark;

	[StringCreateWindow(StringCreateWindowAttribute.StringType.Bark)]
	public LocalizedString AllowedBark;
}
