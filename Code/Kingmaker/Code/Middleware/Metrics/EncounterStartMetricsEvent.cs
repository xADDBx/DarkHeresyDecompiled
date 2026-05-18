namespace Kingmaker.Code.Middleware.Metrics;

public class EncounterStartMetricsEvent : MetricsEvent
{
	protected override string Name => "encounter_start";

	public EncounterStartMetricsEvent IsLoaded(bool loaded)
	{
		AddParam("loaded", loaded.ToString());
		return this;
	}

	public EncounterStartMetricsEvent PartyCount(int party_count)
	{
		AddParam("party_count", party_count.ToString());
		return this;
	}

	public EncounterStartMetricsEvent EnemiesCount(int enemies_count)
	{
		AddParam("enemies_count", enemies_count.ToString());
		return this;
	}

	public EncounterStartMetricsEvent DamageToParty(int damage_to_party)
	{
		AddParam("damage_to_party", damage_to_party.ToString());
		return this;
	}

	public EncounterStartMetricsEvent DamageToEnemies(int damage_to_enemies)
	{
		AddParam("damage_to_enemies", damage_to_enemies.ToString());
		return this;
	}

	public EncounterStartMetricsEvent PartyHealth(int party_health)
	{
		AddParam("party_health", party_health.ToString());
		return this;
	}

	public EncounterStartMetricsEvent PartyArmour(int party_armour)
	{
		AddParam("party_armour", party_armour.ToString());
		return this;
	}

	public EncounterStartMetricsEvent ExperienceLevel(int experience_level)
	{
		AddParam("experience_level", experience_level.ToString());
		return this;
	}

	public EncounterStartMetricsEvent Difficulty(string difficulty)
	{
		AddParam("difficulty", difficulty);
		return this;
	}

	public EncounterStartMetricsEvent CombatLog(bool combat_log)
	{
		AddParam("combat_log", combat_log.ToString());
		return this;
	}
}
