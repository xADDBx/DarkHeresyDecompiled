using System.Collections.Generic;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Alignments;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateAlignmentHeader : TooltipBaseTemplate
{
	private readonly AlignmentAxis m_Axis;

	private readonly List<int> m_RankThresholds;

	private readonly int m_CurrentTier;

	private readonly int m_CurrentValue;

	private readonly int m_MaxValue;

	public TooltipTemplateAlignmentHeader(BaseUnitEntity unit, AlignmentAxis axis)
	{
		m_Axis = axis;
		AlignmentTooltipExtensions.GetAlignmentInfo(axis, unit, out m_RankThresholds, out m_MaxValue, out m_CurrentValue, out m_CurrentTier);
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		string title = ((m_CurrentTier > 0) ? string.Format(UIStrings.Instance.Alignment.AlignmentRankTitle.Text, UIUtilityAlignment.GetAlignmentDirectionText(m_Axis).Text, UIUtilityAlignment.GetAlignmentRankText(m_CurrentTier)) : UIUtilityAlignment.GetAlignmentDirectionText(m_Axis).Text);
		yield return new BrickTitleVM(title, TooltipTitleType.H1);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		return GetBodyBricks(type);
	}

	private IEnumerable<ITooltipBrick> GetBodyBricks(TooltipTemplateType type)
	{
		return type switch
		{
			TooltipTemplateType.Tooltip => GetBodyTooltip(), 
			TooltipTemplateType.Info => GetBodyInfo(), 
			_ => new List<ITooltipBrick>(), 
		};
	}

	private List<ITooltipBrick> GetBodyTooltip()
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		AlignmentAxisStaticInfo alignmentInfo = ConfigRoot.Instance.AlignmentMarksRoot.GetAlignmentInfo(m_Axis);
		list.Add(new BrickTextVM(alignmentInfo.Description.Text.Capitalize(), TooltipTextType.Simple, TooltipTextAlignment.Left));
		list.AddRange(AlignmentTooltipExtensions.GetSlider(m_RankThresholds, m_CurrentValue, m_MaxValue));
		return list;
	}

	private List<ITooltipBrick> GetBodyInfo()
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		AlignmentAxisStaticInfo alignmentInfo = ConfigRoot.Instance.AlignmentMarksRoot.GetAlignmentInfo(m_Axis);
		list.Add(new BrickTextVM(alignmentInfo.Description.Text.Capitalize(), TooltipTextType.Simple, TooltipTextAlignment.Left));
		list.AddRange(AlignmentTooltipExtensions.GetSlider(m_RankThresholds, m_CurrentValue, m_MaxValue));
		return list;
	}
}
