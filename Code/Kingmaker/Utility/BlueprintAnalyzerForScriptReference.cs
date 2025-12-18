using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;

namespace Kingmaker.Utility;

public static class BlueprintAnalyzerForScriptReference
{
	public static List<T> FindAllInBlueprint<T>(BlueprintScriptableObject blueprint)
	{
		return new List<T>();
	}

	public static List<T> FindAllInBlueprints<T, K>() where K : BlueprintScriptableObject
	{
		return new HashSet<T>().ToList();
	}

	public static List<T> FindAll<T>()
	{
		return FindAllInBlueprints<T, BlueprintScriptableObject>();
	}

	private static IEnumerable<T> Parse<T>(string id, string json)
	{
		yield break;
	}
}
