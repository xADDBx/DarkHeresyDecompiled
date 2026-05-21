using Kingmaker.Gameplay.Features.Encounter;

namespace Kingmaker.Code.Middleware.Metrics;

public class EncounterFinishMetricsEvent : MetricsEvent
{
	protected override string Name => "encounter_finish";

	public EncounterFinishMetricsEvent PartyHealth(int party_health)
	{
		AddParam("party_health", party_health.ToString());
		return this;
	}

	public EncounterFinishMetricsEvent PartyArmour(int party_armour)
	{
		AddParam("party_armour", party_armour.ToString());
		return this;
	}

	public EncounterFinishMetricsEvent Reason(EncounterCompletionType reason)
	{
		AddParam("reason", reason switch
		{
			EncounterCompletionType.Custom => "custom", 
			EncounterCompletionType.Morale => "morale", 
			EncounterCompletionType.AllEnemiesDead => "all_enemies_dead", 
			_ => MetricsUtils.EnumToSnakeCase(reason), 
		});
		return this;
	}

	public EncounterFinishMetricsEvent PowerBalance(float power_balance)
	{
		AddParam("power_balance", power_balance.ToString("F2"));
		return this;
	}

	public EncounterFinishMetricsEvent DamageToParty(int damage_to_party)
	{
		AddParam("damage_to_party", damage_to_party.ToString());
		return this;
	}

	public EncounterFinishMetricsEvent DamageToEnemies(int damage_to_enemies)
	{
		AddParam("damage_to_enemies", damage_to_enemies.ToString());
		return this;
	}

	public EncounterFinishMetricsEvent AdditionalGoal(string additional_goal)
	{
		AddParam("additional_goal", additional_goal);
		return this;
	}

	public EncounterFinishMetricsEvent AdditionalGoalState(bool additional_goal_state)
	{
		AddParam("additional_goal_state", additional_goal_state.ToString());
		return this;
	}

	public EncounterFinishMetricsEvent Difficulty(string difficulty)
	{
		AddParam("difficulty", difficulty);
		return this;
	}

	public EncounterFinishMetricsEvent CombatLog(bool combat_log)
	{
		AddParam("combat_log", combat_log.ToString());
		return this;
	}
}
