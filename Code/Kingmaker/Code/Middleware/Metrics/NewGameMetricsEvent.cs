namespace Kingmaker.Code.Middleware.Metrics;

public class NewGameMetricsEvent : MetricsEvent
{
	protected override string Name => "new_game";

	public NewGameMetricsEvent Difficulty(string difficulty)
	{
		AddParam("difficulty", difficulty);
		return this;
	}

	public NewGameMetricsEvent Preset(string preset)
	{
		AddParam("preset", preset);
		return this;
	}
}
