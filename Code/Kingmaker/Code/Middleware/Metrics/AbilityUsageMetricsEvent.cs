using System.Collections.Generic;

namespace Kingmaker.Code.Middleware.Metrics;

public class AbilityUsageMetricsEvent : MetricsEvent
{
	protected override string Name => "ability";

	public AbilityUsageMetricsEvent(bool isGameEvent)
		: base(isGameEvent)
	{
	}

	public AbilityUsageMetricsEvent Id(string id)
	{
		AddParam("id", id);
		return this;
	}

	public AbilityUsageMetricsEvent Caster(string caster)
	{
		AddParam("caster", caster);
		return this;
	}

	public AbilityUsageMetricsEvent Toggles(IEnumerable<string> toggles)
	{
		AddParam("toggles", toggles);
		return this;
	}

	public AbilityUsageMetricsEvent Modifiers(IEnumerable<string> modifiers)
	{
		AddParam("modifiers", modifiers);
		return this;
	}
}
