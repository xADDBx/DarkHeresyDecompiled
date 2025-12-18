using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Gameplay.Features.Reputation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Requirements;

[Obsolete]
[AllowedOn(typeof(BlueprintAnswer))]
[TypeId("d049148cefe24aeebe1fc1c10414f7db")]
public class RequirementReputationCost : Requirement
{
	[SerializeField]
	public int Reputation;

	[SerializeField]
	public FactionType Faction;
}
