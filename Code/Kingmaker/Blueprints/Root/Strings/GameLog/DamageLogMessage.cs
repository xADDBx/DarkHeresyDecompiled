using System;
using Kingmaker.Localization;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Strings.GameLog;

[Serializable]
[Obsolete]
public class DamageLogMessage
{
	public Color32 Color;

	public LocalizedString Message;

	public LocalizedString MessageFailedCheck;

	public LocalizedString MessageUnknownSource;

	public LocalizedString MessageCollision;

	public LocalizedString Tooltip;

	public LocalizedString TooltipSource;

	public LocalizedString TooltipSneak;

	public LocalizedString TooltipDifficulty;

	[Tooltip("Damage details in tooltip")]
	public LocalizedString DamageImmune;
}
