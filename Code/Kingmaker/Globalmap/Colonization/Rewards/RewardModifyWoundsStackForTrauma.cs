using System;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Rewards;

[Obsolete]
[TypeId("8fc9c4343c1c482bab74aed73a034b01")]
public class RewardModifyWoundsStackForTrauma : Reward
{
	[Tooltip("Wounds stack for player's party will be greater on value")]
	public int WoundsStackModifier;
}
