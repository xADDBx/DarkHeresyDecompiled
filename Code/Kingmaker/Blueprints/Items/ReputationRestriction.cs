using System;
using Kingmaker.Gameplay.Features.Reputation;

namespace Kingmaker.Blueprints.Items;

[Serializable]
public sealed class ReputationRestriction
{
	public static readonly ReputationRestriction Empty = new ReputationRestriction();

	public ReputationType Type;

	public int Value;

	public bool IsPassed(FactionType factionType)
	{
		return Game.Instance.Reputation.Get(factionType, Type) >= Value;
	}
}
