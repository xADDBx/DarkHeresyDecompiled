namespace Kingmaker.Code.Middleware.Metrics;

public class EpilogueMetricsEvent : DialogMetricsEvent
{
	protected override string Name => "epilogue";

	public new DialogMetricsEvent Id(string id)
	{
		AddParam("id", id);
		return this;
	}
}
