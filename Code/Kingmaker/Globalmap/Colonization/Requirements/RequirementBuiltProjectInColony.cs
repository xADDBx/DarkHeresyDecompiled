using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Requirements;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintColonyProject))]
[TypeId("4e862cff9aff4bc7a099ebfaad96b51f")]
public class RequirementBuiltProjectInColony : Requirement
{
	[SerializeField]
	private BlueprintColonyProjectReference m_BuiltProject;

	public BlueprintColonyProject BuiltProject => m_BuiltProject?.Get();
}
