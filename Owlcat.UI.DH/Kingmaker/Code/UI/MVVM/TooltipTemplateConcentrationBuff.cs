using System.Collections.Generic;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public sealed class TooltipTemplateConcentrationBuff : TooltipBaseTemplate
{
	private readonly bool m_IsSteadyConcentration;

	private readonly TooltipTemplateBuff m_TooltipTemplateBuff;

	public TooltipTemplateConcentrationBuff(Buff buff, IEntity overrideCaster = null, Sprite overrideIcon = null, string overrideName = null, string overrideSecondary = null)
	{
		m_TooltipTemplateBuff = new TooltipTemplateBuff(buff, overrideCaster, overrideIcon, overrideName, overrideSecondary);
		FeatureCountableFlag featureCountableFlag = buff?.Owner?.Features?.SteadyConcentration;
		m_IsSteadyConcentration = featureCountableFlag != null && (bool)featureCountableFlag;
	}

	public override void Prepare(TooltipTemplateType type)
	{
		m_TooltipTemplateBuff.Prepare(type);
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		List<ITooltipBrick> obj = (m_TooltipTemplateBuff.GetHeader(type) as List<ITooltipBrick>) ?? new List<ITooltipBrick>();
		obj.Add(new BrickTitleVM(UIStrings.Instance.HUDTexts.ConcentrationHint, TooltipTitleType.H3));
		return obj;
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> obj = (m_TooltipTemplateBuff.GetBody(type) as List<ITooltipBrick>) ?? new List<ITooltipBrick>();
		obj.Add(new BrickSeparatorVM(TooltipBrickElementType.Small));
		obj.Add((!m_IsSteadyConcentration) ? new BrickTextVM(UIUtilityEncyclopedy.GetGlossaryEntry("Concentration").GetDescription()) : new BrickTextVM(UIStrings.Instance.HUDTexts.SteadyConcentrationHint, TooltipTextType.Bold));
		obj.Add(new BrickSeparatorVM(TooltipBrickElementType.Medium));
		return obj;
	}
}
