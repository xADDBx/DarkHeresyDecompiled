using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Globalmap.Colonization.Rewards;

[Obsolete]
[AllowedOn(typeof(BlueprintColonyProject))]
[TypeId("1c4c3ee98f054592aa7d83d558d79c74")]
public class RewardChangeNewPassageCost : Reward
{
	public int NewCost;
}
