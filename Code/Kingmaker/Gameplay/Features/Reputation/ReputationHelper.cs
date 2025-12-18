using System;
using Core.Cheats;

namespace Kingmaker.Gameplay.Features.Reputation;

public static class ReputationHelper
{
	public static ReputationDescription GetReputation(FactionType faction)
	{
		return Game.Instance.Reputation.Get(faction);
	}

	[Cheat(Name = "add_reputation_all", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void Cheat_AddReputationAll(ReputationType reputationType, int value)
	{
		Cheat_AddReputation(FactionType.AstraMilitarum, reputationType, value);
		Cheat_AddReputation(FactionType.AdeptusMechanicus, reputationType, value);
		Cheat_AddReputation(FactionType.MinistorumAndRedemptionists, reputationType, value);
		Cheat_AddReputation(FactionType.RulingCouncilOfNobles, reputationType, value);
	}

	[Cheat(Name = "add_reputation", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void Cheat_AddReputation(FactionType faction, ReputationType reputationType, int value)
	{
		Game.Instance.Reputation.Add(faction, reputationType, value);
	}

	[Obsolete]
	public static int? GetNextLvl(FactionType factionType)
	{
		return null;
	}

	[Obsolete]
	public static int GetCurrentReputationLevel(FactionType factionType)
	{
		return 0;
	}

	[Obsolete]
	public static bool IsMaxReputation(FactionType factionType)
	{
		return false;
	}

	[Obsolete]
	public static float GetProgressToNextLevel(FactionType factionType)
	{
		return 0f;
	}

	[Obsolete]
	public static int? GetNextLevelReputationPoints(FactionType factionType)
	{
		return null;
	}

	[Obsolete]
	public static int GetCurrentReputationPoints(FactionType faction)
	{
		return 0;
	}

	[Obsolete]
	public static int GetReputationLevelByPoints(FactionType factionType, int points)
	{
		return 0;
	}

	[Obsolete]
	public static int GetReputationPointsByLevel(FactionType factionType, int level)
	{
		return 0;
	}
}
