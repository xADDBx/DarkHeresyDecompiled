namespace Kingmaker.Code.Middleware.Metrics;

public class LocationMetricsEvent : MetricsEvent
{
	protected override string Name => "location_change";

	public LocationMetricsEvent(bool isGameEvent)
		: base(isGameEvent)
	{
	}

	public LocationMetricsEvent From(string from_id)
	{
		AddParam("from_id", from_id);
		return this;
	}

	public LocationMetricsEvent To(string to_id)
	{
		AddParam("to_id", to_id);
		return this;
	}
}
