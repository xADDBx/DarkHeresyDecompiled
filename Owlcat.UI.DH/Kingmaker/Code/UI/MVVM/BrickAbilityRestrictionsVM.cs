using System;
using System.Collections.Generic;
using System.Linq;
using Code.View.UI.UIUtils;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Common;
using Kingmaker.UI.UIUtils;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickAbilityRestrictionsVM : TooltipBrickVM
{
	public readonly IReadOnlyList<AbilityRestrictionGroupVM> Groups;

	public readonly bool IsPassed;

	public readonly string HeaderText;

	public BrickAbilityRestrictionsVM(BlueprintUnitFact blueprint, MechanicEntity owner)
	{
		HeaderText = UIStrings.Instance.Tooltips.AbilityRestrictionsTitle;
		List<AbilityRestrictionGroupVM> list = new List<AbilityRestrictionGroupVM>();
		CollectHasNoFactsGroups(blueprint, owner, list);
		CollectHasFactsGroups(blueprint, owner, list);
		CollectGenericRestrictions(blueprint, owner, list);
		CollectMoraleGroup(blueprint, owner, list);
		list.Sort(delegate(AbilityRestrictionGroupVM lhs, AbilityRestrictionGroupVM rhs)
		{
			if (lhs.ShowLogicalOr && rhs.ShowLogicalOr)
			{
				return 0;
			}
			if (lhs.ShowLogicalOr)
			{
				return 1;
			}
			return rhs.ShowLogicalOr ? (-1) : 0;
		});
		bool isPassed = true;
		for (int i = 0; i < list.Count; i++)
		{
			list[i].AddTo(this);
			if (!list[i].IsPassed)
			{
				isPassed = false;
			}
			if (i > 0)
			{
				list[i].SetPreviousGroup(list[i - 1]);
			}
			if (i < list.Count - 1)
			{
				list[i].SetNextGroup(list[i + 1]);
			}
		}
		Groups = list;
		IsPassed = isPassed;
	}

	private static void CollectHasNoFactsGroups(BlueprintUnitFact blueprint, MechanicEntity owner, List<AbilityRestrictionGroupVM> groups)
	{
		foreach (AbilityCasterHasNoFacts component in blueprint.GetComponents<AbilityCasterHasNoFacts>())
		{
			if (component != null && !component.HideInUI)
			{
				IEnumerable<string> abilityCasterRestrictionShortUITexts = component.GetAbilityCasterRestrictionShortUITexts(owner);
				List<AbilityRestrictionEntry> list = FactsToRestrictionEntries(component.Facts, abilityCasterRestrictionShortUITexts, (BlueprintUnitFact f) => !owner.Facts.Contains(f));
				if (list.Count > 0)
				{
					AddGroup(owner, groups, list, allMustPass: true);
				}
			}
		}
	}

	private static void CollectHasFactsGroups(BlueprintUnitFact blueprint, MechanicEntity owner, List<AbilityRestrictionGroupVM> groups)
	{
		foreach (AbilityCasterHasFacts component in blueprint.GetComponents<AbilityCasterHasFacts>())
		{
			if (component != null && !component.HideInUI)
			{
				IEnumerable<string> abilityCasterRestrictionShortUITexts = component.GetAbilityCasterRestrictionShortUITexts(owner);
				List<AbilityRestrictionEntry> list = FactsToRestrictionEntries(component.Facts, abilityCasterRestrictionShortUITexts, (BlueprintUnitFact f) => owner.Facts.Contains(f));
				if (list.Count > 0)
				{
					AddGroup(owner, groups, list, component.NeedsAll);
				}
			}
		}
	}

	private static List<AbilityRestrictionEntry> FactsToRestrictionEntries(ReferenceArrayProxy<BlueprintUnitFact> facts, IEnumerable<string> descriptions, Func<BlueprintUnitFact, bool> isPassedCheck)
	{
		List<AbilityRestrictionEntry> list = new List<AbilityRestrictionEntry>();
		int num = 0;
		foreach (string description in descriptions)
		{
			if (num >= facts.Length)
			{
				Debug.LogError("Descriptions count is more than facts count");
				break;
			}
			bool isPassed = isPassedCheck(facts[num]);
			list.Add(new AbilityRestrictionEntry(description, isPassed));
			num++;
		}
		return list;
	}

	private static void CollectGenericRestrictions(BlueprintUnitFact blueprint, MechanicEntity owner, List<AbilityRestrictionGroupVM> groups)
	{
		foreach (IAbilityCasterRestriction component in blueprint.GetComponents<IAbilityCasterRestriction>())
		{
			if (!(component is AbilityCasterHasFacts) && !(component is AbilityCasterHasNoFacts))
			{
				bool isPassed = component.IsCasterRestrictionPassed(owner);
				List<AbilityRestrictionEntry> list = (from desc in component.GetAbilityCasterRestrictionShortUITexts(owner)
					where desc != null
					select new AbilityRestrictionEntry(desc, isPassed)).ToList();
				if (list.Count > 0)
				{
					AddGroup(owner, groups, list, allMustPass: true);
				}
			}
		}
	}

	private static void CollectMoraleGroup(BlueprintUnitFact blueprint, MechanicEntity owner, List<AbilityRestrictionGroupVM> groups)
	{
		AbilitySpecialMoraleAction component = blueprint.GetComponent<AbilitySpecialMoraleAction>();
		if (component != null)
		{
			BlueprintEncyclopediaGlossaryEntry blueprintEncyclopediaGlossaryEntry = component.MoralePhaseType switch
			{
				MoraleAbilityType.Heroic => UIUtilityEncyclopedy.GetGlossaryEntry("MoraleHeroic"), 
				MoraleAbilityType.Broken => UIUtilityEncyclopedy.GetGlossaryEntry("MoraleBroken"), 
				MoraleAbilityType.Both => null, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
			if (blueprintEncyclopediaGlossaryEntry != null)
			{
				bool isPassed = UIUtilityAbilities.CheckMoraleIsPassed(owner, component.MoralePhaseType);
				string arg = FormatGlossaryLink(blueprintEncyclopediaGlossaryEntry);
				string description = string.Format(ConfigRoot.Instance.LocalizedTexts.Reasons.MoraleShouldBe, arg);
				List<AbilityRestrictionEntry> entries = new List<AbilityRestrictionEntry>
				{
					new AbilityRestrictionEntry(description, isPassed)
				};
				AddGroup(owner, groups, entries, allMustPass: true);
			}
		}
	}

	private static void AddGroup(MechanicEntity owner, List<AbilityRestrictionGroupVM> groups, IReadOnlyList<AbilityRestrictionEntry> entries, bool allMustPass)
	{
		groups.Add(new AbilityRestrictionGroupVM(entries, owner, !allMustPass));
	}

	private static string FormatGlossaryLink([NotNull] BlueprintEncyclopediaGlossaryEntry glossaryEntry)
	{
		return $"<b><color=#{UIConfig.Instance.LinkColor.HTML()}><link=\"{EntityLink.Type.Encyclopedia}:{glossaryEntry.name}\">{glossaryEntry.Title.Text}</link></color></b>";
	}
}
