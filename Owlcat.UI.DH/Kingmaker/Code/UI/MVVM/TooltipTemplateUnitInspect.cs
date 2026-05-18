using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Gameplay.Components;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.MVVM.Tooltip.Templates;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Inspect;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateUnitInspect : TooltipBaseTemplate
{
	private readonly ReadOnlyReactiveProperty<BaseUnitEntity> m_UnitReactiveProperty;

	private readonly BaseUnitEntity m_Unit;

	private readonly UnitUIInspectSettings m_Settings;

	private MechanicEntityUIWrapper m_UnitUIWrapper;

	private readonly InspectReactiveData m_InspectReactiveData;

	public TooltipTemplateUnitInspect(BaseUnitEntity unit)
	{
		try
		{
			if (unit != null)
			{
				m_Unit = unit;
				m_Settings = m_Unit.Blueprint.GetComponent<UnitUISettings>()?.InspectSettings;
				Game.Instance.Player.InspectUnitsManager.ForceRevealUnitInfo(m_Unit);
				UnitInspectInfoByPart info = InspectUnitsHelper.GetInfo(m_Unit.BlueprintForInspection, force: true);
				m_Unit.GetOptional<UnitPartInspectedBuffs>()?.GetBuffs(info);
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
			yield return new BrickTextVM(UIStrings.Instance.Tooltips.UnitIsNotInspected);
			yield break;
		}
		BlueprintArmyType army = m_Unit.Blueprint.Army;
		string text = string.Empty;
		if (army?.IsDaemon ?? false)
		{
			text = UIStrings.Instance.CharacterSheet.Chaos;
		}
		if (army?.IsXenos ?? false)
		{
			text = UIStrings.Instance.CharacterSheet.Xenos;
		}
		if (army?.IsHuman ?? false)
		{
			text = UIStrings.Instance.CharacterSheet.Human;
		}
		Sprite surfaceCombatStandardPortrait = UIUtilityUnit.GetSurfaceCombatStandardPortrait(m_Unit, PortraitCombatSize.Small);
		yield return new BrickPortraitAndNameVM(surfaceCombatStandardPortrait, m_Unit.CharacterName, new BrickTitleVM(new TextEntity(text, TextFieldParams.Left), TooltipTitleType.H6), (!m_Unit.IsInPlayerParty) ? UIUtilityUnit.GetSurfaceEnemyDifficulty(m_Unit) : 0, UIUtilityUnit.UsedSubtypeIcon(m_Unit), UIUtilityTooltip.GetPortraitType(m_Unit));
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

	private bool HasSettingsFlags(UnitInspectUIFlags flags)
	{
		if (m_Settings != null)
		{
			return m_Settings.HasFlags(flags);
		}
		return false;
	}

	private void AddCommonInfoBricks(List<ITooltipBrick> result)
	{
		if (!HasSettingsFlags(UnitInspectUIFlags.HideUnitInfo))
		{
			AddWounds(result);
			AddDurability(result);
			AddDefence(result);
			AddDamageReduction(result);
			AddMovePoints(result);
		}
		if (!HasSettingsFlags(UnitInspectUIFlags.HideMorale) && !m_Unit.Features.DoNotUseMoraleAndPowerBalance)
		{
			AddMorale(result);
		}
	}

	private void GetTooltipBody(List<ITooltipBrick> result)
	{
		AddCommonInfoBricks(result);
		AddBuffsAndStatusEffects(result);
	}

	private void GetInfoBody(List<ITooltipBrick> result)
	{
		AddCommonInfoBricks(result);
		result.Add(new BrickSpaceVM(2f));
		AddBuffsAndStatusEffects(result);
		result.Add(new BrickSpaceVM(2f));
		AddWeapon(result);
		result.Add(new BrickSpaceVM(2f));
		result.Add(new BrickTitleVM(UIStrings.Instance.Inspect.AbilitiesTitle, TooltipTitleType.H3));
		bool flag = false;
		BlueprintAbility[] array = (from a in UIUtilityUnit.CollectAbilities(m_Unit)
			select a.Blueprint).ToArray();
		if (array.Any())
		{
			flag = true;
			result.Add(new BrickTitleVM(UIStrings.Instance.Inspect.ActiveAbilitiesTitle, TooltipTitleType.H2));
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
				result.Add(new BrickTitleVM(UIStrings.Instance.Inspect.UltimateAbilitiesTitle, TooltipTitleType.H2));
				AddAbilities(result, list.ToArray(), TooltipTemplateType.Info);
			}
		}
		UIFeature[] array2 = UIUtilityUnit.CollectFeats(m_Unit).ToArray();
		if (array2.Any())
		{
			flag = true;
			result.Add(new BrickTitleVM(UIStrings.Instance.Inspect.PassiveAbilitiesTitle, TooltipTitleType.H2));
			AddFeatures(result, array2, TooltipTemplateType.Info);
		}
		if (!flag)
		{
			result.Add(new BrickTextVM(UIStrings.Instance.Inspect.NoAbilities.Text, TooltipTextType.Simple | TooltipTextType.BrightColor, TooltipTextAlignment.Midl, m_Unit));
		}
		if (!HasSettingsFlags(UnitInspectUIFlags.HideCharacteristics))
		{
			result.Add(new BrickSpaceVM(2f));
			result.Add(new BrickAbilityScoresBlockVM(m_UnitReactiveProperty));
		}
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
		BaseUnitEntity unit = m_Unit;
		if (unit != null && unit.GetOptional<PartHealth>()?.IsCountHpAsArmor == true)
		{
			return;
		}
		if (m_InspectReactiveData != null)
		{
			bricks.Add(new BrickIconStatValueVM(new TextValueAddElement(UIStrings.Instance.Inspect.Wounds.Text, m_InspectReactiveData.WoundsValue.CurrentValue), UIConfig.Instance.UIIcons.TooltipInspectIcons.Wounds, BrickElementPalette.Normal, BrickElementPalette.Normal, new TooltipTemplateGlossary("HitPoints"), m_InspectReactiveData.WoundsValue));
			return;
		}
		UpdateUnitWrapper();
		if (InspectExtensions.TryGetWoundsText(m_UnitUIWrapper, out var woundsValue))
		{
			bricks.Add(new BrickIconStatValueVM(new TextValueAddElement(UIStrings.Instance.Inspect.Wounds.Text, woundsValue), UIConfig.Instance.UIIcons.TooltipInspectIcons.Wounds, BrickElementPalette.Normal, BrickElementPalette.Normal, new TooltipTemplateGlossary("HitPoints")));
		}
	}

	private void AddDurability(List<ITooltipBrick> bricks)
	{
		if (m_InspectReactiveData != null)
		{
			bricks.Add(new BrickIconStatValueVM(new TextValueAddElement(UIStrings.Instance.Inspect.Durability.Text, m_InspectReactiveData.DurabilityValue.CurrentValue), UIConfig.Instance.UIIcons.TooltipInspectIcons.Durability, BrickElementPalette.Normal, BrickElementPalette.Normal, new TooltipTemplateGlossary("Durability"), m_InspectReactiveData.DurabilityValue));
			return;
		}
		UpdateUnitWrapper();
		if (InspectExtensions.TryGetDurabilityText(m_UnitUIWrapper, out var durabilityValue))
		{
			bricks.Add(new BrickIconStatValueVM(new TextValueAddElement(UIStrings.Instance.Inspect.Durability.Text, durabilityValue), UIConfig.Instance.UIIcons.TooltipInspectIcons.Durability, BrickElementPalette.Normal, BrickElementPalette.Normal, new TooltipTemplateGlossary("Durability")));
		}
	}

	private void AddDefence(List<ITooltipBrick> bricks)
	{
		if (m_InspectReactiveData != null)
		{
			bricks.Add(new BrickIconStatValueVM(new TextValueAddElement(UIStrings.Instance.Inspect.Defence.Text, m_InspectReactiveData.DefenceValue.CurrentValue), tooltip: new TooltipTemplateDefence(m_Unit, StatType.Defence), icon: UIConfig.Instance.UIIcons.TooltipInspectIcons.Defence, type: BrickElementPalette.Normal, backgroundType: BrickElementPalette.Normal, reactiveValue: m_InspectReactiveData.DefenceValue));
		}
		else
		{
			bricks.Add(new BrickIconStatValueVM(new TextValueAddElement(UIStrings.Instance.Inspect.Defence.Text, InspectExtensions.GetDefence(m_Unit)), tooltip: new TooltipTemplateDefence(m_Unit, StatType.Defence), icon: UIConfig.Instance.UIIcons.TooltipInspectIcons.Defence));
		}
	}

	private void AddDamageReduction(List<ITooltipBrick> bricks)
	{
		if (m_InspectReactiveData != null)
		{
			bricks.Add(new BrickIconStatValueVM(new TextValueAddElement(UIStrings.Instance.Inspect.DamageReduction.Text, m_InspectReactiveData.DamageReductionValue.CurrentValue), tooltip: new TooltipTemplateDamageReduction(m_Unit, StatType.ArmorDamageReduction), icon: UIConfig.Instance.UIIcons.TooltipInspectIcons.DamageReduction, type: BrickElementPalette.Normal, backgroundType: BrickElementPalette.Normal, reactiveValue: m_InspectReactiveData.DamageReductionValue));
		}
		else
		{
			bricks.Add(new BrickIconStatValueVM(new TextValueAddElement(UIStrings.Instance.Inspect.DamageReduction.Text, InspectExtensions.GetDamageReduction(m_Unit)), tooltip: new TooltipTemplateDamageReduction(m_Unit, StatType.ArmorDamageReduction), icon: UIConfig.Instance.UIIcons.TooltipInspectIcons.DamageReduction));
		}
	}

	private void AddMovePoints(List<ITooltipBrick> bricks)
	{
		if (m_InspectReactiveData != null)
		{
			bricks.Add(new BrickIconStatValueVM(new TextValueAddElement(UIStrings.Instance.Inspect.MovePoints.Text, m_InspectReactiveData.MovementPointsValue.CurrentValue), tooltip: new TooltipTemplateGlossary("MovementPoints"), icon: UIConfig.Instance.UIIcons.TooltipInspectIcons.MovePoints, type: BrickElementPalette.Normal, backgroundType: BrickElementPalette.Normal, reactiveValue: m_InspectReactiveData.MovementPointsValue));
		}
		else
		{
			bricks.Add(new BrickIconStatValueVM(new TextValueAddElement(UIStrings.Instance.Inspect.MovePoints.Text, InspectExtensions.GetMovementPoints(m_Unit)), tooltip: new TooltipTemplateGlossary("MovementPoints"), icon: UIConfig.Instance.UIIcons.TooltipInspectIcons.MovePoints));
		}
	}

	private void AddMorale(List<ITooltipBrick> bricks)
	{
		if (m_InspectReactiveData != null)
		{
			(int, int, int) currentValue = m_InspectReactiveData.MoraleValue.CurrentValue;
			bricks.Add(new BrickSegmentedProgressBarVM(UIStrings.Instance.Inspect.Morale.Text, currentValue.Item1, currentValue.Item2, currentValue.Item3, new TooltipTemplateGlossary("Morale")));
			return;
		}
		IUIUnitMoraleData morale = InspectExtensions.GetMorale(m_Unit);
		if (morale != null)
		{
			bricks.Add(new BrickSegmentedProgressBarVM(UIStrings.Instance.Inspect.Morale.Text, morale.MinValue, morale.MaxValue, morale.Morale, new TooltipTemplateGlossary("Morale")));
		}
	}

	private void AddBuffsAndStatusEffects(List<ITooltipBrick> bricks)
	{
		if (!HasSettingsFlags(UnitInspectUIFlags.HideEffectsAndConditions))
		{
			UpdateUnitWrapper();
			BuffGroupFlags groupsShowFlags = m_Settings.GetGroupsShowFlags();
			bricks.Add(new BrickBuffGroupsVM(m_Unit, groupsShowFlags));
		}
	}

	private void AddWeapon(List<ITooltipBrick> bricks)
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
				list.Add(new BrickWeaponSetVM(item.PrimaryHand, isPrimary: true));
			}
			primaryHand = item.SecondaryHand;
			if (primaryHand != null && primaryHand.HasWeapon)
			{
				if (list.Count > 0)
				{
					list.Add(new BrickSpaceVM(2f));
				}
				list.Add(new BrickWeaponSetVM(item.SecondaryHand, isPrimary: false));
			}
		}
		if (list.Count > 0)
		{
			bricks.Add(new BrickTitleVM(UIStrings.Instance.Inspect.WeaponsTitle, TooltipTitleType.H3));
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
				bricks.Add(new BrickSeparatorVM(TooltipBrickElementType.Small));
				bricks.Add(new BrickTextVM(stringBuilder.ToString(), TooltipTextType.Bold, TooltipTextAlignment.Midl, m_Unit));
			}
			break;
		}
		case TooltipTemplateType.Info:
		{
			BlueprintAbility[] array = abilities;
			foreach (BlueprintAbility blueprintAbility in array)
			{
				bool isHidden = blueprintAbility.CultAmbushVisibility(m_Unit) == UnitPartCultAmbush.VisibilityStatuses.NotVisible;
				bricks.Add(new BrickFeatureVM(blueprintAbility, isHeader: false, null, m_Unit, isHidden));
			}
			break;
		}
		}
	}

	private void AddFeatures(List<ITooltipBrick> bricks, UIFeature[] features, TooltipTemplateType type)
	{
		if (features.Empty())
		{
			return;
		}
		features = features.Where((UIFeature f) => !f.Feature.IsStarshipFeature).ToArray();
		switch (type)
		{
		case TooltipTemplateType.Tooltip:
		{
			StringBuilder stringBuilder = new StringBuilder();
			UIFeature[] array = features;
			foreach (UIFeature uIFeature in array)
			{
				if (!string.IsNullOrEmpty(uIFeature.Name) && !uIFeature.Feature.HideInUI)
				{
					UIUtilityText.TryAddWordSeparator(stringBuilder, ", ");
					stringBuilder.Append(uIFeature.Name);
				}
			}
			if (stringBuilder.Length != 0)
			{
				stringBuilder.Replace(" ,", ",");
				bricks.Add(new BrickSeparatorVM(TooltipBrickElementType.Small));
				bricks.Add(new BrickTextVM(stringBuilder.ToString(), TooltipTextType.Bold, TooltipTextAlignment.Midl, m_Unit));
			}
			break;
		}
		case TooltipTemplateType.Info:
		{
			UIFeature[] array = features;
			for (int i = 0; i < array.Length; i++)
			{
				BlueprintFeature feature = array[i].Feature;
				if (!string.IsNullOrEmpty(feature.Name) && !feature.HideInUI)
				{
					bool isHidden = feature.CultAmbushVisibility(m_Unit) == UnitPartCultAmbush.VisibilityStatuses.NotVisible;
					bricks.Add(new BrickFeatureVM(feature, isHeader: false, available: true, showIcon: true, null, m_Unit, forceSetName: false, isHidden));
				}
			}
			break;
		}
		}
	}
}
