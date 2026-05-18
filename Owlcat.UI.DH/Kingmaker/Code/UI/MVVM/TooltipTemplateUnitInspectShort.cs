using System;
using System.Collections.Generic;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.MVVM.Tooltip.Templates;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using ObservableCollections;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

[Obsolete("Unused")]
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
			bricks.Add(new BrickIconStatValueVM(new TextValueAddElement(UIStrings.Instance.Inspect.Wounds.Text, woundsValue), UIConfig.Instance.UIIcons.TooltipInspectIcons.Wounds, BrickElementPalette.Normal, BrickElementPalette.Normal, new TooltipTemplateGlossary("HitPoints")));
		}
	}

	private void AddDurability(List<ITooltipBrick> bricks)
	{
		if (InspectExtensions.TryGetDurabilityText(m_UnitUIWrapper, out var durabilityValue))
		{
			bricks.Add(new BrickIconStatValueVM(new TextValueAddElement(UIStrings.Instance.Inspect.Durability.Text, durabilityValue), UIConfig.Instance.UIIcons.TooltipInspectIcons.Durability, BrickElementPalette.Normal, BrickElementPalette.Normal, new TooltipTemplateGlossary("Durability")));
		}
	}

	private void AddDefence(List<ITooltipBrick> bricks)
	{
		bricks.Add(new BrickIconStatValueVM(new TextValueAddElement(UIStrings.Instance.Inspect.Defence.Text, InspectExtensions.GetDefence(m_Unit)), tooltip: new TooltipTemplateDefence(m_Unit, StatType.Defence), icon: UIConfig.Instance.UIIcons.TooltipInspectIcons.Defence));
	}

	private void AddDamageReduction(List<ITooltipBrick> bricks)
	{
		bricks.Add(new BrickIconStatValueVM(new TextValueAddElement(UIStrings.Instance.Inspect.DamageReduction.Text, InspectExtensions.GetDamageReduction(m_Unit)), tooltip: new TooltipTemplateDamageReduction(m_Unit, StatType.ArmorDamageReduction), icon: UIConfig.Instance.UIIcons.TooltipInspectIcons.DamageReduction));
	}

	private void AddMovePoints(List<ITooltipBrick> bricks)
	{
		bricks.Add(new BrickIconStatValueVM(new TextValueAddElement(UIStrings.Instance.Inspect.MovePoints.Text, InspectExtensions.GetMovementPoints(m_Unit)), tooltip: new TooltipTemplateGlossary("MovementPoints"), icon: UIConfig.Instance.UIIcons.TooltipInspectIcons.MovePoints));
	}

	private void AddBuffsAndStatusEffects(List<ITooltipBrick> bricks)
	{
		bricks.Add(new BrickTitleVM(UIStrings.Instance.Inspect.StatusEffectsTitle.Text));
		ObservableList<ITooltipBrick> buffsTooltipBricks = InspectExtensions.GetBuffsTooltipBricks(m_Unit);
		bricks.Add(new BrickWidgetVM(buffsTooltipBricks, new BrickTextVM(UIStrings.Instance.Inspect.NoStatusEffects.Text, TooltipTextType.Simple | TooltipTextType.BrightColor)));
	}
}
