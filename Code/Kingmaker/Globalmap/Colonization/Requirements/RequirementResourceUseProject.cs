using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Requirements;

[Obsolete]
[AllowedOn(typeof(BlueprintColonyProject))]
[AllowMultipleComponents]
[TypeId("e358bb9eda304804bc36c2b8b0e704e5")]
public class RequirementResourceUseProject : Requirement
{
	[SerializeField]
	private ResourceData Resource;

	public BlueprintResource ResourceBlueprint => Resource?.Resource?.Get();

	public int Count => Resource?.Count ?? 0;
}
