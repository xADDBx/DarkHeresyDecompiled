using System;
using System.Collections.Generic;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Localization;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UIDetectiveJournal
{
	[Header("MainPage")]
	public LocalizedString CurrentCases;

	public LocalizedString UnknownCluesHeader;

	public LocalizedString ToggleClosedCases;

	public LocalizedString ClickToViewCTA;

	public LocalizedString NoCasesLabel;

	public LocalizedString UnknownCluesTitle;

	public LocalizedString UnknownCluesDescription;

	public LocalizedString NoCasesCallToAction;

	public LocalizedString ClosedVerdict;

	[Header("SingleCase")]
	public LocalizedString PrepareReportLabel;

	public LocalizedString WatchReportLabel;

	public LocalizedString MakeDeductionText;

	public LocalizedString NoConclusionSelected;

	public LocalizedString AnswersListTitle;

	public LocalizedString AnswerChangedTitle;

	[Header("ClueInfo")]
	public LocalizedString NewAddendumLabel;

	public LocalizedString RefutedLabel;

	public LocalizedString RefutedDescription;

	public LocalizedString CaseDataLabel;

	public LocalizedString NoAddendumsLabel;

	public LocalizedString NoAddendumsCallToAction;

	public LocalizedString NewAddendumsReceived;

	public LocalizedString AddendumsAddedToOtherClue;

	public LocalizedString ClueAddedFromStudy;

	public LocalizedString SelectedHypothesisTitle;

	public LocalizedString PotentialHypothesisTitle;

	[Header("Studies")]
	public LocalizedString ToStudiesLabel;

	public LocalizedString MultipleStudiesLabel;

	public LocalizedString NextStudy;

	public LocalizedString FinishStudies;

	[Header("Conclusions")]
	public LocalizedString ChooseConclusionLabel;

	public LocalizedString RemoveConclusionLabel;

	[Header("Report")]
	public LocalizedString ReportTitleCTA;

	public LocalizedString ReportTitle;

	public LocalizedString SendReportLabel;

	public LocalizedString ConfirmSendReportLabel;

	public LocalizedString SelectConclusionCTA;

	public LocalizedString CaseLabel;

	public LocalizedString HypothesisLabel;

	public LocalizedString NoStrongEvidence;

	public LocalizedString HasSomeEvidence;

	public LocalizedString AccusationLabel;

	public LocalizedString BasedOnConclusionLabel;

	[Header("Hints")]
	public LocalizedString CannotPrepareReport;

	public LocalizedString SendReportHint;

	public LocalizedString CompleteReportHint;

	public LocalizedString CannotSendReportHint;

	public LocalizedString ToCaseCommonHint;

	public LocalizedString CannotCloseCaseHint;

	public LocalizedString HasNewCluesHint;

	public LocalizedString HasNewConclusionsHint;

	public LocalizedString HasNewStudiesHint;

	[Header("Annotations")]
	public LocalizedString AnnotationsTitle;

	public LocalizedString NewEntityAnnotationDescription;

	public LocalizedString ConfirmedConclusionAnnotationDescription;

	public LocalizedString RefutedConclusionAnnotationDescription;

	[Header("ExplorationBarks")]
	public List<LocalizedString> CanExploreBarks;

	[Header("Sources")]
	public LocalizedString SourceHeader;

	public LocalizedString SourceUnknown;

	public LocalizedString SourceCue;

	public LocalizedString SourceItem;

	public LocalizedString SourceStudy;

	public LocalizedString SourceReconstruction;

	public LocalizedString ReceivedFromCue;

	public LocalizedString ReceivedFromStudy;

	public LocalizedString ReceivedFromCompanion;

	public LocalizedString ReceivedFromReconstruction;

	public LocalizedString GetSourceLabel(CaseItemIssueType issueType)
	{
		return issueType switch
		{
			CaseItemIssueType.Default => SourceUnknown, 
			CaseItemIssueType.Dialog => SourceCue, 
			CaseItemIssueType.Item => SourceItem, 
			CaseItemIssueType.Companion => SourceStudy, 
			CaseItemIssueType.Study => SourceStudy, 
			CaseItemIssueType.Reconstruction => SourceReconstruction, 
			_ => throw new ArgumentOutOfRangeException("issueType", issueType, null), 
		};
	}

	public string GetSourceTitle(CaseItemIssueType issueType)
	{
		return issueType switch
		{
			CaseItemIssueType.Default => null, 
			CaseItemIssueType.Dialog => ReceivedFromCue.Text, 
			CaseItemIssueType.Item => null, 
			CaseItemIssueType.Companion => ReceivedFromCompanion.Text, 
			CaseItemIssueType.Study => ReceivedFromStudy.Text, 
			CaseItemIssueType.Reconstruction => ReceivedFromReconstruction.Text, 
			_ => throw new ArgumentOutOfRangeException("issueType", issueType, null), 
		};
	}
}
