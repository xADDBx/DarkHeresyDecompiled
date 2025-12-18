namespace Kingmaker.Code.Middleware.Metrics;

public class LevelUpMetricsEvent : MetricsEvent
{
	protected override string Name => "level_up";

	public LevelUpMetricsEvent(bool isGameEvent)
		: base(isGameEvent)
	{
	}

	public LevelUpMetricsEvent SelectionId(string selection)
	{
		AddParam("selection", selection);
		return this;
	}

	public LevelUpMetricsEvent CompanionId(string companion)
	{
		AddParam("companion", companion);
		return this;
	}
}
