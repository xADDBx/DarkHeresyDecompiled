using System;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Levelup.Selections.Prerequisites;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.UnitLogic.Levelup.Selections.Feature;

public class SelectionStateFeature : SelectionState
{
	[CanBeNull]
	private FeatureSelectionItem[] m_Items;

	[CanBeNull]
	private (FeatureSelectionItem Item, CalculatedPrerequisite Prerequisite)[] m_CalculatedPrerequisites;

	public FeatureSelectionItem? SelectionItem { get; private set; }

	public new BlueprintSelectionFeature Blueprint => (BlueprintSelectionFeature)base.Blueprint;

	public ReadonlyList<FeatureSelectionItem> Items => m_Items;

	public SelectionStateFeature([NotNull] LevelUpManager manager, [NotNull] BlueprintSelectionFeature blueprint, [NotNull] BlueprintPath path, int pathRank)
		: base(manager, blueprint, path, pathRank)
	{
		UpdateItems();
	}

	private void UpdateItems()
	{
		m_Items = Blueprint.GetSelectionItems(base.Manager.PreviewUnit, base.Path).ToArray();
		if (!base.Manager.AutoCommit)
		{
			m_CalculatedPrerequisites = m_Items.Select((FeatureSelectionItem i) => (i: i, CalculatedPrerequisite.Calculate(this, i, base.Manager.PreviewUnit))).ToArray();
		}
	}

	public bool CanSelect(FeatureSelectionItem selectionItem)
	{
		if (!Items.HasItem((FeatureSelectionItem i) => i == selectionItem))
		{
			PFLog.LevelUp.Error("Can't select item which not belongs to selection");
			return false;
		}
		CalculatedPrerequisite calculatedPrerequisite = GetCalculatedPrerequisite(selectionItem);
		return calculatedPrerequisite == null || calculatedPrerequisite.Value;
	}

	[CanBeNull]
	public CalculatedPrerequisite GetCalculatedPrerequisite(FeatureSelectionItem selectionItem)
	{
		if (base.Manager.AutoCommit)
		{
			if (!selectionItem.MeetRankPrerequisites(base.Manager.PreviewUnit) || (selectionItem.Feature.Prerequisites != null && !selectionItem.Feature.Prerequisites.Meet(base.Manager.PreviewUnit)))
			{
				return CalculatedPrerequisiteSimple.False;
			}
			return CalculatedPrerequisiteSimple.True;
		}
		return m_CalculatedPrerequisites?.FirstItem<(FeatureSelectionItem, CalculatedPrerequisite)>(((FeatureSelectionItem Item, CalculatedPrerequisite Prerequisite) i) => i.Item == selectionItem).Item2;
	}

	[CanBeNull]
	public FeatureGroup? GetSelectionItemFeatureGroup()
	{
		return SelectionItem?.GetSourceFeatureGroup();
	}

	public bool Select(FeatureSelectionItem selectionItem)
	{
		if (CanSelect(selectionItem))
		{
			SelectionItem = selectionItem;
			NotifySelectionChanged();
			return true;
		}
		return false;
	}

	public void ClearSelection()
	{
		if (SelectionItem.HasValue)
		{
			SelectionItem = null;
			NotifySelectionChanged();
		}
	}

	protected override bool IsMadeInternal()
	{
		return SelectionItem.HasValue;
	}

	protected override bool IsValidInternal()
	{
		if (SelectionItem.HasValue)
		{
			return CanSelect(SelectionItem.Value);
		}
		return true;
	}

	protected override bool CanSelectAnyInternal()
	{
		return m_Items.HasItem(CanSelect);
	}

	protected override void ApplyInternal(BaseUnitEntity unit)
	{
		BlueprintFeature feature = SelectionItem.Value.Feature;
		if (feature is BlueprintCareerPath careerPath)
		{
			ApplyCareerPath(unit, careerPath);
			return;
		}
		if (!(feature is BlueprintPath))
		{
			ApplyFeature(unit, feature, SelectionItem.Value.SourceBlueprint);
			return;
		}
		throw new InvalidOperationException($"Can't apply selection of {feature}");
	}

	private void ApplyFeature(BaseUnitEntity unit, BlueprintFeature featureBlueprint, BlueprintScriptableObject sourceBlueprint)
	{
		Kingmaker.UnitLogic.Feature feature = unit.Progression.Features.Add(featureBlueprint);
		if (feature == null)
		{
			throw new Exception($"Failed to apply selection {Blueprint} ({featureBlueprint}) to unit {unit}: result feature is null");
		}
		feature.AddSource(base.Path, sourceBlueprint, base.PathRank);
		unit.Progression.AddFeatureSelection(base.Path, base.PathRank, Blueprint, feature.Blueprint, feature.Rank);
	}

	private void ApplyCareerPath(BaseUnitEntity unit, BlueprintCareerPath careerPath)
	{
		if (unit.Progression.GetPathRank(careerPath) > 0)
		{
			throw new InvalidOperationException($"Can't apply selection of {careerPath} to {unit}, career path rank > 0");
		}
		BlueprintPath.RankEntry rankEntry = careerPath.GetRankEntry(1);
		if (rankEntry != null && rankEntry.Selections.Length > 0)
		{
			throw new InvalidOperationException($"Can't apply selection of {careerPath} to {unit}, selections on 1st rank isn't empty");
		}
		unit.Progression.AddPathRank(careerPath);
		if (rankEntry == null)
		{
			return;
		}
		foreach (BlueprintFeature feature in rankEntry.Features)
		{
			unit.Progression.Features.Add(feature)?.AddSource(careerPath, feature, 1);
		}
	}

	protected override void InvalidateInternal()
	{
		UpdateItems();
	}
}
