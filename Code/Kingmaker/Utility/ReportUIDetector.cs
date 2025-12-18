namespace Kingmaker.Utility;

internal static class ReportUIDetector
{
	public static bool TryGetUiFeatureName(out string featureName)
	{
		featureName = Game.Instance.BugReportContext.GetInterfaceName();
		return !string.IsNullOrWhiteSpace(featureName);
	}
}
