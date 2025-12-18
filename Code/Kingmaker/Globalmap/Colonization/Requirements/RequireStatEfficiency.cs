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
[TypeId("5fafbc860bd3431d908d2da53bd85c51")]
public class RequireStatEfficiency : Requirement
{
	[SerializeField]
	private int m_MinEfficiencyValue;

	public int MinEfficiencyValue => m_MinEfficiencyValue;
}
