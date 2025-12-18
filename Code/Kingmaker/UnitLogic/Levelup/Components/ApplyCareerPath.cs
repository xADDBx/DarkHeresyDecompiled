using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework;
using Kingmaker.Framework.Abilities.Blueprints;
using Kingmaker.Gameplay.Parts;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.CharacterGender;
using Kingmaker.UnitLogic.Levelup.Selections.CharacterName;
using Kingmaker.UnitLogic.Levelup.Selections.Doll;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Levelup.Selections.Portrait;
using Kingmaker.UnitLogic.Levelup.Selections.Stats;
using Kingmaker.UnitLogic.Levelup.Selections.Voice;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Pool;

namespace Kingmaker.UnitLogic.Levelup.Components;

[Serializable]
[AllowedOn(typeof(BlueprintUnit))]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[TypeId("e85c079c48fe4978beddcb7b61475362")]
public sealed class ApplyCareerPath : UnitFactComponentDelegate
{
	[Serializable]
	public class SelectionEntry
	{
		public FeatureGroup Group;

		[SerializeField]
		[ValidateNotEmpty]
		[ValidateNoNullEntries]
		private BlueprintFeature.Reference[] m_Items = new BlueprintFeature.Reference[0];

		public ReferenceArrayProxy<BlueprintFeature> Items
		{
			get
			{
				BlueprintReference<BlueprintFeature>[] items = m_Items;
				return items;
			}
		}

		public void AddItem(BlueprintFeature feature)
		{
			Array.Resize(ref m_Items, m_Items.Length + 1);
			m_Items[^1] = feature.ToReference<BlueprintFeature.Reference>();
		}
	}

	[Serializable]
	public class AbilityModifierEntry
	{
		[ValidateNotNull]
		public BpRef<BlueprintAbilityModifier> Modifier;

		[HideIf("HasTargetToggleAbility")]
		public BpRef<BlueprintAbility> TargetAbility;

		[HideIf("HasTargetAbility")]
		public BpRef<BlueprintToggleAbility> TargetToggleAbility;

		public BpRef<BlueprintAbilityTag> TargetTag;

		private bool HasTargetAbility => TargetAbility != null;

		private bool HasTargetToggleAbility => TargetToggleAbility != null;
	}

	[SerializeField]
	[ValidateNotNull]
	private BlueprintPath.Reference m_CareerPath;

	public int Ranks;

	public AttributeType[] AttributesAdvancementOrder = new AttributeType[0];

	public SkillType[] SkillsAdvancementOrder = new SkillType[0];

	public SelectionEntry[] Selections = new SelectionEntry[0];

	public AbilityModifierEntry[] AbilityModifiers = new AbilityModifierEntry[0];

	public BlueprintPath CareerPath
	{
		get
		{
			return m_CareerPath;
		}
		set
		{
			m_CareerPath = value.ToReference<BlueprintPath.Reference>();
		}
	}

	protected override void OnActivate()
	{
		if (CareerPath is BlueprintCareerPath { IsAvailable: false })
		{
			return;
		}
		bool num = CareerPath is BlueprintOriginPath;
		int num2 = (num ? int.MaxValue : (base.Owner.OriginalBlueprint.GetComponent<CharacterLevelLimit>()?.LevelLimit ?? int.MaxValue));
		int rank = base.Owner.Progression.GetRank(CareerPath);
		int characterLevel = base.Owner.Progression.CharacterLevel;
		int num3 = ((!num) ? ((characterLevel >= num2) ? num2 : Math.Clamp(characterLevel + Ranks - rank, characterLevel, num2)) : 0);
		if (!num && num3 <= characterLevel)
		{
			return;
		}
		base.Owner.Progression.AdvanceExperienceToLevel(num3, log: false);
		foreach (SelectionState selection in new LevelUpManager(base.Owner, CareerPath, autoCommit: true, num3).Selections)
		{
			bool flag;
			if (!(selection is SelectionStateFeature selectionStateFeature))
			{
				if (!(selection is SelectionStateStats selectionStateStats))
				{
					if (!(selection is SelectionStateDoll) && !(selection is SelectionStatePortrait) && !(selection is SelectionStateCharacterName) && !(selection is SelectionStateVoice) && !(selection is SelectionStateGender))
					{
						throw new ArgumentOutOfRangeException("selection");
					}
					flag = true;
				}
				else
				{
					flag = !selectionStateStats.CanSelectAny || SelectStats(selectionStateStats);
				}
			}
			else
			{
				flag = !selectionStateFeature.CanSelectAny || SelectFeature(selectionStateFeature);
			}
			if (!flag)
			{
				PFLog.LevelUp.ErrorWithReport($"ApplyCareerPath: can't find suitable option for selection ${selection.Blueprint} " + $"in path ${selection.Path}[${selection.PathRank}] " + $"({base.Owner})");
			}
		}
		ApplyAbilityModifiers();
	}

	private bool SelectFeature(SelectionStateFeature selection)
	{
		ReferenceArrayProxy<BlueprintFeature>? referenceArrayProxy = Selections.FirstItem((SelectionEntry i) => i.Group == selection.Blueprint.Group)?.Items;
		if (referenceArrayProxy.HasValue)
		{
			foreach (BlueprintFeature feature in referenceArrayProxy.Value)
			{
				FeatureSelectionItem selectionItem = selection.Items.FirstItem((FeatureSelectionItem i) => i.Feature == feature && selection.CanSelect(i));
				if (selectionItem.Feature != null)
				{
					selection.Select(selectionItem);
					return true;
				}
			}
		}
		FeatureSelectionItem selectionItem2 = selection.Items.FirstItem(selection.CanSelect);
		if (selectionItem2.Feature == null)
		{
			return false;
		}
		selection.Select(selectionItem2);
		return true;
	}

	private bool SelectStats(SelectionStateStats selection)
	{
		BlueprintSelectionStats blueprint = selection.Blueprint;
		IEnumerable<StatType> enumerable;
		if (!(blueprint is BlueprintSelectionSkills))
		{
			if (!(blueprint is BlueprintSelectionAttributes))
			{
				throw new ArgumentOutOfRangeException("Blueprint");
			}
			enumerable = AttributesAdvancementOrder.Select(StatTypeHelper.ToStatType);
		}
		else
		{
			enumerable = SkillsAdvancementOrder.Select(StatTypeHelper.ToStatType);
		}
		IEnumerable<StatType> enumerable2 = enumerable;
		Dictionary<StatType, int> value;
		using (CollectionPool<Dictionary<StatType, int>, KeyValuePair<StatType, int>>.Get(out value))
		{
			foreach (StatType item in enumerable2)
			{
				int num2 = (value[item] = value.Get(item, 0) + 1);
				int num3 = num2;
				if (selection.GetPointsTotal(item) < num3)
				{
					selection.AddPoints(item, num3);
					if (selection.IsMade)
					{
						return true;
					}
				}
			}
			return selection.CanSelectAny;
		}
	}

	private void ApplyAbilityModifiers()
	{
		PartAbilityModifiers optional = base.Owner.GetOptional<PartAbilityModifiers>();
		if (optional == null)
		{
			return;
		}
		AbilityModifierEntry[] abilityModifiers = AbilityModifiers;
		foreach (AbilityModifierEntry abilityModifierEntry in abilityModifiers)
		{
			BlueprintAbilityModifier blueprint = abilityModifierEntry.Modifier.Blueprint;
			if (!optional.KnownModifiers.Contains(blueprint))
			{
				continue;
			}
			BlueprintAbility maybeBlueprint = abilityModifierEntry.TargetAbility.MaybeBlueprint;
			if (maybeBlueprint != null)
			{
				optional.AddModifier(blueprint, maybeBlueprint);
				continue;
			}
			BlueprintToggleAbility maybeBlueprint2 = abilityModifierEntry.TargetToggleAbility.MaybeBlueprint;
			if (maybeBlueprint2 != null)
			{
				optional.BindModifier(blueprint, maybeBlueprint2);
				continue;
			}
			BlueprintAbilityTag maybeBlueprint3 = abilityModifierEntry.TargetTag.MaybeBlueprint;
			if (maybeBlueprint3 != null)
			{
				optional.AddModifier(blueprint, maybeBlueprint3);
			}
		}
	}
}
