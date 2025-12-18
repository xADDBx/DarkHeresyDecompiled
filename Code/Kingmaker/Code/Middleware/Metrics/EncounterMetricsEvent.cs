using Kingmaker.Gameplay.Features.Encounter;

namespace Kingmaker.Code.Middleware.Metrics;

public class EncounterMetricsEvent : MetricsEvent
{
	public enum EncounterStates
	{
		Start,
		Finish,
		Load
	}

	protected override string Name => "encounter";

	public EncounterMetricsEvent(bool isGameEvent)
		: base(isGameEvent)
	{
	}

	public EncounterMetricsEvent Id(string id)
	{
		AddParam("id", id);
		return this;
	}

	public EncounterMetricsEvent EncounterState(EncounterStates state)
	{
		AddParam("state", state switch
		{
			EncounterStates.Start => "start", 
			EncounterStates.Finish => "finish", 
			EncounterStates.Load => "load", 
			_ => MetricsEvent.EnumToSnakeCase(state), 
		});
		return this;
	}

	public EncounterMetricsEvent WinReason(EncounterCompletionType reason)
	{
		AddParam("reason", reason switch
		{
			EncounterCompletionType.Custom => "custom", 
			EncounterCompletionType.Morale => "morale", 
			EncounterCompletionType.AllEnemiesDead => "all_enemies_dead", 
			_ => MetricsEvent.EnumToSnakeCase(reason), 
		});
		return this;
	}

	public EncounterMetricsEvent PowerBalance(float power_balance)
	{
		AddParam("power_balance", power_balance.ToString("F2"));
		return this;
	}
}
