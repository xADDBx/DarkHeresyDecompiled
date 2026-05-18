using System;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.Localization;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class DetectiveJournalDecor
{
	[Header("Case")]
	public LocalizedString CaseNumber;

	public LocalizedString CaseName;

	[Header("Info")]
	public LocalizedString PaperTitlePerson;

	public LocalizedString HeightLabel;

	public LocalizedString WeightLabel;

	public LocalizedString HairColorLabel;

	public LocalizedString EyesColorLabel;

	public LocalizedString AgeLabel;

	public LocalizedString PaperTitleLocation;

	public LocalizedString PaperReportNameLocation;

	public LocalizedString PersonNameLabel;

	public LocalizedString LocationNameLabel;

	[Header("Report")]
	public LocalizedString ReportTitle;

	public LocalizedString ReportDepartment;

	public LocalizedString ReportAuthor;

	public LocalizedString ReportDecorSignature;

	public LocalizedString ReportNumberLabel;

	[Header("MiniEpilogues")]
	public LocalizedString OfficialReportTitle;

	public LocalizedString MainDepartmentTitle;

	public LocalizedString ToDepartmentTitle;

	public LocalizedString ArchiveName;

	[Header("Tooltips")]
	public LocalizedString ClueName;

	public LocalizedString AddendumDesc;

	public LocalizedString ConclusionDesc;

	[Header("Greek")]
	public LocalizedString GreekLetterPhy;

	public LocalizedString GreekLetterOmega;

	public LocalizedString GetPersonInfoLabel(PersonInfoType infoType)
	{
		return infoType switch
		{
			PersonInfoType.Height => HeightLabel, 
			PersonInfoType.Weight => WeightLabel, 
			PersonInfoType.Age => AgeLabel, 
			PersonInfoType.EyeColor => EyesColorLabel, 
			PersonInfoType.HairColor => HairColorLabel, 
			_ => null, 
		};
	}

	public LocalizedString GetTooltipDesc(BlueprintCaseItem caseItem)
	{
		if (!(caseItem is BlueprintClue))
		{
			if (!(caseItem is BlueprintClueAddendum))
			{
				if (caseItem is BlueprintConclusion)
				{
					return ConclusionDesc;
				}
				throw new ArgumentOutOfRangeException("caseItem");
			}
			return AddendumDesc;
		}
		return ClueName;
	}
}
