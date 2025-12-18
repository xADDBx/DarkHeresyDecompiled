using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.UnitLogic.Alignments;
using Owlcat.Runtime.Core.Utility;
using UnityEngine.Serialization;

namespace Kingmaker.Globalmap.Colonization.Requirements;

[Obsolete]
[AllowedOn(typeof(BlueprintColonyProject))]
[AllowedOn(typeof(BlueprintAnswer))]
[TypeId("29147804bf4e4746bbab4eaf5c22d11e")]
public class RequirementSoulMarkRank : Requirement
{
	[FormerlySerializedAs("ConvictionRequirement")]
	public AlignmentShift alignmentRequirement;
}
