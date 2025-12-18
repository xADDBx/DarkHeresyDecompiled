using System;
using Kingmaker.Utility;
using Owlcat.UI;

namespace Kingmaker.Code.View.Bridge.Services;

public static class BugReportService
{
	public static Func<TooltipBaseTemplate, BugContext> GetAbilityContext;

	public static Func<bool> HasBark;

	public static Func<string> GetOtherUIFeatureName;
}
