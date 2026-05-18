using System;
using System.Collections.Generic;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.Code.UI.MVVM;
using Owlcat.UI;

namespace Kingmaker.Code.View.UI.MVVM.Tooltip.Templates;

public class TooltipTemplateMoralePhase : TooltipBaseTemplate
{
	private readonly BlueprintEncyclopediaGlossaryEntry m_MoraleHeroicGlossaryEntry;

	public TooltipTemplateMoralePhase(MoralePhaseType moralePhase)
	{
		m_MoraleHeroicGlossaryEntry = GetGlossaryEntry(moralePhase);
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new BrickTitleVM(m_MoraleHeroicGlossaryEntry?.Title);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		return new List<ITooltipBrick>
		{
			new BrickTextVM(m_MoraleHeroicGlossaryEntry?.GetDescription())
		};
	}

	private BlueprintEncyclopediaGlossaryEntry GetGlossaryEntry(MoralePhaseType moralePhase)
	{
		return moralePhase switch
		{
			MoralePhaseType.Regular => null, 
			MoralePhaseType.Heroic => UIUtilityEncyclopedy.GetGlossaryEntry("MoraleHeroic"), 
			MoralePhaseType.Broken => UIUtilityEncyclopedy.GetGlossaryEntry("MoraleBroken"), 
			_ => throw new ArgumentOutOfRangeException("moralePhase", moralePhase, null), 
		};
	}
}
