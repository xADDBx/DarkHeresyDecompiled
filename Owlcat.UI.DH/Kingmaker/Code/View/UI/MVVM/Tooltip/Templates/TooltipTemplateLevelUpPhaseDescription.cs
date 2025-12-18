using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.Tooltip.Templates;

public class TooltipTemplateLevelUpPhaseDescription : TooltipBaseTemplate
{
	private readonly CharGenPhaseType m_PhaseType;

	public TooltipTemplateLevelUpPhaseDescription(CharGenPhaseType phaseType)
	{
		m_PhaseType = phaseType;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		string item = UIStrings.Instance.CharGen.GetDefaultTooltipStrings(m_PhaseType).Item1;
		yield return new TooltipBrickLevelUpHeader(new TooltipBrickLevelUpFeatureData(null, null, null, null, null, null, null, iconWithFrame: true, default(Vector2), null, null, item));
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		if (m_PhaseType == CharGenPhaseType.LevelUpModification)
		{
			list.Add(new TooltipBrickImage(UIConfig.Instance.UIIcons.TooltipIcons.ModifierTutorial));
		}
		list.Add(new TooltipBrickText(UIStrings.Instance.CharGen.GetDefaultTooltipStrings(m_PhaseType).Item2));
		return list;
	}
}
