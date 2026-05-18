using System;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public struct AbilityTooltipConfig
{
	public Sprite MeleeAttackRateIcon;

	public Sprite RangedAttackRateIcon;

	public Sprite AttackDistanceIcon;

	public Sprite AbilityCooldownIcon;

	public Sprite AbilityRestrictionIcon;

	public Sprite AbilityPsykerIcon;

	public Sprite AbilityCantUseIcon;

	public Color DefaultColor;

	public Color RestrictionColor;

	public Color PsykerColor;
}
