using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Progression.Features.Advancements;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.UnitLogic.Levelup.Selections.Stats;

public sealed class SelectionStateStats : SelectionState
{
	private readonly Dictionary<StatType, int> _pointsInitial = new Dictionary<StatType, int>();

	private readonly Dictionary<StatType, int> _pointsSpent = new Dictionary<StatType, int>();

	public new BlueprintSelectionStats Blueprint => (BlueprintSelectionStats)base.Blueprint;

	private BaseUnitEntity PreviewUnit => base.Manager.PreviewUnit;

	public IEnumerable<StatType> Stats => Blueprint.Advancements.Select((BlueprintStatAdvancement i) => i.Stat);

	public int PointsTotal => Blueprint.PointsTotal;

	public int PointsSpentTotal => _pointsSpent.Values.Sum();

	public int MaxPointsPerStat => Blueprint.MaxPointsPerStat;

	public SelectionStateStats([NotNull] LevelUpManager manager, [NotNull] BlueprintSelectionStats blueprint, [NotNull] BlueprintPath path, int pathRank)
		: base(manager, blueprint, path, pathRank)
	{
		RecalculatePoints();
	}

	private void RecalculatePoints()
	{
		foreach (StatType stat in Stats)
		{
			BlueprintStatAdvancement advancementBlueprint = GetAdvancementBlueprint(stat);
			if (advancementBlueprint != null)
			{
				int num = (_pointsInitial[stat] = PreviewUnit.Progression.Features.GetRank(advancementBlueprint));
				int num2 = num;
				int num3 = _pointsSpent.Get(stat, 0);
				if (num2 + num3 > MaxPointsPerStat)
				{
					_pointsSpent[stat] = MaxPointsPerStat - num2;
				}
			}
		}
	}

	[CanBeNull]
	private BlueprintStatAdvancement GetAdvancementBlueprint(StatType statType)
	{
		return Blueprint.Advancements.FirstOrDefault((BlueprintStatAdvancement i) => i.Stat == statType);
	}

	public int GetPointsSpent(StatType statType)
	{
		return _pointsSpent.Get(statType, 0);
	}

	public int GetPointsInitial(StatType statType)
	{
		return _pointsInitial.Get(statType, 0);
	}

	public int GetPointsTotal(StatType statType)
	{
		return GetPointsInitial(statType) + GetPointsSpent(statType);
	}

	public void SetPoints(StatType statType, int points)
	{
		int num = points - GetPointsTotal(statType);
		if (num > 0)
		{
			AddPoints(statType, num);
		}
		else if (num < 0)
		{
			RemovePoints(statType, -num);
		}
	}

	public void AddPoints(StatType statType, int points = 1)
	{
		if (GetAdvancementBlueprint(statType) != null)
		{
			int pointsSpent = GetPointsSpent(statType);
			int pointsTotal = GetPointsTotal(statType);
			points = Math.Min(points, MaxPointsPerStat - pointsTotal);
			points = Math.Min(points, PointsTotal - PointsSpentTotal);
			if (points > 0)
			{
				_pointsSpent[statType] = pointsSpent + points;
				NotifySelectionChanged();
			}
		}
	}

	public void RemovePoints(StatType statType, int points = 1)
	{
		if (GetAdvancementBlueprint(statType) != null)
		{
			int pointsSpent = GetPointsSpent(statType);
			points = Math.Min(points, pointsSpent);
			if (points > 0)
			{
				_pointsSpent[statType] = pointsSpent - points;
				NotifySelectionChanged();
			}
		}
	}

	protected override bool ShouldApplyInternal()
	{
		return PointsSpentTotal > 0;
	}

	protected override bool IsMadeInternal()
	{
		return PointsSpentTotal == PointsTotal;
	}

	protected override bool IsValidInternal()
	{
		return Stats.All((StatType statType) => GetPointsTotal(statType) <= MaxPointsPerStat);
	}

	protected override bool CanSelectAnyInternal()
	{
		return _pointsInitial.Values.Any((int i) => i < MaxPointsPerStat);
	}

	protected override void ApplyInternal(BaseUnitEntity unit)
	{
		foreach (StatType stat in Stats)
		{
			int pointsSpent = GetPointsSpent(stat);
			Kingmaker.UnitLogic.Feature feature = unit.Progression.Features.SetRank(GetAdvancementBlueprint(stat), GetPointsInitial(stat) + pointsSpent);
			if (feature != null)
			{
				bool flag = feature.Sources.HasItem((EntityFactSource i) => i.Blueprint == base.Path && i.PathFeatureSource == base.Path && i.PathRank == base.PathRank);
				if (pointsSpent > 0 && !flag)
				{
					feature.AddSource(base.Path, base.Path, base.PathRank);
				}
			}
		}
	}

	protected override void InvalidateInternal()
	{
		RecalculatePoints();
	}
}
