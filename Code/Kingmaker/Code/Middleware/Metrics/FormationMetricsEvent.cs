namespace Kingmaker.Code.Middleware.Metrics;

public class FormationMetricsEvent : MetricsEvent
{
	protected override string Name => "formation";

	public FormationMetricsEvent(bool isGameEvent)
		: base(isGameEvent)
	{
	}

	public FormationMetricsEvent Id(string id)
	{
		AddParam("id", id);
		return this;
	}
}
