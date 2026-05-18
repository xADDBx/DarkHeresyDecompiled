using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Levelup;
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

	protected CharInfoBaseAbilityScoresBlockVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit, ReadOnlyReactiveProperty<LevelUpManager> levelUpManager)
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

	protected CharInfoBaseAbilityScoresBlockVM(BaseUnitEntity unit)
		: this(null, null)
	{
		FillStats(unit, null);
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
		FillStats(Unit.CurrentValue, base.PreviewUnit.CurrentValue);
	}

	private void FillStats(BaseUnitEntity entity, BaseUnitEntity previewEntity)
	{
		if (entity == null)
		{
			ClearStats();
			return;
		}
		if (Stats.Count > 0 && Stats.Count == StatsTypes.Count)
		{
			foreach (CharInfoStatVM stat in Stats)
			{
				stat.UpdateEntity(entity, previewEntity);
			}
			return;
		}
		ClearStats();
		foreach (StatType item2 in StatsTypes.OrderBy((StatType s) => StatTypeHelper.DisplayOrder.IndexOf(s)))
		{
			CharInfoStatVM item = ((previewEntity != null) ? new CharInfoStatVM(entity, item2, previewEntity).AddTo(this) : new CharInfoStatVM(entity, item2, showPermanentValue: false).AddTo(this));
			Stats.Add(item);
		}
		m_OnStatsUpdated.Execute(R3.Unit.Default);
	}

	private void ClearStats()
	{
		Stats.Clear();
	}
}
