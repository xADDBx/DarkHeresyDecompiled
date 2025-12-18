using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Progression.Features.Advancements;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoBaseAbilityScoresBlockVM : CharInfoComponentWithLevelUpVM
{
	public readonly AutoDisposingList<CharInfoStatVM> Stats = new AutoDisposingList<CharInfoStatVM>();

	private readonly ReactiveCommand<Unit> m_OnStatsUpdated = new ReactiveCommand<Unit>();

	public Observable<Unit> OnStatsUpdated => m_OnStatsUpdated;

	protected virtual List<StatType> StatsTypes { get; }

	protected CharInfoBaseAbilityScoresBlockVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit, ReadOnlyReactiveProperty<LevelUpManager> levelUpManager, StatsContainer _)
		: base(unit, levelUpManager)
	{
		base.PreviewUnit.Subscribe(delegate
		{
			HandleUpdatePreviewUnit();
		}).AddTo(this);
	}

	protected CharInfoBaseAbilityScoresBlockVM()
		: base(null, null)
	{
	}

	protected CharInfoBaseAbilityScoresBlockVM(StatsContainer stats)
		: this(null, null, stats)
	{
	}

	protected CharInfoBaseAbilityScoresBlockVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit, ReadOnlyReactiveProperty<LevelUpManager> levelUpManager)
		: this(unit, levelUpManager, unit.CurrentValue.Stats.Container)
	{
	}

	private void HandleUpdatePreviewUnit()
	{
		RefreshData();
	}

	public override void HandleUICommitChanges()
	{
		RefreshData();
	}

	protected override void RefreshData()
	{
		base.RefreshData();
		FillStats(Unit.CurrentValue.Stats.Container, base.PreviewUnit.CurrentValue?.Stats.Container);
	}

	protected void FillStats(StatsContainer stats, StatsContainer previewStats)
	{
		ClearStats();
		foreach (StatType item2 in StatsTypes.OrderBy((StatType s) => StatTypeHelper.DisplayOrder.IndexOf(s)))
		{
			ModifiableValue statOptional = stats.GetStatOptional(item2);
			if (statOptional != null)
			{
				ModifiableValue previewStat = previewStats?.GetStat(item2);
				CharInfoStatVM item = new CharInfoStatVM(statOptional, previewStat).AddTo(this);
				Stats.Add(item);
			}
		}
		UpdateRecommendedMarks();
		m_OnStatsUpdated.Execute();
	}

	private void ClearStats()
	{
		Stats.Clear();
	}

	private void UpdateRecommendedMarks()
	{
		List<StatType> selectedCareerRecommendedStats = UtilityChargen.GetSelectedCareerRecommendedStats<BlueprintSkillAdvancement>(Unit.CurrentValue);
		SetRecommendedMarks(selectedCareerRecommendedStats);
	}

	public void SetRecommendedMarks(List<StatType> recommendedSkills)
	{
		foreach (CharInfoStatVM stat in Stats)
		{
			stat.UpdateRecommendedMark(recommendedSkills);
		}
	}
}
