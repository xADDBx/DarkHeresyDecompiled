using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;

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

	public static List<(T item, string propertyPath)> FindAllInBlueprintWithPaths<T>(SimpleBlueprint blueprint)
	{
		return new List<(T, string)>();
	}

	public static List<(T item, string propertyPath)> FindAllInBlueprintsWithPaths<T, K>() where T : Element where K : BlueprintScriptableObject
	{
		return new List<(T, string)>();
	}

	public static List<(T item, string propertyPath)> FindAllWithPaths<T>() where T : Element
	{
		return FindAllInBlueprintsWithPaths<T, BlueprintScriptableObject>();
	}

	[CanBeNull]
	public static string FindPathInOwnerBlueprint<T>(T element) where T : Element
	{
		return (from pair in FindAllInBlueprintWithPaths<T>(element.Owner)
			where pair.item == element
			select pair into bpp
			select bpp.propertyPath).FirstOrDefault();
	}

	private static IEnumerable<T> Parse<T>(string id, string json)
	{
		yield break;
	}
}
