using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Gameplay.Features.Experience;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Owlcat.Fmw.Blueprints;

namespace Kingmaker.Blueprints;

[Serializable]
public sealed class BrokenBuffsCollection
{
	[Serializable]
	public class DifficultyEntry
	{
		public UnitDifficultyType DifficultyType;

		public List<Entry> Weights = new List<Entry>();
	}

	[Serializable]
	public class Entry
	{
		public BpRef<BlueprintBuff> Buff;

		public int Weight;
	}

	public List<DifficultyEntry> Entries = new List<DifficultyEntry>();

	public bool TryGetBuffForDifficulty(UnitDifficultyType type, out BpRef<BlueprintBuff> buff)
	{
		buff = null;
		if (!Entries.TryFind((DifficultyEntry e) => e.DifficultyType == type, out var result))
		{
			return false;
		}
		if (result.Weights.Count == 0)
		{
			return false;
		}
		int maxExclusive = result.Weights.Sum((Entry w) => w.Weight);
		int num = PFStatefulRandom.Blueprints.Range(0, maxExclusive);
		int num2 = 0;
		foreach (Entry weight in result.Weights)
		{
			num2 += weight.Weight;
			if (num < num2)
			{
				buff = weight.Buff;
				return true;
			}
		}
		return buff != null;
	}
}
