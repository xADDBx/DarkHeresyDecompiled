using System.Collections.Generic;

namespace Kingmaker.Code.Middleware.Metrics;

public class EncounterCompanionFinishMetricsEvent : MetricsEvent
{
	protected override string Name => "encounter_finish_companion";

	public EncounterCompanionFinishMetricsEvent Id(string id)
	{
		AddParam("id", id);
		return this;
	}

	public EncounterCompanionFinishMetricsEvent AbilitiesUsagesId(IEnumerable<string> abilities_usages_id)
	{
		AddParam("abilities_usages_id", abilities_usages_id);
		return this;
	}

	public EncounterCompanionFinishMetricsEvent AbilitiesUsagesCount(IEnumerable<string> abilities_usages_count)
	{
		AddParam("abilities_usages_count", abilities_usages_count);
		return this;
	}
}
