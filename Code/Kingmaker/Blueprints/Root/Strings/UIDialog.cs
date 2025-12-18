using System;
using Kingmaker.Localization;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UIDialog
{
	public LocalizedString SucccedeedCheckFormat;

	public LocalizedString FailedCheckFormat;

	public LocalizedString SoulMarkShiftFormat;

	public LocalizedString HasRelatedItems;

	public LocalizedString Succeeded;

	public LocalizedString Failed;

	public LocalizedString AligmentShiftedFormat;

	public LocalizedString AlignmentRequirementLabel;

	public LocalizedString AnswerDialogueFormat;

	public LocalizedString AnswerYouNeedItem;

	public LocalizedString OpenGlossary;

	public LocalizedString CloseGlossary;

	public LocalizedString OperationOrConditionDesc;

	public LocalizedString OperationAndConditionDesc;

	public LocalizedString ShowVotes;

	public LocalizedString HideVotes;

	public LocalizedString CaseClosedCondition;

	public LocalizedString NewItemLabel;

	public LocalizedString NewItemReceived;

	public LocalizedString NewClueReceived;

	public LocalizedString NewAddendumReceived;

	public LocalizedString NewConclusionConstructed;

	public static UIDialog Instance => UIStrings.Instance.Dialog;
}
