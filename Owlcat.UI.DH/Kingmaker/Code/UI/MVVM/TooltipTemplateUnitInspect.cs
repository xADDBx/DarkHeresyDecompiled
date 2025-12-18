using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.MVVM.Tooltip.Templates;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Inspect;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.DotNetExtensions;
using ObservableCollections;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateUnitInspect : TooltipBaseTemplate
{
	private readonly ReadOnlyReactiveProperty<BaseUnitEntity> m_UnitReactiveProperty;

	private readonly BaseUnitEntity m_Unit;

	private readonly UnitInspectInfoByPart m_InspectInfo;

	private MechanicEntityUIWrapper m_UnitUIWrapper;

	private readonly InspectReactiveData m_InspectReactiveData;

	private static readonly TooltipBrickText s_NoEffects = new TooltipBrickText(UIStrings.Instance.Inspect.NoStatusEffects.Text, TooltipTextType.Simple | TooltipTextType.BrightColor, isHeader: false, TooltipTextAlignment.Midl, needChangeSize: true, 16);

	public TooltipTemplateUnitInspect(BaseUnitEntity unit)
	{
		try
		{
			if (unit != null)
			{
				m_Unit = unit;
				Game.Instance.Player.InspectUnitsManager.ForceRevealUnitInfo(m_Unit);
				m_InspectInfo = InspectUnitsHelper.GetInfo(m_Unit.BlueprintForInspection, force: true);
				m_Unit.GetOptional<UnitPartInspectedBuffs>()?.GetBuffs(m_InspectInfo);
			}
		}
		catch (Exception arg)
		{
			Debug.LogError($"Can't create TooltipTemplate for: {unit?.Blueprint?.name}: {arg}");
		}
	}

	public TooltipTemplateUnitInspect(ReadOnlyReactiveProperty<BaseUnitEntity> unit, InspectReactiveData inspectReactiveData)
		: this(unit.CurrentValue)
	{
		m_UnitReactiveProperty = unit;
		m_InspectReactiveData = inspectReactiveData;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		if (m_Unit == null)
		{
			yield return new TooltipBrickText(UIStrings.Instance.Tooltips.UnitIsNotInspected);
			yield break;
		}
		BlueprintArmyType army = m_Unit.Blueprint.Army;
		string title = string.Empty;
		if (army?.IsDaemon ?? false)
		{
			title = UIStrings.Instance.CharacterSheet.Chaos;
		}
		if (army?.IsXenos ?? false)
		{
			title = UIStrings.Instance.CharacterSheet.Xenos;
		}
		if (army?.IsHuman ?? false)
		{
			title = UIStrings.Instance.CharacterSheet.Human;
		}
		Sprite surfaceCombatStandardPortrait = UIUtilityUnit.GetSurfaceCombatStandardPortrait(m_Unit, PortraitCombatSize.Small);
		yield return new TooltipBrickPortraitAndName(surfaceCombatStandardPortrait, m_Unit.CharacterName, new TooltipBrickTitle(title, TooltipTitleType.H6, TextAlignmentOptions.Left), (!m_Unit.IsInPlayerParty) ? UIUtilityUnit.GetSurfaceEnemyDifficulty(m_Unit) : 0, UIUtilityUnit.UsedSubtypeIcon(m_Unit), m_Unit.IsPlayerEnemy, !m_Unit.IsInPlayerParty && !m_Unit.IsPlayerEnemy);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> result = new List<ITooltipBrick>();
		if (m_Unit == null)
		{
			return result;
		}
		if (type == TooltipTemplateType.Tooltip)
		{
			GetTooltipBody(result);
		}
		else
		{
			GetInfoBody(result);
		}
		return result;
	}

	private void GetTooltipBody(List<ITooltipBrick> result)
	{
		AddWounds(result);
		AddDurability(result);
		AddDefence(result);
		AddDamageReduction(result);
		AddMovePoints(result);
		AddBuffsAndStatusEffects(result);
	}

	private void GetInfoBody(List<ITooltipBrick> result)
	{
		AddWounds(result);
		AddDurability(result);
		AddDefence(result);
		AddDamageReduction(result);
		AddMovePoints(result);
		result.Add(new TooltipBrickSpace(2f));
		AddBuffsAndStatusEffects(result);
		result.Add(new TooltipBrickSpace(2f));
		AddWeapon(result);
		result.Add(new TooltipBrickSpace(2f));
		result.Add(new TooltipBrickTitle(UIStrings.Instance.Inspect.AbilitiesTitle));
		bool flag = false;
		BlueprintAbility[] array = (from a in UIUtilityUnit.CollectAbilities(m_Unit)
			select a.Blueprint).ToArray();
		if (array.Any())
		{
			flag = true;
			result.Add(new TooltipBrickTitle(UIStrings.Instance.Inspect.ActiveAbilitiesTitle, TooltipTitleType.H5));
			AddAbilities(result, array, TooltipTemplateType.Info);
		}
		if (m_Unit.IsInPlayerParty)
		{
			List<BlueprintAbility> list = new List<BlueprintAbility>();
			foreach (Ability ability in m_Unit.Abilities)
			{
				if (!ability.Blueprint.Hidden)
				{
					list.Add(ability.Blueprint);
				}
			}
			if (list.Any())
			{
				flag = true;
				result.Add(new TooltipBrickTitle(UIStrings.Instance.Inspect.UltimateAbilitiesTitle, TooltipTitleType.H5));
				AddAbilities(result, list.ToArray(), TooltipTemplateType.Info);
			}
		}
		UIFeature[] array2 = UIUtilityUnit.CollectFeats(m_Unit).ToArray();
		if (array2.Any())
		{
			flag = true;
			result.Add(new TooltipBrickTitle(UIStrings.Instance.Inspect.PassiveAbilitiesTitle, TooltipTitleType.H5));
			FeatureUIData[] features = array2;
			AddFeatures(result, features, TooltipTemplateType.Info);
		}
		if (!flag)
		{
			result.Add(new TooltipBrickText(UIStrings.Instance.Inspect.NoAbilities.Text, TooltipTextType.Simple | TooltipTextType.BrightColor, isHeader: false, TooltipTextAlignment.Midl, needChangeSize: true, 16));
		}
		result.Add(new TooltipBrickSpace(2f));
		result.Add(new TooltipBrickAbilityScoresBlock(m_UnitReactiveProperty));
	}

	private void UpdateUnitWrapper()
	{
		if (m_UnitUIWrapper.MechanicEntity != m_Unit)
		{
			m_UnitUIWrapper = new MechanicEntityUIWrapper(m_Unit);
		}
	}

	private void AddWounds(List<ITooltipBrick> bricks)
	{
		if (m_InspectReactiveData != null)
		{
			bricks.Add(new TooltipBrickIconStatValue(UIStrings.Instance.Inspect.Wounds.Text, m_InspectReactiveData.WoundsValue.CurrentValue, null, UIConfig.Instance.UIIcons.TooltipInspectIcons.Wounds, TooltipBrickIconStatValueType.Normal, TooltipBrickIconStatValueType.Normal, null, new TooltipTemplateGlossary("HitPoints"), m_InspectReactiveData.WoundsValue));
			return;
		}
		UpdateUnitWrapper();
		if (InspectExtensions.TryGetWoundsText(m_UnitUIWrapper, out var woundsValue))
		{
			bricks.Add(new TooltipBrickIconStatValue(UIStrings.Instance.Inspect.Wounds.Text, woundsValue, null, UIConfig.Instance.UIIcons.TooltipInspectIcons.Wounds, TooltipBrickIconStatValueType.Normal, TooltipBrickIconStatValueType.Normal, null, new TooltipTemplateGlossary("HitPoints")));
		}
	}

	private void AddDurability(List<ITooltipBrick> bricks)
	{
		if (m_InspectReactiveData != null)
		{
			bricks.Add(new TooltipBrickIconStatValue(UIStrings.Instance.Inspect.Durability.Text, m_InspectReactiveData.DurabilityValue.CurrentValue, null, UIConfig.Instance.UIIcons.TooltipInspectIcons.Durability, TooltipBrickIconStatValueType.Normal, TooltipBrickIconStatValueType.Normal, null, new TooltipTemplateGlossary("Durability"), m_InspectReactiveData.DurabilityValue));
			return;
		}
		UpdateUnitWrapper();
		if (InspectExtensions.TryGetDurabilityText(m_UnitUIWrapper, out var durabilityValue))
		{
			bricks.Add(new TooltipBrickIconStatValue(UIStrings.Instance.Inspect.Durability.Text, durabilityValue, null, UIConfig.Instance.UIIcons.TooltipInspectIcons.Durability, TooltipBrickIconStatValueType.Normal, TooltipBrickIconStatValueType.Normal, null, new TooltipTemplateGlossary("Durability")));
		}
	}

	private void AddDefence(List<ITooltipBrick> bricks)
	{
		ModifiableValue statOptional = m_Unit.GetStatOptional(StatType.Defence);
		if (m_InspectReactiveData != null)
		{
			bricks.Add(new TooltipBrickIconStatValue(UIStrings.Instance.Inspect.Defence.Text, m_InspectReactiveData.DefenceValue.CurrentValue, null, tooltip: new TooltipTemplateDefence(statOptional), icon: UIConfig.Instance.UIIcons.TooltipInspectIcons.Defence, type: TooltipBrickIconStatValueType.Normal, backgroundType: TooltipBrickIconStatValueType.Normal, valueHint: null, reactiveValue: m_InspectReactiveData.DefenceValue));
		}
		else
		{
			bricks.Add(new TooltipBrickIconStatValue(UIStrings.Instance.Inspect.Defence.Text, InspectExtensions.GetDefence(m_Unit), null, tooltip: new TooltipTemplateDefence(statOptional), icon: UIConfig.Instance.UIIcons.TooltipInspectIcons.Defence));
		}
	}

	private void AddDamageReduction(List<ITooltipBrick> bricks)
	{
		ModifiableValue statOptional = m_Unit.GetStatOptional(StatType.ArmorDamageReduction);
		if (m_InspectReactiveData != null)
		{
			bricks.Add(new TooltipBrickIconStatValue(UIStrings.Instance.Inspect.DamageReduction.Text, m_InspectReactiveData.DamageReductionValue.CurrentValue, null, tooltip: new TooltipTemplateDamageReduction(statOptional), icon: UIConfig.Instance.UIIcons.TooltipInspectIcons.DamageReduction, type: TooltipBrickIconStatValueType.Normal, backgroundType: TooltipBrickIconStatValueType.Normal, valueHint: null, reactiveValue: m_InspectReactiveData.DamageReductionValue));
		}
		else
		{
			bricks.Add(new TooltipBrickIconStatValue(UIStrings.Instance.Inspect.DamageReduction.Text, InspectExtensions.GetDamageReduction(m_Unit), null, tooltip: new TooltipTemplateDamageReduction(statOptional), icon: UIConfig.Instance.UIIcons.TooltipInspectIcons.DamageReduction));
		}
	}

	private void AddMovePoints(List<ITooltipBrick> bricks)
	{
		if (m_InspectReactiveData != null)
		{
			bricks.Add(new TooltipBrickIconStatValue(UIStrings.Instance.Inspect.MovePoints.Text, m_InspectReactiveData.MovementPointsValue.CurrentValue, null, tooltip: new TooltipTemplateGlossary("MovementPoints"), icon: UIConfig.Instance.UIIcons.TooltipInspectIcons.MovePoints, type: TooltipBrickIconStatValueType.Normal, backgroundType: TooltipBrickIconStatValueType.Normal, valueHint: null, reactiveValue: m_InspectReactiveData.MovementPointsValue));
		}
		else
		{
			bricks.Add(new TooltipBrickIconStatValue(UIStrings.Instance.Inspect.MovePoints.Text, InspectExtensions.GetMovementPoints(m_Unit), null, tooltip: new TooltipTemplateGlossary("MovementPoints"), icon: UIConfig.Instance.UIIcons.TooltipInspectIcons.MovePoints));
		}
	}

	private void AddBuffsAndStatusEffects(List<ITooltipBrick> bricks)
	{
		UpdateUnitWrapper();
		bricks.Add(new TooltipBrickTitle(UIStrings.Instance.Inspect.StatusEffectsTitle.Text, TooltipTitleType.H3));
		Dictionary<BuffGroupType, ObservableList<ITooltipBrick>> dictionary = (from b in InspectExtensions.GetBuffsTooltipBricks(m_Unit).OfType<TooltipBrickBuff>()
			group b by b.Group).ToDictionary((IGrouping<BuffGroupType, TooltipBrickBuff> g) => g.Key, (IGrouping<BuffGroupType, TooltipBrickBuff> g) => new ObservableList<ITooltipBrick>(g.Cast<ITooltipBrick>()));
		(BuffGroupType, string)[] array = new(BuffGroupType, string)[4]
		{
			(BuffGroupType.Negative, UIStrings.Instance.Inspect.EffectsNegative.Text),
			(BuffGroupType.CriticalEffect, UIStrings.Instance.Inspect.EffectsCritical.Text),
			(BuffGroupType.DOT, UIStrings.Instance.Inspect.EffectsDOT.Text),
			(BuffGroupType.Positive, UIStrings.Instance.Inspect.EffectsPositive.Text)
		};
		for (int i = 0; i < array.Length; i++)
		{
			var (key, title) = array[i];
			AddBuffGroup(bricks, title, dictionary.GetValueOrDefault(key));
		}
	}

	private static void AddBuffGroup(List<ITooltipBrick> bricks, string title, ObservableList<ITooltipBrick> list)
	{
		bricks.Add(new TooltipBrickTitle(title, TooltipTitleType.H2));
		if (list == null || list.Count == 0)
		{
			bricks.Add(s_NoEffects);
		}
		else
		{
			bricks.Add(new TooltipBrickWidget(list));
		}
	}

	protected void AddWeapon(List<ITooltipBrick> bricks)
	{
		IList<HandsEquipmentSet> handsEquipmentSets = m_Unit.Body.HandsEquipmentSets;
		if (handsEquipmentSets.Empty())
		{
			return;
		}
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		foreach (HandsEquipmentSet item in handsEquipmentSets)
		{
			HandSlot primaryHand = item.PrimaryHand;
			if (primaryHand != null && primaryHand.HasWeapon)
			{
				list.Add(new TooltipBrickWeaponSet(item.PrimaryHand, isPrimary: true));
			}
			primaryHand = item.SecondaryHand;
			if (primaryHand != null && primaryHand.HasWeapon)
			{
				if (list.Count > 0)
				{
					list.Add(new TooltipBrickSpace(2f));
				}
				list.Add(new TooltipBrickWeaponSet(item.SecondaryHand, isPrimary: false));
			}
		}
		if (list.Count > 0)
		{
			bricks.Add(new TooltipBrickTitle(UIStrings.Instance.Inspect.WeaponsTitle));
			bricks.AddRange(list);
		}
	}

	protected void AddAbilities(List<ITooltipBrick> bricks, BlueprintAbility[] abilities, TooltipTemplateType type)
	{
		if (abilities.Empty())
		{
			return;
		}
		switch (type)
		{
		case TooltipTemplateType.Tooltip:
		{
			StringBuilder stringBuilder = new StringBuilder();
			BlueprintAbility[] array = abilities;
			foreach (BlueprintAbility blueprintAbility2 in array)
			{
				if (!string.IsNullOrEmpty(blueprintAbility2.Name))
				{
					UIUtilityText.TryAddWordSeparator(stringBuilder, ", ");
					stringBuilder.Append(blueprintAbility2.Name);
				}
			}
			if (stringBuilder.Length != 0)
			{
				stringBuilder.Replace(" ,", ",");
				bricks.Add(new TooltipBrickSeparator(TooltipBrickElementType.Small));
				bricks.Add(new TooltipBrickText(stringBuilder.ToString(), TooltipTextType.Bold));
			}
			break;
		}
		case TooltipTemplateType.Info:
		{
			BlueprintAbility[] array = abilities;
			foreach (BlueprintAbility blueprintAbility in array)
			{
				bool isHidden = blueprintAbility.CultAmbushVisibility(m_Unit) == UnitPartCultAmbush.VisibilityStatuses.NotVisible;
				bricks.Add(new TooltipBrickFeature(blueprintAbility, isHeader: false, m_Unit, isHidden));
			}
			break;
		}
		}
	}

	protected void AddFeatures(List<ITooltipBrick> bricks, FeatureUIData[] features, TooltipTemplateType type)
	{
		if (features.Empty())
		{
			return;
		}
		features = features.Where((FeatureUIData f) => !f.Feature.IsStarshipFeature).ToArray();
		switch (type)
		{
		case TooltipTemplateType.Tooltip:
		{
			StringBuilder stringBuilder = new StringBuilder();
			FeatureUIData[] array = features;
			foreach (FeatureUIData featureUIData in array)
			{
				if (!string.IsNullOrEmpty(featureUIData.Name) && !featureUIData.Feature.HideInUI)
				{
					UIUtilityText.TryAddWordSeparator(stringBuilder, ", ");
					stringBuilder.Append(featureUIData.Name);
				}
			}
			if (stringBuilder.Length != 0)
			{
				stringBuilder.Replace(" ,", ",");
				bricks.Add(new TooltipBrickSeparator(TooltipBrickElementType.Small));
				bricks.Add(new TooltipBrickText(stringBuilder.ToString(), TooltipTextType.Bold));
			}
			break;
		}
		case TooltipTemplateType.Info:
		{
			FeatureUIData[] array = features;
			for (int i = 0; i < array.Length; i++)
			{
				BlueprintFeature feature = array[i].Feature;
				if (!string.IsNullOrEmpty(feature.Name) && !feature.HideInUI)
				{
					bool isHidden = feature.CultAmbushVisibility(m_Unit) == UnitPartCultAmbush.VisibilityStatuses.NotVisible;
					bricks.Add(new TooltipBrickFeature(feature, isHeader: false, available: true, showIcon: true, m_Unit, forceSetName: false, isHidden));
				}
			}
			break;
		}
		}
	}
}
