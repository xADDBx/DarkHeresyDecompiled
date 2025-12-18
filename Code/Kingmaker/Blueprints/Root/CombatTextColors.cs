using System;
using Kingmaker.Code.Gameplay.Components;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class CombatTextColors
{
	public Color Default = Color.white;

	public Color HealColor = Color.green;

	public Color DamageColor = Color.red;

	[Header("Negative Effect Colors")]
	public Color PositiveLow = Color.white;

	public Color PositiveMiddle = Color.yellow;

	public Color PositiveHigh = Color.red;

	[Header("Negative Effect Colors")]
	public Color NegativeLow = Color.white;

	public Color NegativeMiddle = Color.yellow;

	public Color NegativeHigh = Color.red;

	[Header("Neutral Effect Colors")]
	public Color NeutralLow = Color.white;

	public Color NeutralMiddle = Color.white;

	public Color NeutralHigh = Color.white;

	[Header("MoralePhase")]
	public Color HeroicPhaseColor = Color.white;

	public Color BrokenPhaseColor = Color.white;

	public Color GetColor(CombatTextMessageColorType messageType)
	{
		return messageType switch
		{
			CombatTextMessageColorType.Default => Default, 
			CombatTextMessageColorType.DefaultHeal => HealColor, 
			CombatTextMessageColorType.DefaultDamage => DamageColor, 
			CombatTextMessageColorType.PositiveLow => PositiveLow, 
			CombatTextMessageColorType.PositiveMiddle => PositiveMiddle, 
			CombatTextMessageColorType.PositiveHigh => PositiveHigh, 
			CombatTextMessageColorType.NegativeLow => NegativeLow, 
			CombatTextMessageColorType.NegativeMiddle => NegativeMiddle, 
			CombatTextMessageColorType.NegativeHigh => NegativeHigh, 
			CombatTextMessageColorType.NeutralLow => NeutralLow, 
			CombatTextMessageColorType.NeutralMiddle => NeutralMiddle, 
			CombatTextMessageColorType.NeutralHigh => NeutralHigh, 
			_ => Default, 
		};
	}
}
