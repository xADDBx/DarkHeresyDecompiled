using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Requirements;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintColonyProject))]
[TypeId("65671d15080b4532a019b3576b24d6c1")]
public class RequirementNotBuiltProjectInColony : Requirement
{
	[SerializeField]
	private BlueprintColonyProjectReference m_NotBuiltProject;

	public BlueprintColonyProject NotBuiltProject => m_NotBuiltProject?.Get();
}
