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
[TypeId("00d0b0c8771b486c90548c840664bf51")]
public class RequirementStatContentment : Requirement
{
	[SerializeField]
	private int m_MinContentmentValue;

	public int MinContentmentValue => m_MinContentmentValue;
}
