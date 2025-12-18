using System;
using System.Collections.Generic;

namespace Kingmaker.Utility;

public class BugContextComparer : IComparer<BugContext>
{
	private static readonly Dictionary<string, int> TypePriority = new Dictionary<string, int>
	{
		{ "Desync", 0 },
		{ "Coop", 5 },
		{ "Unit", 10 },
		{ "Item", 10 },
		{ "Ability", 10 },
		{ "Dialog", 20 },
		{ "Encounter", 30 },
		{ "Interface", 40 },
		{ "Area", 70 },
		{ "Generic", 199 },
		{ "Debug", 200 }
	};

	private static readonly Dictionary<string, int> UIFeaturePriority = new Dictionary<string, int> { { "", 200 } };

	private const int DefaultPriority = 100;

	private const int MinPriority = 200;

	public int Compare(BugContext alice, BugContext bob)
	{
		int num = CompareTypes(alice, bob);
		if (num != 0)
		{
			return num;
		}
		if (alice.Type == "Interface")
		{
			return CompareUiFeatures(alice, bob);
		}
		return 0;
	}

	private int CompareTypes(BugContext alice, BugContext bob)
	{
		return CompareByPresetPriorityMap(alice, bob, GetTypePriority);
	}

	private int CompareUiFeatures(BugContext alice, BugContext bob)
	{
		return CompareByPresetPriorityMap(alice, bob, GetUiFeaturePriority);
	}

	private int CompareByPresetPriorityMap(BugContext alice, BugContext bob, Func<BugContext, int> getPriority)
	{
		int num = GetExclusivePriority(alice) ?? getPriority(alice);
		int num2 = GetExclusivePriority(bob) ?? getPriority(bob);
		if (num > num2)
		{
			return 1;
		}
		if (num < num2)
		{
			return -1;
		}
		return 0;
	}

	private int? GetExclusivePriority(BugContext context)
	{
		if (context.Type == "Interface" && context.UiFeature == "SystemMap")
		{
			return 60;
		}
		if (context.Type == "Interface" && context.OtherUiFeature == "Bark")
		{
			return 200;
		}
		return null;
	}

	private static int GetTypePriority(BugContext bc)
	{
		return TypePriority.GetValueOrDefault(bc.Type, 100);
	}

	private static int GetUiFeaturePriority(BugContext bc)
	{
		if (bc.UiFeature != null && UIFeaturePriority.TryGetValue(bc.UiFeature, out var value))
		{
			return value;
		}
		return 100;
	}
}
