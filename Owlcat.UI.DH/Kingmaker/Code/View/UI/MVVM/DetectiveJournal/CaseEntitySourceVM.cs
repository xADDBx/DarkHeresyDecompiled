using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class CaseEntitySourceVM : ViewModel
{
	public class SourceData
	{
		public CaseItemIssueType IssueType;

		public string SourceLabel;

		public TooltipBaseTemplate Tooltip;
	}

	public readonly BlueprintClue ParentClue;

	public readonly BlueprintArea IssueArea;

	public readonly SourceData Source;

	public CaseEntitySourceVM(BlueprintClue thisClue)
		: this(thisClue, Game.Instance.DetectiveSystem.GetSource(thisClue), Game.Instance.DetectiveSystem.GetIssuePlace(thisClue))
	{
	}

	public CaseEntitySourceVM(BlueprintClueAddendum thisAddendum)
		: this(thisAddendum.ParentClue, Game.Instance.DetectiveSystem.GetSource(thisAddendum), Game.Instance.DetectiveSystem.GetIssuePlace(thisAddendum))
	{
	}

	public CaseEntitySourceVM(BlueprintClueStudy study)
		: this(study.ParentClue, study, null)
	{
	}

	private CaseEntitySourceVM(BlueprintClue parentClue, BlueprintScriptableObject issueSource, BlueprintArea issueArea)
	{
		ParentClue = parentClue;
		IssueArea = issueArea;
		Source = GetSourceData(issueSource);
	}

	private SourceData GetSourceData(BlueprintScriptableObject source)
	{
		SourceData sourceData = new SourceData();
		UIDetectiveJournal detectiveJournal = UIStrings.Instance.DetectiveJournal;
		using (GameLogContext.Scope)
		{
			GameLogContext.CaseItemArea = IssueArea;
			if (!(source is BlueprintCue blueprintCue))
			{
				if (!(source is BlueprintAnswer blueprintAnswer))
				{
					if (!(source is BlueprintUnit blueprintUnit))
					{
						if (!(source is BlueprintItem blueprintItem))
						{
							if (!(source is BlueprintClueStudy blueprintClueStudy))
							{
								if (source is BlueprintCaseItemIssueSource blueprintCaseItemIssueSource)
								{
									sourceData.IssueType = blueprintCaseItemIssueSource.IssueType;
									sourceData.SourceLabel = GetSourceLabel(blueprintCaseItemIssueSource.IssueType);
									string text = blueprintCaseItemIssueSource.Description.Text;
									if (string.IsNullOrEmpty(text))
									{
										text = detectiveJournal.GetFallbackDescription(blueprintCaseItemIssueSource.IssueType);
									}
									sourceData.Tooltip = new TooltipTemplateSimple(detectiveJournal.GetSourceTitle(blueprintCaseItemIssueSource.IssueType), text);
								}
								else
								{
									sourceData = null;
								}
							}
							else
							{
								bool flag = blueprintClueStudy.StudyCompanion?.MaybeBlueprint != null;
								sourceData.IssueType = (flag ? CaseItemIssueType.Companion : CaseItemIssueType.Study);
								sourceData.SourceLabel = ((blueprintClueStudy.ParentClue != ParentClue) ? string.Format(detectiveJournal.SourceHeader.Text, blueprintClueStudy.ParentClue.Blueprint.GetUIData().Name.Text) : GetSourceLabel(sourceData.IssueType));
								string header = (flag ? string.Format(detectiveJournal.ReceivedFromCompanion, blueprintClueStudy.StudyCompanion?.MaybeBlueprint.CharacterName) : string.Format(detectiveJournal.ReceivedFromStudy, blueprintClueStudy.Name.Text));
								sourceData.Tooltip = new TooltipTemplateSimple(header, blueprintClueStudy.StudyBark.Text);
							}
						}
						else
						{
							sourceData.IssueType = CaseItemIssueType.Item;
							sourceData.SourceLabel = GetSourceLabel(CaseItemIssueType.Item);
							sourceData.Tooltip = new TooltipTemplateItem(blueprintItem);
						}
					}
					else
					{
						GameLogContext.Text = blueprintUnit.CharacterName;
						sourceData.IssueType = CaseItemIssueType.Unit;
						sourceData.SourceLabel = string.Format(UIStrings.Instance.DetectiveJournal.SourceHeader.Text, blueprintUnit.CharacterName);
						sourceData.Tooltip = new TooltipTemplateSimple(detectiveJournal.GetSourceTitle(CaseItemIssueType.Unit), detectiveJournal.GetFallbackDescription(CaseItemIssueType.Unit));
					}
				}
				else
				{
					sourceData.IssueType = CaseItemIssueType.Dialog;
					sourceData.SourceLabel = GetSourceLabel(CaseItemIssueType.Dialog);
					sourceData.Tooltip = new TooltipTemplateSimple(detectiveJournal.ReceivedFromCue, FormatDialogText(blueprintAnswer.Text.Text));
				}
			}
			else
			{
				sourceData.IssueType = CaseItemIssueType.Dialog;
				sourceData.SourceLabel = GetSourceLabel(CaseItemIssueType.Dialog);
				sourceData.Tooltip = new TooltipTemplateSimple(detectiveJournal.ReceivedFromCue, FormatDialogText(blueprintCue.Text.Text));
			}
			return sourceData;
		}
	}

	private string GetSourceLabel(CaseItemIssueType issueType)
	{
		return string.Format(UIStrings.Instance.DetectiveJournal.SourceHeader.Text, UIStrings.Instance.DetectiveJournal.GetSourceLabel(issueType).Text);
	}

	private string FormatDialogText(string text)
	{
		return UIUtilityText.StringIDToColor(text, DialogCueColors.NarratorColorStringID, new Color32(47, 47, 47, byte.MaxValue));
	}
}
