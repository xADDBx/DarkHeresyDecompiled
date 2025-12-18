using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Localization;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public static class AlignmentTooltipExtensions
{
	public static string GetGlossaryKeyByDirection(AlignmentAxis direction)
	{
		return direction switch
		{
			AlignmentAxis.Monodominance => "Monodominance", 
			AlignmentAxis.Torian => "Torian", 
			AlignmentAxis.Xanthite => "Xanthite", 
			AlignmentAxis.Xenophilia => "Xenophilia", 
			_ => string.Empty, 
		};
	}

	[Obsolete]
	public static BlueprintAlignmentMark GetSoulMarkWithTier(BlueprintAlignmentMark baseBlueprint, int tier)
	{
		List<BlueprintAlignmentMark> list = (from f in baseBlueprint.ComponentsArray.Select(delegate(BlueprintComponent c)
			{
				AddFactsOnRank obj = c as AddFactsOnRank;
				return (obj == null) ? null : obj.Facts.FirstOrDefault((BlueprintMechanicEntityFact f) => f is BlueprintAlignmentMark);
			})
			select f as BlueprintAlignmentMark).ToList();
		if (tier < 0 || tier > list.Count)
		{
			return null;
		}
		return list[tier - 1];
	}

	[Obsolete]
	public static BlueprintFeature GetSoulMarkFeature(BlueprintAlignmentMark baseBlueprint, int tier)
	{
		AddFactsOnRank obj = baseBlueprint.ComponentsArray[tier - 1] as AddFactsOnRank;
		return ((obj != null) ? obj.Facts.FirstOrDefault((BlueprintMechanicEntityFact f) => !(f is BlueprintAlignmentMark) && f is BlueprintFeature) : null) as BlueprintFeature;
	}

	public static void GetAlignmentInfo(AlignmentAxis axis, BaseUnitEntity unit, out List<int> rankThresholds, out int maxValue, out int currentValue, out int currentTier)
	{
		rankThresholds = ConfigRoot.Instance.AlignmentMarksRoot.GetAlignmentRankThresholds(axis).ToList();
		rankThresholds.RemoveAll((int r) => r < 0);
		rankThresholds.Sort((int r0, int r1) => (r0 >= r1) ? 1 : (-1));
		rankThresholds.Insert(0, 0);
		maxValue = rankThresholds.LastOrDefault();
		currentValue = unit.Alignment.GetAlignmentRank(axis);
		currentTier = unit.Alignment.GetAlignmentRank(axis);
	}

	public static IEnumerable<ITooltipBrick> GetSlider(List<int> rankThresholds, int currentValue, int maxValue)
	{
		Color32 progressbarBonus = UIConfig.Instance.TooltipColors.ProgressbarBonus;
		Color32 defaultColor = UIConfig.Instance.TooltipColors.ProgressbarNeutral;
		List<BrickSliderValueVM> sliderValues = new List<BrickSliderValueVM>
		{
			new BrickSliderValueVM(0, maxValue, currentValue, null, needColor: true, progressbarBonus, new Color32(0, 0, 0, 0))
		};
		LocalizedString endText = null;
		int num = rankThresholds.IndexOf(maxValue);
		if (num >= 0)
		{
			endText = UIUtilityText.GetSoulMarkRankText(num);
		}
		yield return new TooltipBricksGroupStart(hasBackground: false, new TooltipBricksGroupLayoutParams
		{
			LayoutType = TooltipBricksGroupLayoutType.Vertical,
			Padding = new RectOffset(0, 0, -15, -25)
		});
		yield return new TooltipBrickSlider(0, maxValue, currentValue, sliderValues, showValue: false, 40, defaultColor, endText);
		yield return new TooltipBricksGroupEnd();
		yield return new TooltipBrickText($"{UIStrings.Instance.Tooltips.CurrentValue.Text} {currentValue}", TooltipTextType.Centered, isHeader: false, TooltipTextAlignment.Midl, needChangeSize: true);
	}

	public static List<ITooltipBrick> GetFeatureBlock(BlueprintAlignmentMark baseBlueprint, int tier, bool highlight)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		if (tier < 1 || tier > baseBlueprint.ComponentsArray.Length)
		{
			return list;
		}
		if (!(baseBlueprint.ComponentsArray[tier - 1] is AddFactsOnRank addFactsOnRank))
		{
			return list;
		}
		Color32 color = (highlight ? new Color32(130, 174, 115, byte.MaxValue) : new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
		list.Add(new TooltipBricksGroupStart(hasBackground: true, null, color));
		list.Add(new TooltipBrickIconValueStat(UIUtilityText.GetSoulMarkRankText(tier), (addFactsOnRank.RankValue.Value - 1).ToString(), null, TooltipIconValueStatType.Normal, isWhite: false, needChangeSize: false, 18, 18, needChangeColor: true, Color.black, Color.black));
		list.Add(new TooltipBricksGroupEnd());
		list.Add(new TooltipBrickText(GetSoulMarkWithTier(baseBlueprint, tier)?.Description));
		BlueprintFeature soulMarkFeature = GetSoulMarkFeature(baseBlueprint, tier);
		list.Add(new TooltipBricksGroupStart(hasBackground: true, null, new Color32(183, 170, 144, byte.MaxValue)));
		list.Add(new TooltipBrickFeature(soulMarkFeature));
		list.Add(new TooltipBricksGroupEnd());
		return list;
	}
}
