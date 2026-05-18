using System.Globalization;

namespace Kingmaker.Code.Middleware.Metrics;

public class CutsceneMetricsEvent : MetricsEvent
{
	protected override string Name => "cutscene";

	public CutsceneMetricsEvent Id(string id)
	{
		AddParam("id", id);
		return this;
	}

	public CutsceneMetricsEvent Skipped(bool skipped)
	{
		AddParam("skipped", skipped.ToString());
		return this;
	}

	public CutsceneMetricsEvent SkipTime(float skip_time)
	{
		AddParam("skip_time", skip_time.ToString(CultureInfo.InvariantCulture));
		return this;
	}
}
