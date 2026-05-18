using System.Collections.Generic;
using System.Linq;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateAlignmentFeature : TooltipBaseTemplate
{
	private readonly BaseUnitEntity m_Unit;

	private readonly int m_Tier;

	private readonly int m_CurrentValue;

	private readonly List<int> m_RankThresholds;

	private readonly AlignmentAxis m_Axis;

	public TooltipTemplateAlignmentFeature(BaseUnitEntity unit, AlignmentAxis direction, int tier)
	{
		m_Unit = unit;
		m_Tier = tier;
		m_Axis = direction;
		AlignmentTooltipExtensions.GetAlignmentInfo(direction, unit, out m_RankThresholds, out var _, out m_CurrentValue, out var _);
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		string title = string.Format(UIStrings.Instance.Alignment.AlignmentRankTitle.Text, UIUtilityAlignment.GetAlignmentDirectionText(m_Axis).Text, UIUtilityAlignment.GetAlignmentRankText(m_Tier));
		yield return new BrickTitleVM(title, TooltipTitleType.H1);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		ReasonCannotHaveMark reason;
		bool flag = !m_Unit.Alignment.CanHaveMarkInAxis(m_Axis, m_Tier, out reason);
		ReasonCannotHaveMark reason2;
		bool flag2 = !m_Unit.Alignment.CanHaveMarkInAxis(m_Axis, 4, out reason2);
		string text = reason switch
		{
			ReasonCannotHaveMark.None => UIStrings.Instance.Alignment.AlignmentMayBeLocked, 
			ReasonCannotHaveMark.MainG3 => UIStrings.Instance.Alignment.AlignmentIsLockedMainChoosed, 
			ReasonCannotHaveMark.OppositeG1 => UIStrings.Instance.Alignment.AlignmentIsLockedOpposite, 
			_ => string.Empty, 
		};
		bool flag3 = m_Unit.Alignment.GetMainAxis() == m_Axis;
		if (!((flag2 && !flag) || flag3))
		{
			yield return new BrickTextVM(text, TooltipTextType.Paragraph, TooltipTextAlignment.Midl, m_Unit);
		}
		AlignmentAxisStaticInfo alignmentInfo = ConfigRoot.Instance.AlignmentMarksRoot.GetAlignmentInfo(m_Axis);
		yield return new BrickTextVM(alignmentInfo.Description.Text.Capitalize(), TooltipTextType.Simple, TooltipTextAlignment.Midl, m_Unit);
		List<BlueprintMechanicEntityFact> facts = (from f in ConfigRoot.Instance.AlignmentMarksRoot.GetFactsForMark(m_Axis, m_Tier - 1)
			where !(f is BlueprintFeatureBase blueprintFeatureBase) || !blueprintFeatureBase.HideInUI
			select f).ToList();
		if (facts.Count > 0)
		{
			yield return new BrickTitleVM(UIStrings.Instance.CharacterSheet.Features.Text, TooltipTitleType.H2);
			foreach (BlueprintMechanicEntityFact item in facts)
			{
				yield return new BrickFeatureVM(item, isHeader: false, new TooltipTemplateDataProvider(item));
			}
		}
		int num = Mathf.Min(m_Tier, m_RankThresholds.Count - 1);
		IEnumerable<ITooltipBrick> slider = AlignmentTooltipExtensions.GetSlider(m_RankThresholds.GetRange(0, num + 1), m_CurrentValue, m_RankThresholds[num]);
		foreach (ITooltipBrick item2 in slider)
		{
			yield return item2;
		}
	}
}
