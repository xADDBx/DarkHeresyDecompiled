using System;
using System.Collections.Generic;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UI.UIUtils;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

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
				GameLogContext.DescriptionOwner = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)m_Caster;
				m_Name = BlueprintFeature.Name;
				m_Description = BlueprintFeature.Description;
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
		TextEntity title = new TextEntity(BlueprintFeature.Name, TextFieldParams.Bold);
		yield return new BrickIconPatternVM(null, null, title, null, null, null, IconPatternMode.SkillMode, m_Acronym, m_TalentInfo);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		return new List<ITooltipBrick>
		{
			new BrickTextVM(m_Description, TooltipTextType.Simple, TooltipTextAlignment.Midl, m_Caster),
			new BrickSeparatorVM(TooltipBrickElementType.Small)
		};
	}
}
