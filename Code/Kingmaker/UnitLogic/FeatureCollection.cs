using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Selection;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;

namespace Kingmaker.UnitLogic;

public class FeatureCollection : MechanicEntityFactsCollection<Feature>
{
	public BaseUnitEntity Owner => base.Manager?.Owner as BaseUnitEntity;

	[JsonConstructor]
	public FeatureCollection()
	{
	}

	public IEnumerator<Feature> GetEnumerator()
	{
		return base.Enumerable.GetEnumerator();
	}

	public int GetRank(BlueprintFeature feature)
	{
		return base.Enumerable.FirstOrDefault((Feature f) => f.Blueprint == feature)?.Rank ?? 0;
	}

	public Feature Get(BlueprintFeature blueprint, FeatureParam param = null)
	{
		return base.RawFacts.FirstItem((Feature i) => i.Blueprint == blueprint && i.Param == param);
	}

	[CanBeNull]
	public Feature Add(BlueprintFeature blueprint, MechanicsContext parentContext = null, int rank = 1)
	{
		return base.Manager.Add(new Feature(blueprint, parentContext, rank));
	}

	[CanBeNull]
	public Feature SetRank(BlueprintFeature blueprint, int rank)
	{
		Feature feature = Get(blueprint);
		if (feature == null)
		{
			if (rank <= 0)
			{
				return null;
			}
			return Add(blueprint, null, rank);
		}
		int num = rank - feature.Rank;
		if (num > 0)
		{
			feature.AddRank(num);
		}
		else if (num < 0)
		{
			feature.RemoveRank(-num);
		}
		return feature;
	}

	protected override Feature PrepareFactForAttach(Feature fact)
	{
		Feature feature = base.RawFacts.FirstItem((Feature i) => i.Blueprint == fact.Blueprint);
		if (feature != null)
		{
			feature.AddRank(fact.Rank);
			return feature;
		}
		return fact;
	}

	protected override Feature PrepareFactForDetach(Feature fact)
	{
		if (fact.Manager == base.Manager)
		{
			if (fact.GetRank() < 2)
			{
				return fact;
			}
			fact.RemoveRank();
		}
		return null;
	}
}
