using System.Collections.Generic;

namespace Kingmaker.Code.Middleware.Metrics;

public class LocationDataMetricsEvent : MetricsEvent
{
	protected override string Name => "location_change_data";

	public LocationDataMetricsEvent Id(string id)
	{
		AddParam("id", id);
		return this;
	}

	public LocationDataMetricsEvent Experience(int experience)
	{
		AddParam("experience", experience.ToString());
		return this;
	}

	public LocationDataMetricsEvent ExperienceLevel(int experience_level)
	{
		AddParam("experience_level", experience_level.ToString());
		return this;
	}

	public LocationDataMetricsEvent Money(long money)
	{
		AddParam("money", money.ToString());
		return this;
	}

	public LocationDataMetricsEvent Difficulty(string difficulty)
	{
		AddParam("difficulty", difficulty);
		return this;
	}

	public LocationDataMetricsEvent Formation(string formation)
	{
		AddParam("formation", formation);
		return this;
	}

	public LocationDataMetricsEvent CompanionCount(int companion_count)
	{
		AddParam("companion_count", companion_count.ToString());
		return this;
	}

	public LocationDataMetricsEvent Reputation(IEnumerable<string> reputation)
	{
		AddParam("reputation", reputation);
		return this;
	}
}
