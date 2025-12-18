using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Utility.DotNetExtensions;

namespace Owlcat.Fmw.Blueprints;

public static class BpRefExtension
{
	public static BpRef<T> Reference<T>([CanBeNull] this T blueprint) where T : SimpleBlueprint
	{
		if (blueprint == null)
		{
			return new BpRef<T>();
		}
		return new BpRef<T>(blueprint);
	}

	public static IEnumerable<BpRef<T>> Reference<T>(this IEnumerable<T> list) where T : SimpleBlueprint
	{
		return list.Select(Reference);
	}

	public static IEnumerable<T> Dereference<T>(this IEnumerable<BpRef<T>> list) where T : SimpleBlueprint
	{
		return list.Select((BpRef<T> r) => ((object)r == null) ? null : r.MaybeBlueprint).NotNull();
	}

	public static bool Contains<T>(this IEnumerable<BpRef<T>> list, T blueprint) where T : SimpleBlueprint
	{
		return list.Any((BpRef<T> r) => r != null && r.Is(blueprint));
	}

	public static bool Contains<T>(this IList<BpRef<T>> list, T blueprint) where T : SimpleBlueprint
	{
		for (int i = 0; i < list.Count; i++)
		{
			BpRef<T> bpRef = list[i];
			if ((((object)bpRef != null) ? bpRef.MaybeBlueprint : null) == blueprint)
			{
				return true;
			}
		}
		return false;
	}
}
