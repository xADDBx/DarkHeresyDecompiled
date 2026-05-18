using System.Collections.Generic;

namespace Kingmaker.Code.Middleware.Metrics;

public class EncounterCompanionStartMetricsEvent : MetricsEvent
{
	protected override string Name => "encounter_start_companion";

	public EncounterCompanionStartMetricsEvent Id(string id)
	{
		AddParam("id", id);
		return this;
	}

	public EncounterCompanionStartMetricsEvent Equipment(IEnumerable<string> equipment)
	{
		AddParam("equipment", equipment);
		return this;
	}

	public EncounterCompanionStartMetricsEvent Abilities(IEnumerable<string> abilities)
	{
		AddParam("abilities", abilities);
		return this;
	}

	public EncounterCompanionStartMetricsEvent ModifiersTargetsId(IEnumerable<string> modifiers_targets_id)
	{
		AddParam("modifiers_targets_id", modifiers_targets_id);
		return this;
	}

	public EncounterCompanionStartMetricsEvent ModifiersTargetsAbility(IEnumerable<string> modifiers_targets_ability)
	{
		AddParam("modifiers_targets_ability", modifiers_targets_ability);
		return this;
	}
}
