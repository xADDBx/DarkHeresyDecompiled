using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateSoulMarkFeature : TooltipBaseTemplate
{
	private readonly int m_Tier;

	private readonly List<int> m_RankThresholds = new List<int>();

	private readonly int m_CurrentValue;

	private readonly AlignmentAxis m_AlignmentAxis;

	private readonly AlignmentAxis? m_MainDirection;

	private readonly BlueprintFeature m_Feature;

	public TooltipTemplateSoulMarkFeature(BaseUnitEntity unit, AlignmentAxis direction, int tier, AlignmentAxis? mainDirection)
	{
		m_Tier = tier;
		m_MainDirection = mainDirection;
		m_AlignmentAxis = direction;
		if (ConfigRoot.Instance.AlignmentMarksRoot.GetMarksAmount(direction) < tier || tier < 0)
		{
			PFLog.Alignment.Error($"Tier error: {tier} of {unit.Blueprint.name} in {mainDirection}");
		}
		else
		{
			AlignmentTooltipExtensions.GetAlignmentInfo(direction, unit, out m_RankThresholds, out var _, out m_CurrentValue, out var _);
		}
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		string format = UIStrings.Instance.Tooltips.SoulMarkRankDescription;
		int tier = m_Tier;
		string title = string.Format(format, tier.ToString(), UIUtilityText.GetSoulMarkRankText(m_Tier).Text);
		yield return new TooltipBrickTitle(title, TooltipTitleType.H1);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		bool isLocked = m_MainDirection.HasValue && m_MainDirection.Value != m_AlignmentAxis && m_Tier > 2;
		if (!m_MainDirection.HasValue || m_MainDirection.Value == m_AlignmentAxis)
		{
			yield return new TooltipBrickText(UIStrings.Instance.Tooltips.SoulMarkMayBeLocked, TooltipTextType.Simple, isHeader: false, TooltipTextAlignment.Midl, needChangeSize: true);
		}
		else if (isLocked)
		{
			yield return new TooltipBrickText(UIStrings.Instance.Tooltips.SoulMarkIsLocked, TooltipTextType.Simple, isHeader: false, TooltipTextAlignment.Midl, needChangeSize: true);
		}
		AlignmentAxisStaticInfo staticInfo = ConfigRoot.Instance.AlignmentMarksRoot.GetAlignmentInfo(m_AlignmentAxis);
		yield return new TooltipBrickText(staticInfo.Description, TooltipTextType.Simple, isHeader: false, TooltipTextAlignment.Midl, needChangeSize: true);
		if (!isLocked)
		{
			yield return new TooltipBricksGroupStart(hasBackground: true, null, Color.white);
			yield return new TooltipBrickFeature(staticInfo.Name, staticInfo.Icon);
			yield return new TooltipBricksGroupEnd();
		}
		int num = Mathf.Min(m_Tier, m_RankThresholds.Count - 1);
		IEnumerable<ITooltipBrick> slider = AlignmentTooltipExtensions.GetSlider(m_RankThresholds.GetRange(0, num + 1), m_CurrentValue, m_RankThresholds[num]);
		foreach (ITooltipBrick item in slider)
		{
			yield return item;
		}
	}
}
