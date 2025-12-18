using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Levelup.Selections.Prerequisites;
using Kingmaker.UnitLogic.Progression;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.Utility.StatefulRandom;
using R3;
using UnityEngine;

namespace Kingmaker.UI.UIUtils;

public static class UIUtilityAbilities
{
	public static string GetPrerequisiteFactName(CalculatedPrerequisiteFact prerequisiteFact)
	{
		BlueprintUnitFact fact = prerequisiteFact.Fact;
		LocalizedString localizedString;
		if (!(fact is BlueprintAbility))
		{
			if (!(fact is BlueprintCareerPath))
			{
				if (!(fact is BlueprintFeature))
				{
					throw new ArgumentOutOfRangeException();
				}
				localizedString = UIStrings.Instance.Tooltips.PrerequisiteFeatures;
			}
			else
			{
				localizedString = UIStrings.Instance.Tooltips.PrerequisiteCareers;
			}
		}
		else
		{
			localizedString = UIStrings.Instance.Tooltips.PrerequisiteAbilities;
		}
		return localizedString;
	}

	public static string GetFactName(CalculatedPrerequisiteFact prerequisiteFact)
	{
		string text = "<b>" + prerequisiteFact.Fact.Name + "</b>";
		if (prerequisiteFact.Fact is BlueprintFeature blueprintFeature)
		{
			text = "<link=\"Highlight:" + blueprintFeature.AssetGuid + "\">" + text + "</link>";
		}
		return text;
	}

	public static PrerequisiteEntryVM UnpackPrerequisiteComposite(CalculatedPrerequisiteComposite prerequisiteComposite)
	{
		StringBuilder stringBuilder = new StringBuilder(GetPrerequisiteFactName(prerequisiteComposite.Prerequisites.First() as CalculatedPrerequisiteFact) + ".");
		string separator = ((prerequisiteComposite.Composition == FeaturePrerequisiteComposition.Or) ? (" " + UIStrings.Instance.Tooltips.or.Text + " ") : (" " + UIStrings.Instance.Tooltips.and.Text + " "));
		List<string> list = new List<string>();
		foreach (CalculatedPrerequisite prerequisite in prerequisiteComposite.Prerequisites)
		{
			if (prerequisite is CalculatedPrerequisiteFact calculatedPrerequisiteFact)
			{
				if (!calculatedPrerequisiteFact.IsDlcRestrictedContent)
				{
					list.Add(GetFactName(calculatedPrerequisiteFact));
				}
				continue;
			}
			throw new ArgumentOutOfRangeException();
		}
		stringBuilder.Append(string.Join(separator, list));
		stringBuilder.Append(".");
		return new PrerequisiteEntryVM(stringBuilder.ToString(), prerequisiteComposite.Value, prerequisiteComposite.Not);
	}

	public static bool CanUnpackComposite(CalculatedPrerequisiteComposite prerequisiteComposite)
	{
		return prerequisiteComposite.Prerequisites.All((CalculatedPrerequisite i) => i is CalculatedPrerequisiteFact);
	}

	public static List<PrerequisiteEntryVM> GetPrerequisiteEntries(CalculatedPrerequisite prerequisite)
	{
		List<PrerequisiteEntryVM> list = new List<PrerequisiteEntryVM>();
		if (!(prerequisite is CalculatedPrerequisiteFact calculatedPrerequisiteFact))
		{
			if (!(prerequisite is CalculatedPrerequisiteComposite calculatedPrerequisiteComposite))
			{
				if (!(prerequisite is CalculatedPrerequisiteStat calculatedPrerequisiteStat))
				{
					if (!(prerequisite is CalculatedPrerequisiteMaxRankNotReached calculatedPrerequisiteMaxRankNotReached))
					{
						if (prerequisite is CalculatedPrerequisiteLevel calculatedPrerequisiteLevel)
						{
							string text = UIStrings.Instance.Tooltips.PrerequisiteLevel;
							bool value = calculatedPrerequisiteLevel.Value;
							bool not = calculatedPrerequisiteLevel.Not;
							int level = calculatedPrerequisiteLevel.Level;
							list.Add(new PrerequisiteEntryVM(text, value, not, level.ToString()));
						}
					}
					else
					{
						list.Add(new PrerequisiteEntryVM(UIStrings.Instance.Tooltips.PrerequisiteRank, calculatedPrerequisiteMaxRankNotReached.Value, calculatedPrerequisiteMaxRankNotReached.Not));
					}
				}
				else
				{
					list.Add(new PrerequisiteEntryVM(LocalizedTexts.Instance.Stats.GetText(calculatedPrerequisiteStat.Stat), calculatedPrerequisiteStat.Value, calculatedPrerequisiteStat.Not, calculatedPrerequisiteStat.MinValue.ToString()));
				}
			}
			else if (CanUnpackComposite(calculatedPrerequisiteComposite))
			{
				list.Add(UnpackPrerequisiteComposite(calculatedPrerequisiteComposite));
			}
			else
			{
				foreach (CalculatedPrerequisite item in calculatedPrerequisiteComposite.Prerequisites.Where((CalculatedPrerequisite i) => !(i is CalculatedPrerequisiteComposite)))
				{
					list.AddRange(GetPrerequisiteEntries(item));
				}
				foreach (CalculatedPrerequisite item2 in calculatedPrerequisiteComposite.Prerequisites.Where((CalculatedPrerequisite i) => i is CalculatedPrerequisiteComposite))
				{
					list.Add(new PrerequisiteEntryVM(GetPrerequisiteEntries(item2), calculatedPrerequisiteComposite.Composition == FeaturePrerequisiteComposition.Or));
				}
			}
		}
		else if (!calculatedPrerequisiteFact.IsDlcRestrictedContent)
		{
			string text2 = GetPrerequisiteFactName(calculatedPrerequisiteFact) + " " + GetFactName(calculatedPrerequisiteFact) + ".";
			list.Add(new PrerequisiteEntryVM(text2, calculatedPrerequisiteFact.Value, calculatedPrerequisiteFact.Not));
		}
		return list;
	}

	public static string GetAbilityTarget(BlueprintAbility blueprintAbility, BlueprintItem blueprintItem)
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			ItemEntityWeapon overrideWeapon = null;
			if (blueprintItem != null)
			{
				overrideWeapon = blueprintItem.CreateEntity() as ItemEntityWeapon;
			}
			BaseUnitEntity caster = UtilityParty.GetCurrentSelectedUnit() ?? Game.Instance.DefaultUnit;
			return GetAbilityTarget(new AbilityData(blueprintAbility, caster)
			{
				OverrideWeapon = overrideWeapon
			});
		}
	}

	public static string GetAbilityTarget(AbilityData abilitydata)
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			if (abilitydata.Blueprint.Range == AbilityRange.Weapon && abilitydata.Weapon != null)
			{
				return abilitydata.Blueprint.GetTarget(abilitydata.Weapon.AttackRange, abilitydata.Caster);
			}
			return abilitydata.Blueprint.GetTarget(-1, abilitydata.Caster);
		}
	}

	public static Sprite GetTargetImage(BlueprintAbility blueprintAbility)
	{
		return blueprintAbility.GetTargetImage();
	}

	public static List<Ability> TryGetAbilitiesWillBeLost(AbilityData abilityData)
	{
		ReactiveProperty<BaseUnitEntity> reactiveProperty = Game.Instance.Controllers?.SelectionCharacter?.SelectedUnit;
		if (reactiveProperty == null)
		{
			return null;
		}
		List<Ability> list = reactiveProperty.Value.ActionBar.Slots.Select((MechanicActionBarSlot s) => GetAbilityFromSlot(s)).ToList();
		if (!list.Contains(abilityData.Fact))
		{
			return null;
		}
		return list.Where((Ability a) => a != null && a != abilityData.Fact && a.Blueprint.AbilityGroups.Any((BlueprintAbilityGroup g) => abilityData.Blueprint.AbilityGroups.Contains(g) && g.CooldownInRounds > 0)).ToList();
	}

	private static Ability GetAbilityFromSlot(MechanicActionBarSlot mechanicActionBarSlot)
	{
		if (!(mechanicActionBarSlot is MechanicActionBarSlotAbility mechanicActionBarSlotAbility))
		{
			if (!(mechanicActionBarSlot is MechanicActionBarSlotItem mechanicActionBarSlotItem))
			{
				if (mechanicActionBarSlot is MechanicActionBarSlotSpontaneusConvertedSpell mechanicActionBarSlotSpontaneusConvertedSpell)
				{
					return mechanicActionBarSlotSpontaneusConvertedSpell.Spell.Fact;
				}
				return null;
			}
			return mechanicActionBarSlotItem.Ability;
		}
		return mechanicActionBarSlotAbility.OriginalAbility.Fact;
	}

	public static string GetAbilityAcronym(BlueprintFeatureBase featureBase)
	{
		return UtilityAbilities.GetAbilityAcronym(featureBase);
	}

	public static string GetAbilityAcronym(string name)
	{
		return UtilityAbilities.GetAbilityAcronym(name);
	}

	[Obsolete]
	public static string GetSpellDescriptorsText(BlueprintAbility abilityBlueprint)
	{
		return string.Empty;
	}

	[Obsolete]
	public static string GetAbilityActionText(BlueprintAbility blueprint, BlueprintItemEquipmentUsable item = null)
	{
		if (blueprint != null)
		{
			return "<obsolete>";
		}
		return string.Empty;
	}

	[Obsolete]
	public static string GetAbilityActionText(AbilityData dataAbilityData)
	{
		if (!(dataAbilityData == null))
		{
			return "<obsolete>";
		}
		return string.Empty;
	}
}
