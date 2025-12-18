using System;
using Kingmaker.Gameplay.Features.Reputation;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Requirements;

[Serializable]
public class FactionReputation
{
	[SerializeField]
	public FactionType Faction;

	[SerializeField]
	public int MinLevelValue;
}
