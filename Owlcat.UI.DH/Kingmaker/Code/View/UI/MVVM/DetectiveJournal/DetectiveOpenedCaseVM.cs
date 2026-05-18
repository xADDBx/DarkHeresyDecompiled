using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI;
using Kingmaker.UI.Events;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Sound;
using ObservableCollections;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class DetectiveOpenedCaseVM : ViewModel, IConclusionStatusChanged, ISubscriber
{
	public readonly BlueprintCase BlueprintCase;

	public readonly CaseCardVM CaseCardVM;

	public readonly AnswerTierChangeVM AnswerTierChangeVM;

	public readonly OpenedCaseScreenVM OpenedCaseScreenVM;

	public readonly AnswersListVM AnswersListVM;

	public readonly ObservableList<DetectiveJournalClueVM> Clues = new ObservableList<DetectiveJournalClueVM>();

	public readonly ObservableList<DeductionOnScreenVM> Conclusions = new ObservableList<DeductionOnScreenVM>();

	private readonly ReactiveProperty<bool> m_CanOpenReport = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<DetectiveReportVM> m_ReportVM = new ReactiveProperty<DetectiveReportVM>();

	private readonly ReactiveProperty<ClueFullInfoVM> m_ClueInfoVM = new ReactiveProperty<ClueFullInfoVM>();

	private readonly ReactiveProperty<ConclusionSelectionWindowVM> m_NewDeductionSelectionVM = new ReactiveProperty<ConclusionSelectionWindowVM>();

	private readonly Action m_Close;

	public ReadOnlyReactiveProperty<bool> CanOpenReport => m_CanOpenReport;

	public ReadOnlyReactiveProperty<DetectiveReportVM> ReportVM => m_ReportVM;

	public ReadOnlyReactiveProperty<ClueFullInfoVM> ClueInfoVM => m_ClueInfoVM;

	public ReadOnlyReactiveProperty<ConclusionSelectionWindowVM> NewDeductionSelectionVM => m_NewDeductionSelectionVM;

	public DetectiveOpenedCaseVM(BlueprintCase blueprintCase, Action closeAction)
	{
		BlueprintCase = blueprintCase;
		m_Close = closeAction;
		CaseCardVM = new CaseCardVM(blueprintCase);
		OpenedCaseScreenVM = new OpenedCaseScreenVM(closeAction).AddTo(this);
		AnswerTierChangeVM = new AnswerTierChangeVM(blueprintCase).AddTo(this);
		AnswersListVM = new AnswersListVM(blueprintCase, ClickOnAnswer).AddTo(this);
		UpdateClues();
		UpdateDeductions();
		UpdateCanOpenReport();
		EventBus.Subscribe(this).AddTo(this);
		SoundState.Instance.OpenedCaseWasShowBefore |= BlueprintCase != null && !BlueprintCase.IsClosed();
		ObservableSubscribeExtensions.Subscribe(Observable.Timer(UIConfig.Instance.DetectiveConfig.DelayBeforeMusicStateDetectiveBoard.Seconds(), UnityTimeProvider.TimeUpdateIgnoreTimeScale), delegate
		{
			SoundState.Instance.OnMusicStateChange(MusicStateHandler.MusicState.DetectiveBoard);
		}).AddTo(this);
		SoundState instance = SoundState.Instance;
		BlueprintCase blueprintCase2 = BlueprintCase;
		instance.OnDetectiveJournalChange((blueprintCase2 == null || !blueprintCase2.IsClosed() || SoundState.Instance.OpenedCaseWasShowBefore) ? MusicStateHandler.DetectiveBoardMusicState.Default : MusicStateHandler.DetectiveBoardMusicState.None);
		if (blueprintCase != null)
		{
			UIUtilityDetective.ExaminedDetectiveData.ExaminedCases.AddExaminedEntityIfNeeded(blueprintCase);
		}
	}

	protected override void OnDispose()
	{
		Conclusions.ForEach(delegate(DeductionOnScreenVM d)
		{
			d.Dispose();
		});
		Conclusions.Clear();
		Clues.ForEach(delegate(DetectiveJournalClueVM c)
		{
			c.Dispose();
		});
		Clues.Clear();
	}

	private void UpdateCanOpenReport()
	{
		m_CanOpenReport.Value = UIUtilityDetective.CanOpenReport(BlueprintCase);
	}

	public void PrepareReport()
	{
		m_ReportVM.Value = new DetectiveReportVM(BlueprintCase, DisposeReport).AddTo(this);
		AnswerTierChangeVM.ResetAnswers();
		AnswersListVM.MarkAsViewed();
		void DisposeReport(bool reportWasSent)
		{
			ReportVM.CurrentValue?.Dispose();
			m_ReportVM.Value = null;
			if (reportWasSent)
			{
				(m_Close ?? ((Action)delegate
				{
					EventBus.RaiseEvent(delegate(IServiceWindowUIHandler h)
					{
						h.HandleCloseAll();
					});
				}))?.Invoke();
			}
		}
	}

	public void ClickOnAnswer(BlueprintCaseAnswer answer)
	{
		BlueprintCaseItem blueprintCaseItem = answer.RelatedItem?.Blueprint;
		if (!(blueprintCaseItem is BlueprintClue clue))
		{
			BlueprintConclusion blueprintConclusion = blueprintCaseItem as BlueprintConclusion;
			if (blueprintConclusion != null)
			{
				BlueprintConclusion conclusionTo = Conclusions.FirstOrDefault((DeductionOnScreenVM c) => c.Conclusion == blueprintConclusion)?.Conclusion;
				EventBus.RaiseEvent(delegate(IMoveToCaseItemHandler h)
				{
					h.HandleMoveToCaseItem(conclusionTo);
				});
			}
		}
		else
		{
			ClickOnClue(clue);
		}
	}

	private void ClickOnClue(BlueprintClue clue)
	{
		m_ClueInfoVM.Value = new ClueFullInfoVM(clue, ClickOnClue, CloseInfo).AddTo(this);
		UIUtilityDetective.ExaminedDetectiveData.ExaminedClues.AddExaminedEntityIfNeeded(clue);
		AnswerTierChangeVM.ResetAnswers();
		void CloseInfo()
		{
			ClueInfoVM.CurrentValue?.Dispose();
			m_ClueInfoVM.Value = null;
			UpdateCaseData();
		}
	}

	private void ClickOnConclusion(BlueprintCaseItem caseItem)
	{
		m_NewDeductionSelectionVM.Value = new ConclusionSelectionWindowVM(caseItem, CloseDeductions);
		AnswerTierChangeVM.ResetAnswers();
		void CloseDeductions(bool _)
		{
			UpdateCaseData();
			NewDeductionSelectionVM.CurrentValue?.Dispose();
			m_NewDeductionSelectionVM.Value = null;
		}
	}

	private void UpdateCaseData()
	{
		UpdateClues();
		UpdateDeductions();
		AnswerTierChangeVM.UpdateAnswers();
	}

	public void UpdateClues()
	{
		foreach (BlueprintClue blueprintClue in (BlueprintCase == null) ? UIUtilityDetective.GetUnknownClues() : UIUtilityDetective.GetOpenedCluesFor(BlueprintCase).RemoveOverrides())
		{
			if (!Clues.Contains((DetectiveJournalClueVM c) => c.Clue == blueprintClue))
			{
				Clues.Add(new DetectiveJournalClueVM(blueprintClue, ClickOnClue, ClickOnConclusion));
			}
		}
	}

	private void UpdateDeductions()
	{
		if (BlueprintCase == null)
		{
			return;
		}
		DetectiveSystem detectiveSystem = Game.Instance.DetectiveSystem;
		List<BlueprintConclusion> currentConclusions = detectiveSystem.GetAvailableConclusions(BlueprintCase).Where(detectiveSystem.HasConclusionExcludingHidden).ToList();
		IEnumerable<DeductionOnScreenVM> enumerable = Conclusions.Where((DeductionOnScreenVM c) => !currentConclusions.Contains(c.Conclusion));
		Conclusions.RemoveAll(enumerable.Contains<DeductionOnScreenVM>);
		enumerable.ForEach(delegate(DeductionOnScreenVM d)
		{
			d.Dispose();
		});
		foreach (BlueprintConclusion conclusion in currentConclusions)
		{
			if (Conclusions.Any((DeductionOnScreenVM c) => c.Conclusion == conclusion))
			{
				continue;
			}
			List<ConclusionSourceWrapper> examinedEntities = UIUtilityDetective.ExaminedDetectiveData.SelectedConclusionSource.GetEntities();
			BlueprintConclusion.Source source = conclusion.Sources.FirstOrDefault((BlueprintConclusion.Source s) => examinedEntities.FirstOrDefault((ConclusionSourceWrapper cs) => cs.Is(s)) != null);
			if (source == null)
			{
				source = conclusion.Sources.FirstOrDefault(UIUtilityDetective.HasConclusionSource) ?? conclusion.Sources.First();
			}
			if (!UIUtilityDetective.HasConclusionSource(source))
			{
				detectiveSystem.RemoveConclusion(conclusion);
				PFLog.UI.Error("Has conclusion " + conclusion.AssetName + " without source items");
			}
			else
			{
				Conclusions.Add(new DeductionOnScreenVM(conclusion, source, ClickOnConclusion));
			}
		}
		Clues.ForEach(delegate(DetectiveJournalClueVM c)
		{
			c.UpdateClueData();
		});
		UpdateCanOpenReport();
	}

	public void HandleConclusionStatusChanged(BlueprintConclusion blueprint)
	{
		UpdateDeductions();
	}
}
