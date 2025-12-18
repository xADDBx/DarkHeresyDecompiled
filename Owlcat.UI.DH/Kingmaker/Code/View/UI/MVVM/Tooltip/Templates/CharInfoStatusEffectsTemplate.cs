using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Inspect;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Parts;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.Tooltip.Templates;

public class CharInfoStatusEffectsTemplate : TooltipBaseTemplate
{
	private readonly ReadOnlyReactiveProperty<BaseUnitEntity> m_UnitReactiveProperty;

	private readonly BaseUnitEntity m_Unit;

	private readonly UnitInspectInfoByPart m_InspectInfo;

	private MechanicEntityUIWrapper m_UnitUIWrapper;

	private readonly InspectReactiveData m_InspectReactiveData;

	private readonly BuffGroupType m_BuffGroupType;

	private static readonly TooltipBrickText s_NoEffects = new TooltipBrickText(UIStrings.Instance.Inspect.NoStatusEffects.Text, TooltipTextType.Simple | TooltipTextType.BrightColor, isHeader: false, TooltipTextAlignment.Midl, needChangeSize: true, 16);

	public CharInfoStatusEffectsTemplate(BaseUnitEntity unit, BuffGroupType BuffGroupType)
	{
		m_BuffGroupType = BuffGroupType;
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

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield break;
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
		AddBuffsAndStatusEffects(result);
	}

	private void AddBuffsAndStatusEffects(List<ITooltipBrick> bricks)
	{
		UpdateUnitWrapper();
		bricks.Add(new TooltipBrickSpace());
		bricks.Add(new TooltipBrickTitle(UIStrings.Instance.Inspect.StatusEffectsTitle.Text));
		Dictionary<BuffGroupType, ObservableList<ITooltipBrick>> dictionary = (from b in InspectExtensions.GetBuffsTooltipBricks(m_Unit).OfType<TooltipBrickBuff>()
			group b by b.Group).ToDictionary((IGrouping<BuffGroupType, TooltipBrickBuff> g) => g.Key, (IGrouping<BuffGroupType, TooltipBrickBuff> g) => new ObservableList<ITooltipBrick>(g.Cast<ITooltipBrick>()));
		AddBuffGroup(bricks, m_BuffGroupType switch
		{
			BuffGroupType.Positive => UIStrings.Instance.Inspect.EffectsPositive.Text, 
			BuffGroupType.Negative => UIStrings.Instance.Inspect.EffectsNegative.Text, 
			BuffGroupType.DOT => UIStrings.Instance.Inspect.EffectsDOT.Text, 
			BuffGroupType.CriticalEffect => UIStrings.Instance.Inspect.EffectsCritical.Text, 
			_ => throw new ArgumentOutOfRangeException(), 
		}, dictionary.GetValueOrDefault(m_BuffGroupType));
	}

	private void UpdateUnitWrapper()
	{
		if (m_UnitUIWrapper.MechanicEntity != m_Unit)
		{
			m_UnitUIWrapper = new MechanicEntityUIWrapper(m_Unit);
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

	private void GetInfoBody(List<ITooltipBrick> result)
	{
		result.Add(new TooltipBrickSpace(2f));
		AddBuffsAndStatusEffects(result);
	}
}
