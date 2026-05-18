using System;
using Kingmaker.Localization;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UIMoralePressureTooltip
{
	public LocalizedString Title;

	public LocalizedString GeneralDescription;

	public LocalizedString CurrentMoralePressure;

	public LocalizedString ThresholdReached;

	public LocalizedString LowPressureDescription;

	public LocalizedString HighPressureDescription;

	public LocalizedString SuppressedMoraleDescription;
}
