using System;
using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UI.UIUtils;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Assets.Code.View.UI.MVVM;

public class TooltipTemplateTalent : TooltipBaseTemplate
{
	public readonly BlueprintFeature BlueprintFeature;

	private readonly TalentIconInfo m_TalentInfo;

	private readonly MechanicEntity m_Caster;

	private string m_Name;

	private string m_Description;

	private string m_Acronym;

	public TooltipTemplateTalent(BlueprintFeature talent, MechanicEntity caster = null)
	{
		BlueprintFeature = talent;
		m_Caster = caster;
		m_TalentInfo = talent.TalentIconInfo;
	}

	public override void Prepare(TooltipTemplateType type)
	{
		if (BlueprintFeature == null)
		{
			return;
		}
		try
		{
			using (GameLogContext.Scope)
			{
				GameLogContext.UnitEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)m_Caster;
				m_Name = BlueprintFeature.Name;
				m_Description = UIUtilityText.UpdateDescriptionWithUIProperties(BlueprintFeature.Description, m_Caster);
				m_Acronym = UIUtilityAbilities.GetAbilityAcronym(m_Name);
			}
		}
		catch (Exception arg)
		{
			Debug.LogError($"Can't create TooltipTemplate for: {BlueprintFeature.name}: {arg}");
		}
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		TooltipBrickIconPattern.TextFieldValues titleValues = new TooltipBrickIconPattern.TextFieldValues
		{
			Text = BlueprintFeature.Name,
			TextParams = new TextFieldParams
			{
				FontStyles = FontStyles.Bold
			}
		};
		yield return new TooltipBrickIconPattern(null, null, titleValues, null, null, null, IconPatternMode.SkillMode, m_Acronym, m_TalentInfo);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		return new List<ITooltipBrick>
		{
			new TooltipBrickText(m_Description),
			new TooltipBrickSeparator(TooltipBrickElementType.Small)
		};
	}
}
