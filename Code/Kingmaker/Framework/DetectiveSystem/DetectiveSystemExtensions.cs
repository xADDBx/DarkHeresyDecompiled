using System.Linq;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Fmw.Blueprints;

namespace Kingmaker.Framework.DetectiveSystem;

public static class DetectiveSystemExtensions
{
	private static DetectiveSystem DetectiveSystem => Game.Instance.DetectiveSystem;

	public static bool IsRefuted(this BlueprintConclusion conclusion)
	{
		if (!conclusion.Refutations.Dereference().Any(DetectiveSystem.HasItemExcludingHidden))
		{
			return conclusion.Sources.HasItem((BlueprintConclusion.Source i) => (i.Item1.Blueprint is BlueprintConclusion conclusion2 && conclusion2.IsRefuted()) || (i.Item2.Blueprint is BlueprintConclusion conclusion3 && conclusion3.IsRefuted()));
		}
		return true;
	}

	public static bool IsUnlocked(this LinkedClue linkedClue)
	{
		return linkedClue.UnlockCondition.Check();
	}

	public static bool IsUnlocked(this BlueprintConclusion conclusion)
	{
		return conclusion.UnlockCondition.Check();
	}
}
