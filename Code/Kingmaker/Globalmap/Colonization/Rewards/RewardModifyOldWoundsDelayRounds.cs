using System;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Rewards;

[Obsolete]
[TypeId("a1beb39017ed442e9da4de4e19440fb1")]
public class RewardModifyOldWoundsDelayRounds : Reward
{
	[Tooltip("old wounds delay rounds for player's party will be greater on value")]
	public int OldWoundsDelayRoundsModifier;
}
