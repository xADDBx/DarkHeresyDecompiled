using System.Collections.Generic;
using Kingmaker.Utility.StatefulRandom;

namespace Kingmaker.Designers.WarhammerSurfaceCombatPrototype;

public static class PhenomenaListResolver
{
	public static List<BlueprintPsykerRoot.PhenomenaData> ResolveFromRoot(BlueprintPsykerRoot.PhenomenaData[] rootList)
	{
		List<BlueprintPsykerRoot.PhenomenaData> list = new List<BlueprintPsykerRoot.PhenomenaData>(rootList.Length);
		list.AddRange(rootList);
		return list;
	}

	public static BlueprintPsykerRoot.PhenomenaData SelectWeighted(List<BlueprintPsykerRoot.PhenomenaData> list, StatefulRandom random)
	{
		if (list == null || list.Count == 0)
		{
			return null;
		}
		float num = 0f;
		foreach (BlueprintPsykerRoot.PhenomenaData item in list)
		{
			num += item.Weight;
		}
		if (num <= 0f)
		{
			return list[random.Range(0, list.Count)];
		}
		float num2 = (float)((double)random.Range(0, 10000) / 10000.0) * num;
		float num3 = 0f;
		foreach (BlueprintPsykerRoot.PhenomenaData item2 in list)
		{
			num3 += item2.Weight;
			if (num2 < num3)
			{
				return item2;
			}
		}
		return list[list.Count - 1];
	}
}
