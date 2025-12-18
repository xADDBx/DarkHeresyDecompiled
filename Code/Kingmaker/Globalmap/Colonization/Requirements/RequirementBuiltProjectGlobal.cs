using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Requirements;

[Obsolete]
[AllowedOn(typeof(BlueprintColonyProject))]
[AllowMultipleComponents]
[TypeId("d0d6c340d59f4a6eaba89d28c0f31ea3")]
public class RequirementBuiltProjectGlobal : Requirement
{
	[SerializeField]
	private BlueprintColonyProjectReference m_BuiltProject;

	public BlueprintColonyProject BuiltProject => m_BuiltProject?.Get();
}
