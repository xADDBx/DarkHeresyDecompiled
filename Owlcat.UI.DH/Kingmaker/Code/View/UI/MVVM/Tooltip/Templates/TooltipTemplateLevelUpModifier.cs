using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.Abilities.Blueprints;
using Kingmaker.UnitLogic.Levelup;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.Tooltip.Templates;

public class TooltipTemplateLevelUpModifier : TooltipBaseTemplate
{
	public readonly BlueprintAbilityModifier BlueprintModifier;

	private string m_Name = string.Empty;

	private string m_Description = string.Empty;

	private Sprite m_Icon;

	private readonly LevelUpManager m_LevelUpManager;

	private readonly BaseUnitEntity m_Caster;

	private BaseUnitEntity Caster
	{
		get
		{
			BaseUnitEntity baseUnitEntity = m_Caster;
			if (baseUnitEntity == null)
			{
				LevelUpManager levelUpManager = m_LevelUpManager;
				if (levelUpManager == null)
				{
					return null;
				}
				baseUnitEntity = levelUpManager.PreviewUnit;
			}
			return baseUnitEntity;
		}
	}

	public TooltipTemplateLevelUpModifier(BlueprintAbilityModifier modifier, LevelUpManager levelUpManager = null, BaseUnitEntity caster = null)
	{
		BlueprintModifier = modifier;
		m_LevelUpManager = levelUpManager;
		m_Caster = caster;
	}

	public override void Prepare(TooltipTemplateType type)
	{
		if (BlueprintModifier != null)
		{
			FillAbilityModifierDataInfo();
		}
	}

	private void FillAbilityModifierDataInfo()
	{
		try
		{
			m_Name = BlueprintModifier.Name;
			m_Description = BlueprintModifier.Description;
			m_Icon = BlueprintModifier.Tags.FirstOrDefault()?.Icon;
		}
		catch (Exception arg)
		{
			Debug.LogError($"Can't create TooltipTemplate for: {BlueprintModifier.name}: {arg}");
		}
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		List<string> values = BlueprintModifier.Tags.Select((BlueprintAbilityTag tag) => tag.Name.Text).ToList();
		yield return new TooltipBrickLevelUpHeader(new TooltipBrickLevelUpFeatureData(m_Name, null, null, string.Join(", ", values), null, null, m_Icon, iconWithFrame: false));
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		AddDescription(list);
		if (type == TooltipTemplateType.Info)
		{
			AddFittingAbilities(list);
		}
		return list;
	}

	private void AddDescription(List<ITooltipBrick> bricks)
	{
		string text = UIUtilityText.UpdateDescriptionWithUIProperties(m_Description, Caster);
		bricks.Add(new TooltipBrickText(text, TooltipTextType.LevelUpLineSpacing));
	}

	private void AddFittingAbilities(List<ITooltipBrick> bricks)
	{
		bricks.Add(new TooltipBrickLevelUpFittingAbilities(BlueprintModifier, Caster));
	}
}
