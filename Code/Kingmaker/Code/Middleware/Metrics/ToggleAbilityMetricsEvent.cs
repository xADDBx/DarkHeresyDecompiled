namespace Kingmaker.Code.Middleware.Metrics;

public class ToggleAbilityMetricsEvent : MetricsEvent
{
	protected override string Name => "toggle_ability";

	public ToggleAbilityMetricsEvent Id(string id)
	{
		AddParam("id", id);
		return this;
	}

	public ToggleAbilityMetricsEvent State(bool state)
	{
		AddParam("state", state ? "enabled" : "disabled");
		return this;
	}
}
