using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Fmw.Blueprints;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class DetectiveJournalClueVM : ViewModel, IClueDataChangedHandler, ISubscriber, IConclusionsUpdateHandler, IConclusionStatusChanged
{
	public readonly BlueprintClue Clue;

	public readonly List<ConstructConclusionVM> ConclusionsVM = new List<ConstructConclusionVM>();

	public readonly HashSet<BlueprintClue> ConnectedClues = new HashSet<BlueprintClue>();

	private readonly ReactiveProperty<ClueState> m_CurrentState = new ReactiveProperty<ClueState>();

	private readonly ReactiveProperty<bool> m_IsAnswerClue = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_HasNotes = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<int> m_NewAddendumsCount = new ReactiveProperty<int>();

	private readonly ReactiveProperty<BlueprintCaseItem> m_CaseItemWithNewDeduction = new ReactiveProperty<BlueprintCaseItem>();

	private readonly ReactiveProperty<BlueprintCaseItem> m_CaseItemWithDeductionVariant = new ReactiveProperty<BlueprintCaseItem>();

	private readonly ReactiveCommand<Unit> m_ConclusionsUpdated = new ReactiveCommand<Unit>();

	private readonly Action m_ClickOnClue;

	private readonly Action<BlueprintCaseItem> m_ClickOnDeduction;

	private readonly ReactiveProperty<DeductionState> m_CurrentDeductionState = new ReactiveProperty<DeductionState>();

	public ReadOnlyReactiveProperty<bool> IsAnswerClue => m_IsAnswerClue;

	public ReadOnlyReactiveProperty<bool> HasNotes => m_HasNotes;

	public ReadOnlyReactiveProperty<int> NewAddendumsCount => m_NewAddendumsCount;

	public ReadOnlyReactiveProperty<ClueState> CurrentState => m_CurrentState;

	public Observable<Unit> ConclusionsUpdated => m_ConclusionsUpdated;

	public ReadOnlyReactiveProperty<DeductionState> CurrentDeductionState => m_CurrentDeductionState;

	public ClueUIData UIData => Clue.GetUIData();

	public DetectiveJournalClueVM(BlueprintClue clue, Action<BlueprintClue> clickOnClue, Action<BlueprintCaseItem> clickOnDeduction)
	{
		Clue = clue;
		m_ClickOnClue = delegate
		{
			clickOnClue(clue);
		};
		m_ClickOnDeduction = clickOnDeduction;
		if (UIConfig.Instance.DetectiveConfig.ShowSolidLines)
		{
			ConnectedClues = (from l in clue.LinkedClues
				where l.IsUnlocked()
				select l.Clue.Blueprint into c
				where Game.Instance.DetectiveSystem.HasItemExcludingHidden(c)
				select c).ToHashSet();
		}
		UpdateConstructConclusions();
		RefreshDataFor(clue);
		UpdateClueData();
		EventBus.Subscribe(this).AddTo(this);
	}

	public void UpdateClueData()
	{
		UpdateDeductionState();
		UpdateClueState();
		UpdateAnswerState();
		UpdateNotesState();
	}

	public void ClickOnClue()
	{
		m_ClickOnClue?.Invoke();
		UpdateClueState();
	}

	public void UpdateClueState()
	{
		m_NewAddendumsCount.Value = UIUtilityDetective.GetNewAddendums(Clue).Count;
		if (UIUtilityDetective.ExaminedDetectiveData.ExaminedClues.IsEntityNew(Clue))
		{
			m_CurrentState.Value = ClueState.New;
			return;
		}
		Queue<StudyGroup> queue = UIUtilityDetective.CreateStudyGroups(Clue);
		if (queue != null && queue.Count > 0 && !Clue.ParentCase.Blueprint.IsUnknown())
		{
			m_CurrentState.Value = ClueState.NewStudies;
		}
		else if (UIUtilityDetective.GetNewAddendums(Clue).Any() && !Clue.ParentCase.Blueprint.IsUnknown())
		{
			m_CurrentState.Value = ClueState.NewAddendums;
		}
		else
		{
			m_CurrentState.Value = ClueState.Default;
		}
	}

	private void UpdateAnswerState()
	{
		m_IsAnswerClue.Value = UIUtilityDetective.HasAnswerWithTier(Clue);
	}

	private void UpdateDeductionState()
	{
		BlueprintCaseItem blueprintCaseItem = null;
		m_CurrentDeductionState.Value = UIUtilityDetective.GetDeductionState(Clue);
		if (CurrentDeductionState.CurrentValue == DeductionState.NewDeduction)
		{
			m_CaseItemWithNewDeduction.Value = Clue;
			return;
		}
		if (CurrentDeductionState.CurrentValue == DeductionState.NewDeductionVariant)
		{
			blueprintCaseItem = Clue;
		}
		foreach (BlueprintClueAddendum item in Game.Instance.DetectiveSystem.GetOpenedAddendumsFor(Clue))
		{
			switch (UIUtilityDetective.GetDeductionState(item))
			{
			case DeductionState.NewDeduction:
				m_CaseItemWithNewDeduction.Value = item;
				return;
			case DeductionState.NewDeductionVariant:
				if (blueprintCaseItem != null)
				{
					blueprintCaseItem = item;
				}
				break;
			}
		}
		m_CaseItemWithNewDeduction.Value = null;
		m_CaseItemWithDeductionVariant.Value = blueprintCaseItem;
	}

	private void UpdateConstructConclusions()
	{
		ConclusionsVM.Clear();
		Dictionary<BlueprintCaseItem, List<BlueprintConclusion>> dictionary = new Dictionary<BlueprintCaseItem, List<BlueprintConclusion>>();
		List<BlueprintConclusion> conclusionsFor = UIUtilityDetective.GetConclusionsFor(Clue);
		IEnumerable<BlueprintConclusion> collection = Clue.Addendums.Where((BpRef<BlueprintClueAddendum> addendum) => Game.Instance.DetectiveSystem.HasItemExcludingHidden(addendum.Blueprint)).SelectMany((BpRef<BlueprintClueAddendum> addendum) => UIUtilityDetective.GetConclusionsFor(addendum.Blueprint));
		conclusionsFor.AddRange(collection);
		foreach (BlueprintConclusion item in conclusionsFor)
		{
			BlueprintConclusion.Source suitableConclusionSource = UIUtilityDetective.GetSuitableConclusionSource(item);
			if (Game.Instance.DetectiveSystem.HasItemExcludingHidden(suitableConclusionSource.Item2) && (!(suitableConclusionSource.Item2.Blueprint is BlueprintClueAddendum blueprintClueAddendum) || Game.Instance.DetectiveSystem.HasItemExcludingHidden((BlueprintClue?)blueprintClueAddendum.ParentClue)))
			{
				if (!dictionary.ContainsKey(suitableConclusionSource.Item1))
				{
					dictionary[suitableConclusionSource.Item1] = new List<BlueprintConclusion>();
				}
				dictionary[suitableConclusionSource.Item1].Add(item);
			}
		}
		IEnumerable<BlueprintCaseItem> source = from kvp in dictionary
			where !kvp.Value.Any((BlueprintConclusion c) => Game.Instance.DetectiveSystem.HasItemExcludingHidden(c))
			select kvp.Key;
		ConclusionsVM.AddRange(source.Select((BlueprintCaseItem c) => new ConstructConclusionVM(c, delegate
		{
			m_ClickOnDeduction?.Invoke(c);
		}, null)));
		m_ConclusionsUpdated?.Execute(Unit.Default);
	}

	public void RefreshDataFor(BlueprintClue clue)
	{
		if (Clue == clue)
		{
			UpdateClueData();
			UpdateConstructConclusions();
		}
	}

	public void UpdateConclusions()
	{
		UpdateConstructConclusions();
	}

	private void UpdateNotesState()
	{
		m_HasNotes.Value = Clue.Notes.Any((BpRef<BlueprintClueNote> n) => n.Blueprint.IsVisible);
	}

	public void HandleConclusionStatusChanged(BlueprintConclusion blueprint)
	{
		UpdateConstructConclusions();
	}
}
