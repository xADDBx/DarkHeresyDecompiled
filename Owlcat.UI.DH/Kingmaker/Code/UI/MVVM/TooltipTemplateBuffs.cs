using System;
using System.Collections.Generic;
using Code.View.UI.UIUtils;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Entities;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateBuffs : TooltipBaseTemplate
{
	private readonly MechanicEntity m_Unit;

	private readonly BuffGroupsVM m_BuffGroupsVM;

	private readonly BuffGroupFlags m_BuffGroupFlags;

	public TooltipTemplateBuffs(MechanicEntity unit, [CanBeNull] BuffGroupsVM buffGroupsVM = null, BuffGroupFlags groupFlags = BuffGroupFlags.All)
	{
		try
		{
			if (unit != null)
			{
				m_Unit = unit;
				m_BuffGroupsVM = buffGroupsVM;
				m_BuffGroupFlags = groupFlags;
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
		BaseUnitEntity baseUnitEntity = (BaseUnitEntity)m_Unit;
		BlueprintArmyType army = baseUnitEntity.Blueprint.Army;
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
		Sprite surfaceCombatStandardPortrait = UIUtilityUnit.GetSurfaceCombatStandardPortrait(baseUnitEntity, PortraitCombatSize.Small);
		yield return new BrickPortraitAndNameVM(surfaceCombatStandardPortrait, baseUnitEntity.CharacterName, new BrickTitleVM(new TextEntity(text, TextFieldParams.Left), TooltipTitleType.H6), (!baseUnitEntity.IsInPlayerParty) ? UIUtilityUnit.GetSurfaceEnemyDifficulty(baseUnitEntity) : 0, UIUtilityUnit.UsedSubtypeIcon(baseUnitEntity), UIUtilityTooltip.GetPortraitType(baseUnitEntity));
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		BrickBuffGroupsVM item = ((m_BuffGroupsVM != null) ? new BrickBuffGroupsVM(m_BuffGroupsVM, m_BuffGroupFlags) : new BrickBuffGroupsVM(m_Unit, m_BuffGroupFlags));
		return new List<ITooltipBrick> { item };
	}
}
