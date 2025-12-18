using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.UIUtils;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateCareerProgression : TooltipBaseTemplate
{
	private readonly CareerPathVM m_CareerPath;

	public AutoDisposingList<CareerPathRankEntryVM> RankEntries => m_CareerPath.RankEntries;

	public TooltipTemplateCareerProgression(CareerPathVM careerPath)
	{
		m_CareerPath = careerPath;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		string title = ((!m_CareerPath.IsInLevelupProcess) ? ((string)UIStrings.Instance.CharacterSheet.CareerPathHeader) : ((string)UIStrings.Instance.CharacterSheet.CareerPathHasNewRanksHeader));
		yield return new TooltipBrickTitle(title);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		if (type == TooltipTemplateType.Info && !m_CareerPath.IsUnlocked)
		{
			AddPrerequisites(list);
		}
		if (m_CareerPath.IsInLevelupProcess)
		{
			AddCurrentLevelupRange(list);
		}
		else
		{
			AddAllRanks(list);
		}
		return list;
	}

	private void AddAllRanks(List<ITooltipBrick> bricks)
	{
		foreach (CareerPathRankEntryVM rankEntry in m_CareerPath.RankEntries)
		{
			AddFeaturesGroup(bricks, rankEntry.Features, rankEntry.Selections, string.Format(UIStrings.Instance.CharacterSheet.RankLabel.Text, rankEntry.Rank));
		}
	}

	private void AddCurrentLevelupRange(List<ITooltipBrick> bricks)
	{
		(int Min, int Max) levelRange = m_CareerPath.GetCurrentLevelupRange();
		foreach (CareerPathRankEntryVM item in RankEntries.Where((CareerPathRankEntryVM entry) => levelRange.Min <= entry.Rank && entry.Rank <= levelRange.Max))
		{
			AddFeaturesGroup(bricks, item.Features, item.Selections, string.Format(UIStrings.Instance.CharacterSheet.RankLabel.Text, item.Rank));
		}
	}

	private void AddFeaturesGroup(List<ITooltipBrick> bricks, AutoDisposingList<RankEntryFeatureItemVM> featureVMs, AutoDisposingList<RankEntrySelectionVM> selectionVMs, string header)
	{
		if (featureVMs.Any() || selectionVMs.Any())
		{
			bricks.Add(new TooltipBrickTitle(header, TooltipTitleType.H5));
			float value = (Game.Instance.IsControllerGamepad ? 92f : 72f);
			TooltipBricksGroupLayoutParams layoutParams = new TooltipBricksGroupLayoutParams
			{
				LayoutType = TooltipBricksGroupLayoutType.Grid,
				Padding = new RectOffset(6, 6, -6, 0),
				Spacing = new Vector2(0f, -4f),
				ColumnCount = 1,
				PreferredElementHeight = value
			};
			bricks.Add(new TooltipBricksGroupStart(hasBackground: false, layoutParams));
			bricks.AddRange(from featureVM in featureVMs.NotNull()
				select new TooltipBrickFeature(featureVM.Feature));
			bricks.AddRange(from selectionVM in selectionVMs.NotNull()
				select new TooltipBrickRankEntrySelection(selectionVM));
			bricks.Add(new TooltipBricksGroupEnd());
		}
	}

	private void AddPrerequisites(List<ITooltipBrick> bricks)
	{
		if (m_CareerPath.Prerequisite != null)
		{
			bricks.Add(new TooltipBrickTitle(UIStrings.Instance.Tooltips.Prerequisites, TooltipTitleType.H5));
			bricks.Add(new TooltipBrickPrerequisite(UIUtilityAbilities.GetPrerequisiteEntries(m_CareerPath.Prerequisite)));
		}
	}
}
