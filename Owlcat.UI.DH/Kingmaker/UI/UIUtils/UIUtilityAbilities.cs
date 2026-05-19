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
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.EntitySystem.Properties.Getters;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework;
using Kingmaker.Gameplay.Features.Scaling.Components;
using Kingmaker.Gameplay.Parts;
using Kingmaker.Items;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Levelup.Selections.Prerequisites;
using Kingmaker.UnitLogic.Progression;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.Utility.StatefulRandom;
using Owlcat.Fmw.Blueprints;
using R3;
using UnityEngine;

namespace Kingmaker.UI.UIUtils;

public static class UIUtilityAbilities
{
	private static readonly IReadOnlyList<BlueprintAbilityModifier> EmptyModifiersList = new List<BlueprintAbilityModifier>();

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
							int level = calculatedPrerequisiteLevel.Level;
							list.Add(new PrerequisiteEntryVM(new TextValueElement(text, level.ToString()), calculatedPrerequisiteLevel.Value, calculatedPrerequisiteLevel.Not));
						}
					}
					else
					{
						list.Add(new PrerequisiteEntryVM(new TextValueElement(UIStrings.Instance.Tooltips.PrerequisiteRank), calculatedPrerequisiteMaxRankNotReached.Value, calculatedPrerequisiteMaxRankNotReached.Not));
					}
				}
				else
				{
					list.Add(new PrerequisiteEntryVM(new TextValueElement(LocalizedTexts.Instance.Stats.GetText(calculatedPrerequisiteStat.Stat), calculatedPrerequisiteStat.MinValue.ToString()), calculatedPrerequisiteStat.Value, calculatedPrerequisiteStat.Not));
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
			list.Add(new PrerequisiteEntryVM(new TextValueElement(text2), calculatedPrerequisiteFact.Value, calculatedPrerequisiteFact.Not));
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

	public static IReadOnlyList<BlueprintAbilityModifier> GetAppliedModifiers(this BlueprintToggleAbility toggleAbility, MechanicEntity caster)
	{
		IReadOnlyList<BlueprintAbilityModifier> readOnlyList = (caster?.GetOptional<PartAbilityModifiers>())?.GetBoundModifiers(toggleAbility).ToList();
		return readOnlyList ?? EmptyModifiersList;
	}

	public static IReadOnlyList<BlueprintAbilityModifier> GetAppliedModifiers(this BlueprintAbility blueprintAbility, MechanicEntity caster, out BlueprintAbilityModifier manuallyAdded)
	{
		PartAbilityModifiers partAbilityModifiers = caster?.GetOptional<PartAbilityModifiers>();
		if (partAbilityModifiers == null)
		{
			manuallyAdded = null;
			return EmptyModifiersList;
		}
		manuallyAdded = null;
		List<BlueprintAbilityModifier> list = new List<BlueprintAbilityModifier>();
		foreach (PartAbilityModifiers.AddedEntry addedModifier in partAbilityModifiers.AddedModifiers)
		{
			if (IsRelevant(addedModifier, blueprintAbility))
			{
				if (manuallyAdded == null && IsManuallyAddedModifier(addedModifier, partAbilityModifiers))
				{
					manuallyAdded = addedModifier.Modifier;
				}
				list.Add(addedModifier.Modifier);
			}
		}
		return list;
		static bool IsManuallyAddedModifier(PartAbilityModifiers.AddedEntry entry, PartAbilityModifiers abilityModifiers)
		{
			if (entry.Ability == null || !abilityModifiers.IsAddedManually(entry.Ability, entry.Modifier))
			{
				if (entry.AbilityTag != null)
				{
					return abilityModifiers.IsAddedManually(entry.AbilityTag, entry.Modifier);
				}
				return false;
			}
			return true;
		}
		static bool IsRelevant(PartAbilityModifiers.AddedEntry entry, BlueprintAbility ability)
		{
			if (entry.Ability == null || entry.Ability != ability)
			{
				if (entry.AbilityTag != null)
				{
					return ability.Tags.Contains(entry.AbilityTag);
				}
				return false;
			}
			return true;
		}
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

	public static bool CheckMoraleIsPassed(MechanicEntity caster, MoraleAbilityType requiredType)
	{
		PartMorale optional = caster.GetOptional<PartMorale>();
		if (optional == null)
		{
			return false;
		}
		switch (requiredType)
		{
		case MoraleAbilityType.Heroic:
			return optional.Phase == MoralePhaseType.Heroic;
		case MoraleAbilityType.Broken:
			return optional.Phase == MoralePhaseType.Broken;
		case MoraleAbilityType.Both:
		{
			MoralePhaseType phase = optional.Phase;
			return phase == MoralePhaseType.Heroic || phase == MoralePhaseType.Broken;
		}
		default:
			return false;
		}
	}

	public static IEnumerable<StatType> GetScalingStats(this BlueprintUnitFact blueprint)
	{
		IEnumerable<PropertyScalingComponent> components = blueprint.GetComponents<PropertyScalingComponent>();
		IEnumerable<AbilityPropertyComponent> components2 = blueprint.GetComponents<AbilityPropertyComponent>();
		HashSet<StatType> hashSet = new HashSet<StatType>();
		if (components != null)
		{
			foreach (PropertyScalingComponent item in components)
			{
				AddRelevantStats(hashSet, item.Calculator);
			}
		}
		if (components2 != null)
		{
			foreach (AbilityPropertyComponent item2 in components2)
			{
				AddRelevantStats(hashSet, item2.ScalingCalculator);
			}
		}
		return hashSet;
		static void AddRelevantStats(HashSet<StatType> statTypes, PropertyCalculator calculator)
		{
			if (calculator?.Getters != null)
			{
				PropertyGetter[] getters = calculator.Getters;
				foreach (PropertyGetter propertyGetter in getters)
				{
					if (propertyGetter is SimplePropertyGetter simplePropertyGetter)
					{
						StatType statType = simplePropertyGetter.Property.ToBaseStat();
						if (statType != 0)
						{
							statTypes.Add(statType);
						}
					}
					else if (propertyGetter is PropertyCalculatorGetter propertyCalculatorGetter)
					{
						AddRelevantStats(statTypes, propertyCalculatorGetter.Value);
					}
				}
			}
		}
	}

	public static bool TryGetAttachableBlueprintFact(this BlueprintScriptableObject feature, out BlueprintUnitFact fact)
	{
		AddFacts addFacts = feature?.GetComponent<AddFacts>();
		fact = ((addFacts != null) ? addFacts.Facts.FirstOrDefault() : null);
		return fact != null;
	}

	public static bool HasCasterRestrictions(this BlueprintAbility blueprintAbility, MechanicEntity caster, out bool restrictionsPassed)
	{
		return HasCasterRestrictionsInternal(blueprintAbility, caster, out restrictionsPassed);
	}

	public static bool HasCasterRestrictions(this BlueprintToggleAbility blueprintAbility, MechanicEntity caster, out bool restrictionsPassed)
	{
		return HasCasterRestrictionsInternal(blueprintAbility, caster, out restrictionsPassed);
	}

	private static bool HasCasterRestrictionsInternal(BlueprintUnitFact blueprintAbility, MechanicEntity caster, out bool restrictionsPassed)
	{
		IEnumerable<IAbilityCasterRestriction> components = blueprintAbility.GetComponents<IAbilityCasterRestriction>();
		bool result = false;
		foreach (IAbilityCasterRestriction item in components)
		{
			result = true;
			if (!item.IsCasterRestrictionPassed(caster))
			{
				restrictionsPassed = false;
				return true;
			}
		}
		foreach (AbilitySpecialMoraleAction component in blueprintAbility.GetComponents<AbilitySpecialMoraleAction>())
		{
			result = true;
			if (!CheckMoraleIsPassed(caster, component.MoralePhaseType))
			{
				restrictionsPassed = false;
				return true;
			}
		}
		restrictionsPassed = true;
		return result;
	}

	private static StatType ToBaseStat(this ContextProperty property)
	{
		if (!Enum.TryParse<StatType>(property.ToString().Replace("Bonus", ""), out var result))
		{
			return StatType.Unknown;
		}
		if (!StatTypeHelper.BaseStats.TryGetValue(result, out var value))
		{
			return result;
		}
		return value;
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
		return mechanicActionBarSlotAbility.OriginalAbility?.Fact;
	}

	private static string GetPrerequisiteFactName(CalculatedPrerequisiteFact prerequisiteFact)
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

	private static PrerequisiteEntryVM UnpackPrerequisiteComposite(CalculatedPrerequisiteComposite prerequisiteComposite)
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
		return new PrerequisiteEntryVM(new TextValueElement(stringBuilder.ToString()), prerequisiteComposite.Value, prerequisiteComposite.Not);
	}

	private static bool CanUnpackComposite(CalculatedPrerequisiteComposite prerequisiteComposite)
	{
		return prerequisiteComposite.Prerequisites.All((CalculatedPrerequisite i) => i is CalculatedPrerequisiteFact);
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
	public static string GetSpellDescriptorsText(BlueprintAbility _)
	{
		return string.Empty;
	}

	[Obsolete]
	public static string GetAbilityActionText(BlueprintAbility blueprint, BlueprintItemEquipmentUsable _ = null)
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
