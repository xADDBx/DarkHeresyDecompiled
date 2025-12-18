using System;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.Visual.Sound;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class DetectiveReportVM : ViewModel
{
	public readonly BlueprintCase BlueprintCase;

	public readonly CaseCardVM CaseCardVM;

	public readonly ReportContext ReportContext;

	public readonly DetectivePaperReportVM PaperReportVM;

	public readonly AnswerSelectionVM AnswerSelectionVM;

	private readonly ReactiveCommand<Unit> m_AnimateHideCommand = new ReactiveCommand<Unit>();

	public readonly ReadOnlyReactiveProperty<bool> QuestionIsAnswered;

	private readonly Action<bool> m_OnClose;

	public Observable<Unit> AnimateHideCommand => m_AnimateHideCommand;

	public DetectiveReportVM(BlueprintCase blueprintCase, Action<bool> onClose)
	{
		BlueprintCase = blueprintCase;
		m_OnClose = onClose;
		CaseCardVM = new CaseCardVM(blueprintCase);
		ReportContext = new ReportContext(blueprintCase);
		AnswerSelectionVM = new AnswerSelectionVM(ReportContext);
		QuestionIsAnswered = ReportContext.SelectedAnswer.Select((ReportAnswerVM a) => a != null).ToReadOnlyReactiveProperty(initialValue: false).AddTo(this);
		PaperReportVM = new DetectivePaperReportVM(ReportContext);
		SoundState instance = SoundState.Instance;
		BlueprintCase blueprintCase2 = BlueprintCase;
		instance.OnDetectiveJournalChange((blueprintCase2 == null || !blueprintCase2.IsClosed()) ? MusicStateHandler.DetectiveBoardMusicState.Report : (SoundState.Instance.OpenedCaseWasShowBefore ? MusicStateHandler.DetectiveBoardMusicState.Default : MusicStateHandler.DetectiveBoardMusicState.None));
	}

	public void Close(bool reportWasSent = false)
	{
		m_OnClose?.Invoke(reportWasSent);
		if (!BlueprintCase.IsClosed())
		{
			SoundState.Instance.OnDetectiveJournalChange(MusicStateHandler.DetectiveBoardMusicState.Default);
		}
	}

	public void SendReport()
	{
		Game.Instance.DetectiveSystem.CloseCase(BlueprintCase, ReportContext.SelectedAnswer.CurrentValue.Answer);
		MarkAllCaseItemsAsViewed();
		m_AnimateHideCommand.Execute();
		SoundState.Instance.OnDetectiveJournalChange(MusicStateHandler.DetectiveBoardMusicState.None);
		SoundState.Instance.OpenedCaseWasShowBefore = false;
	}

	private void MarkAllCaseItemsAsViewed()
	{
		foreach (BlueprintClue availableClue in Game.Instance.DetectiveSystem.GetAvailableClues(BlueprintCase))
		{
			UIUtilityDetective.ExaminedDetectiveData.ExaminedClues.AddExaminedEntityIfNeeded(availableClue);
			UIUtilityDetective.GetNewAddendums(availableClue).ForEach(delegate(BlueprintClueAddendum a)
			{
				UIUtilityDetective.ExaminedDetectiveData.ExaminedAddendums.AddExaminedEntityIfNeeded(a);
			});
		}
		foreach (BlueprintConclusion availableConclusion in Game.Instance.DetectiveSystem.GetAvailableConclusions(BlueprintCase))
		{
			UIUtilityDetective.ExaminedDetectiveData.ExaminedConclusions.AddExaminedEntityIfNeeded(availableConclusion);
		}
	}
}
