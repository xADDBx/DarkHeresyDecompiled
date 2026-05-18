using System.Collections.Generic;
using System.Linq;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.Localization;

namespace Kingmaker.Code.View.Bridge.Utils;

public static class UtilityDetective
{
	private static DetectiveSystem Detective => Game.Instance.DetectiveSystem;

	public static HashSet<BlueprintClue> GetOpenedConnectedClues(BlueprintClue target)
	{
		List<BlueprintClue> list = (from c in Detective.GetOpenedAddendumsFor(target).SelectMany((BlueprintClueAddendum a) => a.LinkedClues)
			where c.IsUnlocked()
			select c.Clue.Blueprint into c
			where Detective.HasClueExcludingHidden(c)
			select c).ToList();
		list.AddRange(from lc in target.LinkedClues
			select lc.Clue.Blueprint into c
			where Detective.HasClueExcludingHidden(c)
			select c);
		foreach (BlueprintClue availableClue in Detective.GetAvailableClues(target.ParentCase))
		{
			if (availableClue != target)
			{
				if ((from c in Detective.GetOpenedAddendumsFor(availableClue).SelectMany((BlueprintClueAddendum a) => a.LinkedClues)
					where Detective.HasClueExcludingHidden(c.Clue) && c.IsUnlocked()
					select c).Any((LinkedClue c) => c.Clue.Blueprint == target))
				{
					list.Add(availableClue);
				}
				if ((from lc in availableClue.LinkedClues
					select lc.Clue.Blueprint into c
					where Detective.HasClueExcludingHidden(c)
					select c).Contains(target))
				{
					list.Add(availableClue);
				}
			}
		}
		return list.ToHashSet();
	}

	public static LocalizedString GetAnswerDegreeDescription(BlueprintCaseAnswer answer)
	{
		if (!Game.Instance.DetectiveSystem.TryGetAnswerDegree(answer, out var degree))
		{
			return null;
		}
		return answer.DegreeProgression.ElementAt(degree).Description;
	}
}
