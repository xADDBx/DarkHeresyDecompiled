using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Requirements;

[Obsolete]
[AllowedOn(typeof(BlueprintAnswer))]
[AllowMultipleComponents]
[TypeId("b630897d1c2f4f41ab197acbe74d14b7")]
public class RequirementResourceUseDialog : Requirement
{
	[SerializeField]
	private ResourceData Resource;

	[SerializeField]
	public bool UseProfitFactorInstead;

	public BlueprintResource ResourceBlueprint => Resource?.Resource?.Get();

	public int Count => Resource?.Count ?? 0;
}
