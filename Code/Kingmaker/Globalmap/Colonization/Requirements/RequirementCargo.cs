using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Cargo;
using Kingmaker.DialogSystem.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Requirements;

[Obsolete]
[AllowedOn(typeof(BlueprintAnswer))]
[TypeId("d35d0b0ef76746a5a3521dbfd22c654c")]
public class RequirementCargo : Requirement
{
	[SerializeField]
	private BlueprintCargoReference m_Cargo;

	[SerializeField]
	private int m_Count = 1;

	public BlueprintCargo Cargo => m_Cargo?.Get();

	public int Count => m_Count;
}
