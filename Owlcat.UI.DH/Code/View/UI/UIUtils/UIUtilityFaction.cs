using System;
using Kingmaker.Gameplay.Features.Reputation;

namespace Code.View.UI.UIUtils;

public static class UIUtilityFaction
{
	public static string GetEncyclopediaName(FactionType faction)
	{
		return faction switch
		{
			FactionType.None => string.Empty, 
			FactionType.AstraMilitarum => "AstraMilitarumFaction", 
			FactionType.AdeptusMechanicus => "AdeptusMechanicusFaction", 
			FactionType.RulingCouncilOfNobles => "RulingCouncilOfNoblesFaction", 
			FactionType.MinistorumAndRedemptionists => "MinistorumAndRedemptionistsFaction", 
			_ => throw new ArgumentOutOfRangeException("faction", faction, null), 
		};
	}

	public static string GetEncyclopediaName(ReputationType reputation)
	{
		return reputation switch
		{
			ReputationType.Respect => "Rapport", 
			ReputationType.Fear => "Presence", 
			_ => throw new ArgumentOutOfRangeException("reputation", reputation, null), 
		};
	}
}
