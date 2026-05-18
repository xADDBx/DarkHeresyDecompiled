using System.Collections.Generic;

namespace Kingmaker.Code.Middleware.Metrics;

public class LocationCompanionMetricsEvent : MetricsEvent
{
	protected override string Name => "location_change_companion";

	public LocationCompanionMetricsEvent Id(string id)
	{
		AddParam("id", id);
		return this;
	}

	public LocationCompanionMetricsEvent Level(int level)
	{
		AddParam("level", level.ToString());
		return this;
	}

	public LocationCompanionMetricsEvent Equipment(IEnumerable<string> equipment)
	{
		AddParam("equipment", equipment);
		return this;
	}

	public LocationCompanionMetricsEvent Alignment(IEnumerable<string> alignment)
	{
		AddParam("alignment", alignment);
		return this;
	}
}
