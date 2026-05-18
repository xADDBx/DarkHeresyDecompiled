using Kingmaker.Gameplay.Features.Reputation;

namespace Kingmaker.Code.Middleware.Metrics;

public class ReputationMetricsEvent : MetricsEvent
{
	protected override string Name => "reputation";

	public ReputationMetricsEvent Faction(FactionType faction)
	{
		AddParam("faction", faction switch
		{
			FactionType.AdeptusMechanicus => "adeptus_mechanicus", 
			FactionType.AstraMilitarum => "astra_militarum", 
			FactionType.MinistorumAndRedemptionists => "ministorum_and_redemptionists", 
			FactionType.RulingCouncilOfNobles => "ruling_council_of_nobles", 
			FactionType.None => "none", 
			_ => MetricsUtils.EnumToSnakeCase(faction), 
		});
		return this;
	}

	public ReputationMetricsEvent Type(ReputationType type)
	{
		AddParam("type", type switch
		{
			ReputationType.Fear => "fear", 
			ReputationType.Respect => "respect", 
			_ => MetricsUtils.EnumToSnakeCase(type), 
		});
		return this;
	}

	public ReputationMetricsEvent Value(int value)
	{
		AddParam("value", value.ToString());
		return this;
	}

	public ReputationMetricsEvent CharacterLevel(int group_level)
	{
		AddParam("group_level", group_level.ToString());
		return this;
	}
}
