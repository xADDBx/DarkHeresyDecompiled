using System;
using System.Collections.Generic;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.MVVM.Tooltip.Templates;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using ObservableCollections;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateUnitInspectShort : TooltipBaseTemplate
{
	private readonly MechanicEntityUIWrapper m_UnitUIWrapper;

	private readonly BaseUnitEntity m_Unit;

	public TooltipTemplateUnitInspectShort(BaseUnitEntity unit)
	{
		try
		{
			if (unit != null)
			{
				m_Unit = unit;
				m_UnitUIWrapper = new MechanicEntityUIWrapper(m_Unit);
				Game.Instance.Player.InspectUnitsManager.ForceRevealUnitInfo(m_Unit);
			}
		}
		catch (Exception arg)
		{
			Debug.LogError($"Can't create TooltipTemplate for: {unit?.Blueprint?.name}: {arg}");
		}
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
		GetTooltipBody(result);
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

	private void AddWounds(List<ITooltipBrick> bricks)
	{
		if (InspectExtensions.TryGetWoundsText(m_UnitUIWrapper, out var woundsValue))
		{
			bricks.Add(new TooltipBrickIconStatValue(UIStrings.Instance.Inspect.Wounds.Text, woundsValue, null, UIConfig.Instance.UIIcons.TooltipInspectIcons.Wounds, TooltipBrickIconStatValueType.Normal, TooltipBrickIconStatValueType.Normal, null, new TooltipTemplateGlossary("HitPoints")));
		}
	}

	private void AddDurability(List<ITooltipBrick> bricks)
	{
		if (InspectExtensions.TryGetDurabilityText(m_UnitUIWrapper, out var durabilityValue))
		{
			bricks.Add(new TooltipBrickIconStatValue(UIStrings.Instance.Inspect.Durability.Text, durabilityValue, null, UIConfig.Instance.UIIcons.TooltipInspectIcons.Durability, TooltipBrickIconStatValueType.Normal, TooltipBrickIconStatValueType.Normal, null, new TooltipTemplateGlossary("Durability")));
		}
	}

	private void AddDefence(List<ITooltipBrick> bricks)
	{
		ModifiableValue statOptional = m_Unit.GetStatOptional(StatType.Defence);
		bricks.Add(new TooltipBrickIconStatValue(UIStrings.Instance.Inspect.Defence.Text, InspectExtensions.GetDefence(m_Unit), null, tooltip: new TooltipTemplateDefence(statOptional), icon: UIConfig.Instance.UIIcons.TooltipInspectIcons.Defence));
	}

	private void AddDamageReduction(List<ITooltipBrick> bricks)
	{
		ModifiableValue statOptional = m_Unit.GetStatOptional(StatType.ArmorDamageReduction);
		bricks.Add(new TooltipBrickIconStatValue(UIStrings.Instance.Inspect.DamageReduction.Text, InspectExtensions.GetDamageReduction(m_Unit), null, tooltip: new TooltipTemplateDamageReduction(statOptional), icon: UIConfig.Instance.UIIcons.TooltipInspectIcons.DamageReduction));
	}

	private void AddMovePoints(List<ITooltipBrick> bricks)
	{
		bricks.Add(new TooltipBrickIconStatValue(UIStrings.Instance.Inspect.MovePoints.Text, InspectExtensions.GetMovementPoints(m_Unit), null, tooltip: new TooltipTemplateGlossary("MovementPoints"), icon: UIConfig.Instance.UIIcons.TooltipInspectIcons.MovePoints));
	}

	private void AddBuffsAndStatusEffects(List<ITooltipBrick> bricks)
	{
		bricks.Add(new TooltipBrickTitle(UIStrings.Instance.Inspect.StatusEffectsTitle.Text));
		ObservableList<ITooltipBrick> buffsTooltipBricks = InspectExtensions.GetBuffsTooltipBricks(m_Unit);
		bricks.Add(new TooltipBrickWidget(buffsTooltipBricks, new TooltipBrickText(UIStrings.Instance.Inspect.NoStatusEffects.Text, TooltipTextType.Simple | TooltipTextType.BrightColor)));
	}
}
