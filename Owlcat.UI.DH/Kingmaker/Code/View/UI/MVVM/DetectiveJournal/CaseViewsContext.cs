using ObservableCollections;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class CaseViewsContext
{
	public readonly DetectiveOpenedCaseBaseView OpenedCaseView;

	public readonly CaseCardScreenBaseView CaseCardScreenBaseView;

	public readonly ObservableList<DetectiveJournalClueView> Clues;

	public readonly ObservableList<DeductionOnScreenView> Conclusions;

	public readonly RectTransform LinesContainer;

	public readonly RectTransform CluesContainer;

	public CaseViewsContext(DetectiveOpenedCaseBaseView openedCaseView, CaseCardScreenBaseView caseCardScreenBaseView, ObservableList<DetectiveJournalClueView> clues, ObservableList<DeductionOnScreenView> conclusions, RectTransform linesContainer, RectTransform cluesContainer)
	{
		OpenedCaseView = openedCaseView;
		CaseCardScreenBaseView = caseCardScreenBaseView;
		Clues = clues;
		Conclusions = conclusions;
		LinesContainer = linesContainer;
		CluesContainer = cluesContainer;
	}
}
