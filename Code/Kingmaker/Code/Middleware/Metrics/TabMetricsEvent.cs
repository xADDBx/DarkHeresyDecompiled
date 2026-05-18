using System.Globalization;
using Kingmaker.Settings;

namespace Kingmaker.Code.Middleware.Metrics;

public class TabMetricsEvent : MetricsEvent
{
	protected override string Name => "tab";

	public TabMetricsEvent Mode(HighlightObjectsMode mode)
	{
		AddParam("mode", mode switch
		{
			HighlightObjectsMode.Hold => "hold", 
			HighlightObjectsMode.Toggle => "toggle", 
			_ => MetricsUtils.EnumToSnakeCase(mode), 
		});
		return this;
	}

	public TabMetricsEvent ToggleState(bool toggleState)
	{
		AddParam("toggleState", toggleState.ToString());
		return this;
	}

	public TabMetricsEvent Duration(double duration)
	{
		AddParam("duration", duration.ToString("F3", CultureInfo.InvariantCulture));
		return this;
	}
}
