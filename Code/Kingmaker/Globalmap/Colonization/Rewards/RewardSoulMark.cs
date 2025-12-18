using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.UnitLogic.Alignments;
using Owlcat.Runtime.Core.Utility;
using UnityEngine.Serialization;

namespace Kingmaker.Globalmap.Colonization.Rewards;

[Obsolete]
[AllowedOn(typeof(BlueprintColonyProject))]
[AllowedOn(typeof(BlueprintAnswer))]
[TypeId("c8994546dfb24e6da61004bb7598afda")]
public class RewardSoulMark : Reward
{
	[FormerlySerializedAs("ConvictionShift")]
	public AlignmentShift alignmentShift;
}
