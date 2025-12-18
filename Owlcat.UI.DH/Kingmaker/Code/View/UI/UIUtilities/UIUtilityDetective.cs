using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.UI.MVVM.DetectiveJournal;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.Localization;
using Kingmaker.UI;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Fmw.Blueprints;

namespace Kingmaker.Code.View.UI.UIUtilities;

public static class UIUtilityDetective
{
	[ThreadStatic]
	private static int s_GetUIDataDepth;

	public static DetectiveSystem Detective => Game.Instance.DetectiveSystem;

	public static ExaminedDetectiveData ExaminedDetectiveData => Game.Instance.Player.UISettings.DetectiveSystemData.ExaminedDetectiveData;

	public static AnswerDebugValues DebugFlags => UIConfig.Instance.DetectiveConfig.AnswerDebugValues;

	public static List<BlueprintClue> GetOpenedCluesFor(BlueprintCase blueprintCase)
	{
		return blueprintCase.Clues.Where((BpRef<BlueprintClue> caseClue) => Game.Instance.DetectiveSystem.HasClue(caseClue)).Dereference().ToList();
	}

	public static List<T> GetOpenedCaseItemsFor<T>(BlueprintCase blueprintCase) where T : BlueprintCaseItem
	{
		return (from item in blueprintCase.AllItems.OfType<T>()
			where Game.Instance.DetectiveSystem.HasItem(item)
			select item).ToList();
	}

	public static T GetOverride<T>(this T caseItem) where T : BlueprintCaseItem
	{
		try
		{
			s_GetUIDataDepth++;
			if (s_GetUIDataDepth > 100)
			{
				throw new Exception(string.Format("{0}: recursive links in {1} list", caseItem, "Overrides"));
			}
			foreach (T overriddenBlueprint in GetOverriddenBlueprints(caseItem))
			{
				if (Detective.HasItem(overriddenBlueprint))
				{
					return overriddenBlueprint.GetOverride();
				}
			}
			return caseItem;
		}
		finally
		{
			s_GetUIDataDepth--;
		}
	}

	public static List<T> RemoveOverrides<T>(this List<T> caseItems) where T : BlueprintCaseItem
	{
		if (!caseItems.Any())
		{
			return caseItems;
		}
		HashSet<T> overriddenBlueprints = caseItems.SelectMany(GetOverriddenBlueprints).ToHashSet();
		return caseItems.Where((T item) => !overriddenBlueprints.Contains(item)).ToList();
	}

	public static List<T> RemoveLowerTier<T>(this List<T> caseItems) where T : BlueprintCaseItem
	{
		return caseItems.Select((T c) => c.GetOverride()).Distinct().ToList();
	}

	private static IEnumerable<T> GetOverriddenBlueprints<T>(T item) where T : BlueprintCaseItem
	{
		if (!(item is BlueprintClue blueprintClue))
		{
			if (item is BlueprintClueAddendum blueprintClueAddendum)
			{
				return blueprintClueAddendum.Overrides.Select((BlueprintClueAddendum.Override o) => o.Addendum.Blueprint).Cast<T>();
			}
			return Enumerable.Empty<T>();
		}
		return blueprintClue.Overrides.Select((BlueprintClue.Override o) => o.Clue.Blueprint).Cast<T>();
	}

	public static List<BlueprintClue> GetUnknownClues()
	{
		return Game.Instance.DetectiveSystem.GetUnknownClues().ToList().RemoveOverrides();
	}

	public static bool HasUnknownClues()
	{
		return GetUnknownClues().Any((BlueprintClue c) => ExaminedDetectiveData.ExaminedClues.IsEntityNew(c));
	}

	public static List<BlueprintClue> GetAllConnectedClues(BlueprintClue target)
	{
		return (from c in target.Addendums.SelectMany((BpRef<BlueprintClueAddendum> a) => a.Blueprint.LinkedClues)
			where c.IsUnlocked()
			select c.Clue.Blueprint).ToList();
	}

	public static DeductionState GetDeductionState(BlueprintCaseItem caseItem)
	{
		List<BlueprintConclusion> second = (from c in caseItem.PossibleConclusions
			where Enumerable.Any(c.Blueprint.Sources, (BlueprintConclusion.Source s) => s.Item1 == caseItem)
			select c into r
			select r.Blueprint).ToList();
		IEnumerable<BlueprintConclusion> source = Detective.GetAvailableConclusions(caseItem.ParentCase).Intersect<BlueprintConclusion>(second);
		if (!source.Any())
		{
			return DeductionState.None;
		}
		if (!source.Any((BlueprintConclusion d) => Detective.HasConclusion(d)))
		{
			return DeductionState.NewDeduction;
		}
		if (!source.Any((BlueprintConclusion d) => ExaminedDetectiveData.ExaminedConclusions.IsEntityNew(d)))
		{
			return DeductionState.HasDeduction;
		}
		return DeductionState.NewDeductionVariant;
	}

	public static List<BlueprintConclusion> GetConclusionsFor(BlueprintCaseItem caseItem)
	{
		List<BlueprintConclusion> second = caseItem.PossibleConclusions.Select((BpRef<BlueprintConclusion> r) => r.Blueprint).ToList();
		return (from s in Detective.GetAvailableConclusions(caseItem.ParentCase).Intersect<BlueprintConclusion>(second)
			where Enumerable.Any(s.Sources, (BlueprintConclusion.Source s2) => s2.Item1 == caseItem)
			select s).ToList();
	}

	public static string GetRefutedText(BlueprintConclusion conclusion)
	{
		StringBuilder stringBuilder = new StringBuilder();
		BpRef<BlueprintCaseItem>[] refutations = conclusion.Refutations;
		foreach (BpRef<BlueprintCaseItem> bpRef in refutations)
		{
			stringBuilder.AppendLine(bpRef.Blueprint.Description);
		}
		return stringBuilder.ToString();
	}

	public static List<BlueprintClueAddendum> GetNewAddendums(BlueprintClue blueprintClue)
	{
		ExaminedDetectiveData.ExaminedData<BlueprintClueAddendum> examinedAddendums = ExaminedDetectiveData.ExaminedAddendums;
		IEnumerable<BlueprintClueAddendum> openedAddendumsFor = Detective.GetOpenedAddendumsFor(blueprintClue);
		IEnumerable<BlueprintClueAddendum> second = from o in openedAddendumsFor.Where((BlueprintClueAddendum a) => examinedAddendums.IsEntityNew(a)).SelectMany((BlueprintClueAddendum a) => a.Overrides)
			select o.Addendum.Blueprint;
		return openedAddendumsFor.Except<BlueprintClueAddendum>(second).Where(examinedAddendums.IsEntityNew).ToList();
	}

	public static bool CanOpenReport(BlueprintCase blueprintCase)
	{
		if (blueprintCase == null)
		{
			return false;
		}
		if (blueprintCase.IsClosed())
		{
			return true;
		}
		return GetAnswersWithTier(blueprintCase).Any();
	}

	public static Queue<StudyGroup> CreateStudyGroups(BlueprintClue clue)
	{
		Queue<StudyGroup> queue = new Queue<StudyGroup>();
		foreach (BpRef<BlueprintClueStudy> study in clue.Studies.Where((BpRef<BlueprintClueStudy> s) => Detective.IsAvailableForStudy(s) && s.Blueprint.GiveItems.Dereference().Any((BlueprintCaseItem a) => !Game.Instance.DetectiveSystem.HasItem(a))))
		{
			StudyGroup studyGroup = queue.FirstOrDefault((StudyGroup g) => g.StudyName == study.MaybeBlueprint?.Name.Text && g.BarkText == study.MaybeBlueprint?.StudyBark.Text);
			if (studyGroup != null)
			{
				studyGroup.AddStudy(study);
			}
			else
			{
				queue.Enqueue(new StudyGroup(study));
			}
		}
		return queue;
	}

	public static void GetNestingCount(BlueprintConclusion conclusion, out int nestingCount)
	{
		ExaminedDetectiveData.ExaminedData<ConclusionSourceWrapper> preserved = Game.Instance.Player.UISettings.DetectiveSystemData.ExaminedDetectiveData.SelectedConclusionSource;
		if (!((Enumerable.FirstOrDefault(conclusion.Sources, (BlueprintConclusion.Source s) => Enumerable.FirstOrDefault(preserved.GetEntities(), (ConclusionSourceWrapper e) => e.Is(s)) != null) ?? conclusion.Sources.First()).Item1.Blueprint is BlueprintConclusion conclusion2))
		{
			nestingCount = 0;
			return;
		}
		GetNestingCount(conclusion2, out nestingCount);
		nestingCount++;
	}

	public static LocalizedString GetAddendumDescriptionWithOverride(BlueprintClueAddendum blueprintAddendum)
	{
		LocalizedString description = blueprintAddendum.Description;
		BlueprintClueAddendum.Override @override = Detective.GetOverride(blueprintAddendum);
		if (@override == null)
		{
			return description;
		}
		if (@override.Type == BlueprintClueAddendum.OverrideType.Addition)
		{
			description = @override.Addendum.Blueprint.Description;
		}
		else
		{
			PFLog.UI.Error("Unsupported override type");
		}
		return description;
	}

	public static BlueprintConclusion.Source GetSuitableConclusionSource(BlueprintConclusion conclusion)
	{
		return conclusion.Sources.OrderBy(HasItem).FirstOrDefault((BlueprintConclusion.Source s) => Detective.HasItem(s.Item2.Blueprint) && (!(s.Item2.Blueprint is BlueprintClueAddendum blueprintClueAddendum) || Detective.HasItem((BlueprintClue?)blueprintClueAddendum.ParentClue)));
		static bool HasItem(BlueprintConclusion.Source source)
		{
			return Enumerable.FirstOrDefault(ExaminedDetectiveData.SelectedConclusionSource.GetEntities(), (ConclusionSourceWrapper cs) => cs.Is(source)) != null;
		}
	}

	public static ClueUIData GetUIData(this BlueprintClue clue)
	{
		try
		{
			BlueprintClue @override = clue.GetOverride();
			ClueUIData result = default(ClueUIData);
			result.Name = @override.Name;
			result.Description = @override.Description;
			result.Icon = @override.Icon;
			result.UIType = @override.UIClueType;
			return result;
		}
		catch (Exception arg)
		{
			PFLog.UI.Error($"Failed to get UI data for clue {clue.Name}: {arg}");
			return default(ClueUIData);
		}
	}

	public static bool HasNewClues(BlueprintCase blueprintCase)
	{
		return GetNewClues(blueprintCase).Any();
	}

	public static IEnumerable<BlueprintClue> GetNewClues(BlueprintCase blueprintCase)
	{
		return from c in GetOpenedCluesFor(blueprintCase).RemoveOverrides()
			where ExaminedDetectiveData.ExaminedClues.IsEntityNew(c)
			select c;
	}

	public static IEnumerable<BlueprintConclusion> GetNewConclusions(BlueprintCase blueprintCase)
	{
		return from c in GetOpenedCaseItemsFor<BlueprintConclusion>(blueprintCase).RemoveOverrides()
			where ExaminedDetectiveData.ExaminedConclusions.IsEntityNew(c)
			select c;
	}

	public static bool HasNewAddendums(BlueprintCase blueprintCase)
	{
		return GetCluesWithNewAddendums(blueprintCase).Any();
	}

	public static IEnumerable<BlueprintClue> GetCluesWithNewAddendums(BlueprintCase blueprintCase)
	{
		return (from a in GetOpenedCluesFor(blueprintCase).SelectMany((BlueprintClue c) => c.Addendums)
			where Game.Instance.DetectiveSystem.HasClueAddendum(a)
			select a into c
			where ExaminedDetectiveData.ExaminedAddendums.IsEntityNew(c)
			select c into a
			select a.Blueprint.ParentClue.Blueprint).Distinct();
	}

	public static bool HasNewConclusions(BlueprintCase blueprintCase)
	{
		if (!GetNewConclusions(blueprintCase).Any())
		{
			return GetConclusionsWithNewConclusions(blueprintCase).Any();
		}
		return true;
	}

	public static IEnumerable<BlueprintClue> GetCluesWithNewConclusions(BlueprintCase blueprintCase)
	{
		return from i in GetOpenedCluesFor(blueprintCase).RemoveOverrides()
			where HasNewConclusion(i) || Game.Instance.DetectiveSystem.GetOpenedAddendumsFor(i).Cast<BlueprintCaseItem>().Any(HasNewConclusion)
			select i;
		static bool HasNewConclusion(BlueprintCaseItem item)
		{
			return GetConclusionsFor(item).Any((BlueprintConclusion c) => ExaminedDetectiveData.ExaminedConclusions.IsEntityNew(c));
		}
	}

	public static IEnumerable<BlueprintConclusion> GetConclusionsWithNewConclusions(BlueprintCase blueprintCase)
	{
		return GetOpenedCaseItemsFor<BlueprintConclusion>(blueprintCase).RemoveOverrides().Where(HasNewConclusion);
		static bool HasNewConclusion(BlueprintCaseItem item)
		{
			return GetConclusionsFor(item).Any((BlueprintConclusion c) => ExaminedDetectiveData.ExaminedConclusions.IsEntityNew(c));
		}
	}

	public static bool HasNewStudies(BlueprintCase blueprintCase)
	{
		return GetCluesWithNewStudies(blueprintCase).Any();
	}

	public static bool HasFakeStudies(BlueprintClueStudy study)
	{
		return study.GiveItems.Dereference().Any((BlueprintCaseItem a) => !(a is BlueprintClueAddendum blueprintClueAddendum) || blueprintClueAddendum.ParentClue != study.ParentClue);
	}

	public static BlueprintClue GetStudyLink(BlueprintClueStudy study, out FakeStudyType studyType)
	{
		BlueprintCaseItem blueprintCaseItem = study.GiveItems.Dereference().FirstOrDefault((BlueprintCaseItem a) => !(a is BlueprintClueAddendum blueprintClueAddendum2) || blueprintClueAddendum2.ParentClue != study.ParentClue);
		if (!(blueprintCaseItem is BlueprintClue result))
		{
			if (blueprintCaseItem is BlueprintClueAddendum blueprintClueAddendum)
			{
				studyType = FakeStudyType.Addendum;
				return blueprintClueAddendum.ParentClue;
			}
			studyType = FakeStudyType.Unknown;
			return null;
		}
		studyType = FakeStudyType.Clue;
		return result;
	}

	public static IEnumerable<BlueprintClue> GetCluesWithNewStudies(BlueprintCase blueprintCase)
	{
		if (blueprintCase == null)
		{
			return Enumerable.Empty<BlueprintClue>();
		}
		List<BlueprintClue> list = (from c in GetOpenedCluesFor(blueprintCase).RemoveOverrides()
			where Detective.IsAvailableForStudy(c)
			select c).ToList();
		list.RemoveAll((BlueprintClue c) => !CanBeStudied(c));
		return list;
		static bool CanBeStudied(BlueprintClue clue)
		{
			return clue.Studies.Dereference().Any((BlueprintClueStudy s) => Game.Instance.DetectiveSystem.IsAvailableForStudy(s) && !s.GiveItems.Dereference().All((BlueprintCaseItem i) => Game.Instance.DetectiveSystem.HasItem(i)));
		}
	}

	public static bool HasAnswerWithTier(BlueprintCaseItem blueprintCaseItem)
	{
		int degree;
		return (from a in Game.Instance.DetectiveSystem.GetActualCaseQuestion(blueprintCaseItem.ParentCase.Blueprint).AllAnswers.Dereference()
			where a.RelatedItem == blueprintCaseItem
			select (!Game.Instance.DetectiveSystem.TryGetAnswerDegree(a, out degree)) ? (-1) : degree).Any((int tier) => tier >= 0);
	}

	public static IEnumerable<BlueprintCaseAnswer> GetAnswersFor(BlueprintCaseItem blueprintCaseItem)
	{
		if (blueprintCaseItem.ParentCase.Blueprint.IsUnknown())
		{
			return Enumerable.Empty<BlueprintCaseAnswer>();
		}
		return from a in Game.Instance.DetectiveSystem.GetActualCaseQuestion(blueprintCaseItem.ParentCase.Blueprint).AllAnswers.Dereference()
			where a.RelatedItem.Blueprint == blueprintCaseItem
			select a;
	}

	public static IEnumerable<BlueprintCaseAnswer> GetAnswersWithTier(BlueprintCase blueprintCase)
	{
		if (blueprintCase == null)
		{
			return Enumerable.Empty<BlueprintCaseAnswer>();
		}
		int degree;
		return from a in Game.Instance.DetectiveSystem.GetActualCaseQuestion(blueprintCase).AllAnswers.Dereference()
			where Game.Instance.DetectiveSystem.TryGetAnswerDegree(a, out degree) && degree >= 0
			select a;
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
