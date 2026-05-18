using System.Collections.Generic;
using System.Linq;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Alignments;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public static class AlignmentTooltipExtensions
{
	public static void GetAlignmentInfo(AlignmentAxis axis, BaseUnitEntity unit, out List<int> rankThresholds, out int maxValue, out int currentValue, out int currentTier)
	{
		rankThresholds = ConfigRoot.Instance.AlignmentMarksRoot.GetAlignmentRankThresholds(axis).ToList();
		rankThresholds.Insert(0, 0);
		maxValue = rankThresholds.LastOrDefault();
		int currentRank = unit.Alignment.GetAlignmentRank(axis);
		currentValue = currentRank;
		currentTier = rankThresholds.FindLastIndex((int rt) => rt <= currentRank);
	}

	public static IEnumerable<ITooltipBrick> GetSlider(List<int> rankThresholds, int currentValue, int maxValue)
	{
		Color32 progressbarBonus = UIConfig.Instance.TooltipColors.ProgressbarBonus;
		Color32 progressbarNeutral = UIConfig.Instance.TooltipColors.ProgressbarNeutral;
		List<SliderValuesVM> sliderValueVMs = new List<SliderValuesVM>
		{
			new SliderValuesVM(0, maxValue, currentValue, null, needColor: true, progressbarBonus, new Color32(0, 0, 0, 0))
		};
		string maxValueText = null;
		int num = rankThresholds.IndexOf(maxValue);
		if (num >= 0)
		{
			maxValueText = UIUtilityAlignment.GetAlignmentRankText(num);
		}
		List<TooltipBrickVM> children = new List<TooltipBrickVM>
		{
			new BrickSliderVM(0, maxValue, currentValue, sliderValueVMs, showValue: false, progressbarNeutral, maxValueText)
		};
		yield return new BricksGroupOneColumnVM(children);
		yield return new BrickTextVM($"{UIStrings.Instance.Tooltips.CurrentValue.Text} {currentValue}", TooltipTextType.Centered);
	}
}
