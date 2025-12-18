using System;
using System.Collections.Generic;
using System.Linq;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs.Components;
using ObservableCollections;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateBuffs : TooltipBaseTemplate
{
	private readonly MechanicEntity m_Unit;

	private MechanicEntityUIWrapper m_UnitUIWrapper;

	public TooltipTemplateBuffs(MechanicEntity unit)
	{
		try
		{
			if (unit != null)
			{
				m_Unit = unit;
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
		BaseUnitEntity baseUnitEntity = (BaseUnitEntity)m_Unit;
		BlueprintArmyType army = baseUnitEntity.Blueprint.Army;
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
		Sprite surfaceCombatStandardPortrait = UIUtilityUnit.GetSurfaceCombatStandardPortrait(baseUnitEntity, PortraitCombatSize.Small);
		yield return new TooltipBrickPortraitAndName(surfaceCombatStandardPortrait, baseUnitEntity.CharacterName, new TooltipBrickTitle(title, TooltipTitleType.H6, TextAlignmentOptions.Left), (!baseUnitEntity.IsInPlayerParty) ? UIUtilityUnit.GetSurfaceEnemyDifficulty(baseUnitEntity) : 0, UIUtilityUnit.UsedSubtypeIcon(baseUnitEntity), baseUnitEntity.IsPlayerEnemy, !baseUnitEntity.IsInPlayerParty && !baseUnitEntity.IsPlayerEnemy);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		AddBuffsAndStatusEffects(list);
		return list;
	}

	private void UpdateUnitWrapper()
	{
		if (m_UnitUIWrapper.MechanicEntity != m_Unit)
		{
			m_UnitUIWrapper = new MechanicEntityUIWrapper(m_Unit);
		}
	}

	private void AddBuffsAndStatusEffects(List<ITooltipBrick> bricks)
	{
		UpdateUnitWrapper();
		bricks.Add(new TooltipBrickTitle(UIStrings.Instance.Inspect.StatusEffectsTitle.Text));
		Dictionary<BuffGroupType, ObservableList<ITooltipBrick>> dictionary = (from b in InspectExtensions.GetBuffsTooltipBricks(m_Unit).OfType<TooltipBrickBuff>()
			group b by b.Group).ToDictionary((IGrouping<BuffGroupType, TooltipBrickBuff> g) => g.Key, (IGrouping<BuffGroupType, TooltipBrickBuff> g) => new ObservableList<ITooltipBrick>(g.Cast<ITooltipBrick>()));
		(BuffGroupType, string)[] array = ((!m_UnitUIWrapper.IsPlayerFaction) ? new(BuffGroupType, string)[4]
		{
			(BuffGroupType.Positive, UIStrings.Instance.Inspect.EffectsNegative.Text),
			(BuffGroupType.CriticalEffect, UIStrings.Instance.Inspect.EffectsCritical.Text),
			(BuffGroupType.DOT, UIStrings.Instance.Inspect.EffectsDOT.Text),
			(BuffGroupType.Negative, UIStrings.Instance.Inspect.EffectsPositive.Text)
		} : new(BuffGroupType, string)[4]
		{
			(BuffGroupType.Negative, UIStrings.Instance.Inspect.EffectsNegative.Text),
			(BuffGroupType.CriticalEffect, UIStrings.Instance.Inspect.EffectsCritical.Text),
			(BuffGroupType.DOT, UIStrings.Instance.Inspect.EffectsDOT.Text),
			(BuffGroupType.Positive, UIStrings.Instance.Inspect.EffectsPositive.Text)
		});
		for (int i = 0; i < array.Length; i++)
		{
			var (buffGroupType, title) = array[i];
			if (TryAddBuffGroup(bricks, title, dictionary.GetValueOrDefault(buffGroupType)))
			{
				AddGroupHint(bricks, buffGroupType);
			}
		}
	}

	private static bool TryAddBuffGroup(ICollection<ITooltipBrick> bricks, string title, ObservableList<ITooltipBrick> list)
	{
		if (list == null || list.Count == 0)
		{
			return false;
		}
		bricks.Add(new TooltipBrickTitle(title, TooltipTitleType.H2));
		bricks.Add(new TooltipBrickWidget(list));
		return true;
	}

	private static void AddGroupHint(ICollection<ITooltipBrick> bricks, BuffGroupType group)
	{
		if (group == BuffGroupType.CriticalEffect)
		{
			TooltipBrickText item = new TooltipBrickText(UIStrings.Instance.Tooltips.CriticalEffectHint, TooltipTextType.Italic | TooltipTextType.BrightColor);
			bricks.Add(item);
		}
	}
}
