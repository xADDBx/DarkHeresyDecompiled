namespace Kingmaker.Code.Middleware.Metrics;

public class LevelUpMetricsEvent : MetricsEvent
{
	protected override string Name => "level_up";

	public LevelUpMetricsEvent Level(int level)
	{
		AddParam("level", level.ToString());
		return this;
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
