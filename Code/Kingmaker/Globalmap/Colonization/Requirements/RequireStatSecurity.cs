using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Requirements;

[Obsolete]
[AllowedOn(typeof(BlueprintColonyProject))]
[AllowedOn(typeof(BlueprintAnswer))]
[TypeId("47b854d1c8dc4a22b15dea737ed17c4a")]
public class RequireStatSecurity : Requirement
{
	[SerializeField]
	private int m_MinSecurityValue;

	public int MinSecurityValue => m_MinSecurityValue;
}
