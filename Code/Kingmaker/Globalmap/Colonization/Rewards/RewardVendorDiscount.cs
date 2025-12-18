using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Gameplay.Features.Reputation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Rewards;

[Obsolete]
[AllowedOn(typeof(BlueprintAnswer))]
[TypeId("3944ab5e54f6449f91fab75d1909a8b5")]
public class RewardVendorDiscount : Reward
{
	[SerializeField]
	public FactionType Faction;

	[SerializeField]
	public int Discount;
}
