using System.Collections.Generic;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Alignments;
using Owlcat.UI;
using TMPro;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateSoulMarkHeader : TooltipBaseTemplate
{
	private readonly AlignmentAxis m_Direction;

	private readonly BlueprintAlignmentMark m_BlueprintAlignmentMark;

	private readonly List<int> m_RankThresholds = new List<int>();

	private readonly int m_CurrentTier;

	private readonly int m_CurrentValue;

	private readonly int m_MaxValue;

	public TooltipTemplateSoulMarkHeader(BaseUnitEntity unit, AlignmentAxis direction)
	{
		m_Direction = direction;
		AlignmentTooltipExtensions.GetAlignmentInfo(direction, unit, out m_RankThresholds, out m_MaxValue, out m_CurrentValue, out m_CurrentTier);
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickTitle(UIUtilityText.GetSoulMarkDirectionText(m_Direction).Text, TooltipTitleType.H1);
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
		string format = UIStrings.Instance.Tooltips.SoulMarkRankDescription;
		int currentTier = m_CurrentTier;
		string title = string.Format(format, currentTier.ToString(), UIUtilityText.GetSoulMarkRankText(m_CurrentTier).Text);
		list.Add(new TooltipBrickTitle(title, TooltipTitleType.H4, TextAlignmentOptions.Left));
		string glossaryKeyByDirection = AlignmentTooltipExtensions.GetGlossaryKeyByDirection(m_Direction);
		list.Add(new TooltipBrickText(UIUtilityEncyclopedy.GetGlossaryEntry(glossaryKeyByDirection).GetDescription().Text, TooltipTextType.Simple, isHeader: false, TooltipTextAlignment.Left, needChangeSize: true));
		list.AddRange(AlignmentTooltipExtensions.GetSlider(m_RankThresholds, m_CurrentValue, m_MaxValue));
		return list;
	}

	private List<ITooltipBrick> GetBodyInfo()
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		if (m_CurrentTier >= 0)
		{
			string format = UIStrings.Instance.Tooltips.SoulMarkRankDescription;
			int currentTier = m_CurrentTier;
			string title = string.Format(format, currentTier.ToString(), UIUtilityText.GetSoulMarkRankText(m_CurrentTier).Text);
			list.Add(new TooltipBrickTitle(title, TooltipTitleType.H4, TextAlignmentOptions.Left));
		}
		string glossaryKeyByDirection = AlignmentTooltipExtensions.GetGlossaryKeyByDirection(m_Direction);
		list.Add(new TooltipBrickText(UIUtilityEncyclopedy.GetGlossaryEntry(glossaryKeyByDirection).GetDescription().Text, TooltipTextType.Simple, isHeader: false, TooltipTextAlignment.Left));
		for (int i = 0; i < m_BlueprintAlignmentMark.ComponentsArray.Length; i++)
		{
			int num = i + 1;
			list.AddRange(AlignmentTooltipExtensions.GetFeatureBlock(m_BlueprintAlignmentMark, num, num == m_CurrentTier));
		}
		return list;
	}
}
