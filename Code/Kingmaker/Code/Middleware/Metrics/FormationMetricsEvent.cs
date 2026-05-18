namespace Kingmaker.Code.Middleware.Metrics;

public class FormationMetricsEvent : MetricsEvent
{
	protected override string Name => "formation";

	public FormationMetricsEvent From(string from_id)
	{
		AddParam("from_id", from_id);
		return this;
	}

	public FormationMetricsEvent To(string to_id)
	{
		AddParam("to_id", to_id);
		return this;
	}
}
