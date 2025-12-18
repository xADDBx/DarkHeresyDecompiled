using System.Linq;
using Kingmaker.Framework.DetectiveSystem;

namespace Kingmaker.Code.UI.MVVM;

public static class DetectiveHelper
{
	private static DetectiveSystem Detective => Game.Instance.DetectiveSystem;

	public static bool IsUnknown(this BlueprintCase blueprintCase)
	{
		if (blueprintCase != null)
		{
			return Detective.GetCaseStatus(blueprintCase) == CaseStatus.None;
		}
		return true;
	}

	public static bool IsOpen(this BlueprintCase blueprintCase)
	{
		if (blueprintCase != null)
		{
			return Detective.GetCaseStatus(blueprintCase) == CaseStatus.Opened;
		}
		return false;
	}

	public static bool IsClosed(this BlueprintCase blueprintCase)
	{
		if (blueprintCase != null)
		{
			return Detective.GetCaseStatus(blueprintCase) == CaseStatus.Closed;
		}
		return false;
	}

	public static bool IsFailed(this BlueprintCase blueprintCase)
	{
		if (blueprintCase != null && Detective.GetCaseStatus(blueprintCase) == CaseStatus.Closed)
		{
			return Detective.GetCaseAnswer(blueprintCase)?.Answer == null;
		}
		return false;
	}

	public static bool HasUnknownClues()
	{
		return Detective.GetUnknownClues().Any();
	}

	public static bool HasClosedCases()
	{
		return Detective.GetCasesWithStatus(CaseStatus.Closed).Any();
	}
}
