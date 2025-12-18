using System;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Rewards;

[Obsolete]
[TypeId("1b57733e7a114347bab0daeeaa3a5864")]
public class RewardModifyWoundDamagePerTurnThresholdHPFraction : Reward
{
	[Tooltip("Wounds threshold hp for player's party will be greater on value percent")]
	public int WoundDamagePerTurnThresholdHPFractionModifier;
}
