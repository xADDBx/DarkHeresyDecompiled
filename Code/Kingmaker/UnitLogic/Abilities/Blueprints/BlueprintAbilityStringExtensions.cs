using System.Linq;
using System.Text;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Settings;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Mechanics.Actions;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Blueprints;

public static class BlueprintAbilityStringExtensions
{
	public static string GetShortenedDescription(this BlueprintAbility ability)
	{
		if (!SettingsRoot.Game.Tooltips.Shortened)
		{
			return ability.Description;
		}
		return UtilityAbilities.GetLongOrShortText(ability.Description, state: false);
	}

	public static string GetTarget(this BlueprintAbility ability, int weaponRange = -1, MechanicEntity caster = null)
	{
		AbilityTargetStrings abilityTargets = LocalizedTexts.Instance.AbilityTargets;
		AbilityRangeStrings abilityTargetRanges = LocalizedTexts.Instance.AbilityTargetRanges;
		StringBuilder stringBuilder = new StringBuilder();
		if (ability.IsStratagem)
		{
			switch (ability.GetTooltipHelper()?.TargetType ?? TargetType.Any)
			{
			case TargetType.Ally:
				stringBuilder.Append(abilityTargets.AllAllies);
				break;
			case TargetType.Enemy:
				stringBuilder.Append(abilityTargets.AllEnemies);
				break;
			default:
				stringBuilder.Append(abilityTargets.AllCreatures);
				break;
			}
			stringBuilder.Append(' ');
			stringBuilder.Append(abilityTargets.InsideSelectedCombatArea);
		}
		else if (ability.IsCharge)
		{
			stringBuilder.Append(abilityTargets.FirstCreature);
			stringBuilder.Append(' ');
			stringBuilder.Append(string.Format(abilityTargets.WithinLine, ability.GetRange()));
			stringBuilder.Append(' ');
		}
		else if (ability.IsMoveUnit)
		{
			stringBuilder.Append(abilityTargets.Movement);
			if (ability.IsRangeCustom && ability.CustomRange > 0)
			{
				stringBuilder.Append(' ');
				stringBuilder.Append(string.Format(abilityTargetRanges.GetText(AbilityRange.Custom), ability.CustomRange));
			}
		}
		else if (ability.Range == AbilityRange.Personal && ability.PatternSettings == null)
		{
			switch (ability.GetTooltipHelper()?.TargetType)
			{
			case TargetType.Ally:
				stringBuilder.Append(abilityTargets.AllAllies);
				break;
			case TargetType.Enemy:
				stringBuilder.Append(abilityTargets.AllEnemies);
				break;
			default:
				stringBuilder.Append(abilityTargets.AllCreatures);
				break;
			case null:
				stringBuilder.Append(abilityTargets.Personal);
				break;
			}
		}
		else if (ability.Range != 0 && ability.PatternSettings == null)
		{
			if (ability.CanTargetPoint)
			{
				stringBuilder.Append(abilityTargets.TargetPoint);
			}
			else
			{
				switch (ability.GetTooltipHelper()?.TargetType)
				{
				case TargetType.Ally:
					stringBuilder.Append(abilityTargets.AllAllies);
					break;
				case TargetType.Enemy:
					stringBuilder.Append(abilityTargets.AllEnemies);
					break;
				default:
					stringBuilder.Append(abilityTargets.AllCreatures);
					break;
				case null:
					if (ability.CanTargetEnemies && ability.CanTargetFriends)
					{
						stringBuilder.Append(abilityTargets.OneCreature);
					}
					else if (ability.CanTargetEnemies && !ability.CanTargetFriends)
					{
						stringBuilder.Append(abilityTargets.OneEnemyCreature);
					}
					else if (!ability.CanTargetEnemies && ability.CanTargetFriends)
					{
						stringBuilder.Append(abilityTargets.OneFriendlyCreature);
					}
					else
					{
						if (!ability.CanTargetSelf)
						{
							break;
						}
						ContextActionOnAllUnitsInCombat contextActionOnAllUnitsInCombat = ability.GetContextActionOnAllUnitsInCombat();
						if (contextActionOnAllUnitsInCombat == null)
						{
							stringBuilder.Append(abilityTargets.Personal);
							stringBuilder.Append(".\n");
							return stringBuilder.ToString();
						}
						if (contextActionOnAllUnitsInCombat.OnlyAllies)
						{
							stringBuilder.Append(abilityTargets.AllAllies);
							break;
						}
						if (!contextActionOnAllUnitsInCombat.OnlyEnemies)
						{
							stringBuilder.Append(abilityTargets.Personal);
							stringBuilder.Append(".\n");
							return stringBuilder.ToString();
						}
						stringBuilder.Append(abilityTargets.AllEnemies);
					}
					break;
				}
			}
			stringBuilder.Append(' ');
			if (ability.IsRangeCustom || !abilityTargetRanges.Contains(ability.Range))
			{
				int num;
				if (caster != null)
				{
					RuleCalculateAbilityRange ruleCalculateAbilityRange = Rulebook.Trigger(new RuleCalculateAbilityRange(caster, new AbilityData(ability, caster)));
					num = ruleCalculateAbilityRange.OverrideRange ?? ruleCalculateAbilityRange.DefaultRange;
				}
				else
				{
					num = ability.GetRange();
				}
				stringBuilder.Append(string.Format(abilityTargetRanges.GetText(AbilityRange.Custom), num));
			}
			else if (ability.IsRangeWeapon && weaponRange > 0)
			{
				stringBuilder.Append(string.Format(abilityTargetRanges.GetText(AbilityRange.Custom), weaponRange));
			}
			else
			{
				stringBuilder.Append(string.Format(abilityTargetRanges.GetText(ability.Range)));
			}
		}
		else if (ability.IsBurst)
		{
			if (ability.CanTargetEnemies && ability.CanTargetFriends)
			{
				stringBuilder.Append(abilityTargets.FirstCreature);
			}
			else if (ability.CanTargetEnemies && !ability.CanTargetFriends)
			{
				stringBuilder.Append(abilityTargets.FirstEnemyCreature);
			}
			else if (!ability.CanTargetEnemies && ability.CanTargetFriends)
			{
				stringBuilder.Append(abilityTargets.FirstFriendlyCreature);
			}
			stringBuilder.Append(' ');
			stringBuilder.Append(abilityTargets.EveryShot);
			stringBuilder.Append(' ');
			stringBuilder.Append(string.Format(abilityTargets.WithinCone, weaponRange));
		}
		else if (ability.PatternSettings != null)
		{
			if (ability.AoETargets == TargetType.Any)
			{
				stringBuilder.Append(abilityTargets.AllCreatures);
			}
			else if (ability.AoETargets == TargetType.Enemy)
			{
				stringBuilder.Append(abilityTargets.AllEnemies);
			}
			else if (ability.AoETargets == TargetType.Ally)
			{
				stringBuilder.Append(abilityTargets.AllAllies);
			}
			stringBuilder.Append(' ');
			stringBuilder.Append(abilityTargets.InsideAreaOfEffect);
			stringBuilder.Append(' ');
			if (ability.Range == AbilityRange.Personal)
			{
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
			}
			else if (ability.IsRangeCustom)
			{
				stringBuilder.Append(string.Format(abilityTargetRanges.GetText(AbilityRange.Custom), ability.CustomRange));
			}
			else if (ability.IsRangeWeapon)
			{
				stringBuilder.Append(string.Format(abilityTargetRanges.GetText(AbilityRange.Custom), weaponRange));
			}
			else
			{
				stringBuilder.Append(abilityTargetRanges.GetText(ability.Range));
			}
		}
		stringBuilder.Append(".\n");
		return stringBuilder.ToString();
	}

	private static ContextActionOnAllUnitsInCombat GetContextActionOnAllUnitsInCombat(this BlueprintAbility ability)
	{
		return (from c in ability.ComponentsArray
			select c as AbilityEffectRunAction into c
			where c != null
			select c).SelectMany((AbilityEffectRunAction a) => a.Actions.Actions).FirstOrDefault((GameAction a) => a is ContextActionOnAllUnitsInCombat) as ContextActionOnAllUnitsInCombat;
	}

	public static Sprite GetTargetImage(this BlueprintAbility ability)
	{
		UIIcons uIIcons = ConfigRoot.Instance.UIConfig.UIIcons;
		switch (ability.GetTooltipHelper()?.TargetType)
		{
		case TargetType.Enemy:
			return uIIcons.TargetEnemyAll;
		case TargetType.Ally:
			return uIIcons.TargetAllyAll;
		default:
			return uIIcons.TargetAnyAll;
		case null:
			if (ability.PatternSettings != null)
			{
				if (ability.IsMoveUnit)
				{
					return uIIcons.TargetCharge;
				}
				if (ability.AoETargets == TargetType.Any)
				{
					return uIIcons.TargetAnyAll;
				}
				if (ability.AoETargets == TargetType.Enemy)
				{
					return uIIcons.TargetEnemyAll;
				}
				if (ability.AoETargets == TargetType.Ally)
				{
					return uIIcons.TargetAllyAll;
				}
				return null;
			}
			if (ability.Range == AbilityRange.Personal)
			{
				return uIIcons.TargetPersonal;
			}
			if (ability.CanTargetPoint)
			{
				return uIIcons.SpellTargetPoint;
			}
			if (ability.CanTargetEnemies && ability.CanTargetFriends)
			{
				return uIIcons.TargetAnyOne;
			}
			if (ability.CanTargetEnemies && !ability.CanTargetFriends)
			{
				return uIIcons.TargetEnemyOne;
			}
			if (!ability.CanTargetEnemies && ability.CanTargetFriends)
			{
				return uIIcons.TargetAllyOne;
			}
			if (ability.CanTargetSelf)
			{
				ContextActionOnAllUnitsInCombat contextActionOnAllUnitsInCombat = ability.GetContextActionOnAllUnitsInCombat();
				if (contextActionOnAllUnitsInCombat != null)
				{
					if (contextActionOnAllUnitsInCombat.OnlyAllies)
					{
						return uIIcons.TargetAllyAll;
					}
					if (contextActionOnAllUnitsInCombat.OnlyEnemies)
					{
						return uIIcons.TargetEnemyAll;
					}
				}
				return uIIcons.TargetPersonal;
			}
			return null;
		}
	}

	private static AbilityTooltipHelper GetTooltipHelper(this BlueprintAbility ability)
	{
		return ability.GetComponent<AbilityTooltipHelper>();
	}
}
