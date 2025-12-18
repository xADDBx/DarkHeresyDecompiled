using System;
using System.Linq;
using System.Text;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Framework.DetectiveSystem;

namespace Kingmaker.Code.View.Bridge.Utils;

public static class UtilityDetectiveDecor
{
	public static int[] GetCaseUniqueIds(BlueprintCase blueprintCase)
	{
		return GetThreeDigits(blueprintCase.AssetGuid);
	}

	public static string GetGreekLetter(BlueprintCase blueprintCase)
	{
		DetectiveJournalDecor detectiveDecor = UIStrings.Instance.DetectiveDecor;
		return GetRandomMax(blueprintCase.AssetGuid, 2) switch
		{
			0 => detectiveDecor.GreekLetterOmega.Text.ToUpper(), 
			1 => detectiveDecor.GreekLetterPhy.Text.ToUpper(), 
			_ => string.Empty, 
		};
	}

	private static int[] GetThreeDigits(string name)
	{
		int randomMax = GetRandomMax(name, 1000000);
		return new int[3]
		{
			randomMax / 10000,
			randomMax % 10000 / 100,
			randomMax % 100
		};
	}

	private static int GetRandomMax(string name, int max)
	{
		return new Random(BitConverter.ToInt32(Encoding.UTF8.GetBytes(name))).Next(max);
	}

	public static char GetCaseTier(BlueprintCase blueprintCase)
	{
		return 'a';
	}

	public static DetectiveCaseIssueType GetIssuingType(BlueprintCase blueprintCase)
	{
		return DetectiveCaseIssueType.Inquisition;
	}

	public static string GetCaseUniqueId(BlueprintCase blueprintCase)
	{
		int[] caseUniqueIds = GetCaseUniqueIds(blueprintCase);
		return $"{caseUniqueIds.ElementAt(0)}{caseUniqueIds.ElementAt(1)}//{GetGreekLetter(blueprintCase)}-{caseUniqueIds.ElementAt(2)}";
	}

	public static string GetLocationUniqueId(BlueprintClue blueprintClue)
	{
		int[] caseUniqueIds = GetCaseUniqueIds(blueprintClue.ParentCase);
		int num = GetThreeDigits(blueprintClue.AssetGuid).ElementAt(0);
		return $"LOC {caseUniqueIds.ElementAt(0)}-GH-{num}";
	}

	public static string GetDefaultUniqueId(BlueprintClue _)
	{
		return string.Empty;
	}

	public static string GetPersonUniqueId(BlueprintClue blueprintClue)
	{
		int[] caseUniqueIds = GetCaseUniqueIds(blueprintClue.ParentCase);
		int num = GetThreeDigits(blueprintClue.AssetGuid).ElementAt(0);
		return $"O/{caseUniqueIds.ElementAt(0)}//CP{num}";
	}

	public static string GetReportUniqueId(BlueprintCase blueprintCase)
	{
		int[] caseUniqueIds = GetCaseUniqueIds(blueprintCase);
		int num = (caseUniqueIds.ElementAt(0) + 72) % 8;
		return $"{num}-{caseUniqueIds.ElementAt(1)}//{caseUniqueIds.ElementAt(2)}";
	}
}
