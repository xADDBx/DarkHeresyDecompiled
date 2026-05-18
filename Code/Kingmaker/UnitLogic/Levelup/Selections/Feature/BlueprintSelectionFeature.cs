using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.DLC;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Progression;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.Utility.UnityExtensions;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.UnitLogic.Levelup.Selections.Feature;

[Serializable]
[TypeId("7c7ec5faa87f4ea5bc8f3b40932237fd")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintSelectionFeature : BlueprintSelectionWithUI
{
	[Header("Mechanics")]
	public FeatureGroup Group;

	public int MaxRank;

	public PrerequisitesList Prerequisites;

	public bool MeetPrerequisites(BaseUnitEntity unit)
	{
		if (Prerequisites != null)
		{
			return Prerequisites.Meet(unit);
		}
		return true;
	}

	public IEnumerable<FeatureSelectionItem> GetSelectionItems([NotNull] BaseUnitEntity unit, [CanBeNull] BlueprintPath path)
	{
		if (!MeetPrerequisites(unit))
		{
			return Enumerable.Empty<FeatureSelectionItem>();
		}
		IEnumerable<AddFeaturesToLevelUp> first = (from i in path?.GetComponents<AddFeaturesToLevelUp>()
			where i.Group == Group
			select i) ?? Array.Empty<AddFeaturesToLevelUp>();
		IEnumerable<AddFeaturesToLevelUp> components = unit.Facts.GetComponents((AddFeaturesToLevelUp i) => i.Group == Group && !(i.OwnerBlueprint is BlueprintPath));
		List<AddFeaturesToLevelUp> list;
		using (first.Concat(components).ToPooledList(out list))
		{
			if (list.Any((AddFeaturesToLevelUp f) => f == null))
			{
				Debug.LogError("Features contains NULL: " + unit.Blueprint?.name + " path: " + path?.name);
			}
			return list.SelectMany((AddFeaturesToLevelUp i) => from f in i.Features
				where f != null && !f.IsDlcRestricted()
				select f into feature
				select CreateItem(feature, i)).ToArray();
		}
		FeatureSelectionItem CreateItem(BlueprintFeature feature, AddFeaturesToLevelUp pool)
		{
			return new FeatureSelectionItem(feature, pool, MaxRank);
		}
	}
}
