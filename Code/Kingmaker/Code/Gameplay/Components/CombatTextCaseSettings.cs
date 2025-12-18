using System;
using Kingmaker.Localization;
using Kingmaker.ResourceLinks;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Components;

[Serializable]
public class CombatTextCaseSettings
{
	[Tooltip("Will be used for color")]
	public CombatTextMessageColorType MessageColorColorType;

	[Tooltip("If true — take icon from blueprint")]
	public bool UseAbilityIcon = true;

	[Tooltip("Override ability icon. No Icon if null")]
	[HideIf("UseAbilityIcon")]
	public SpriteLink Icon;

	[Tooltip("If true — take name of ability from blueprint")]
	public bool UseNameOfAbility = true;

	[Tooltip("Override text. No text if null")]
	[HideIf("UseNameOfAbility")]
	public LocalizedString Text;
}
