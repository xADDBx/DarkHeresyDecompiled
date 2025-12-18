using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.Blueprints.Quests;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Requirements;

[Obsolete]
[AllowedOn(typeof(BlueprintColonyProject))]
[AllowedOn(typeof(BlueprintQuestContract))]
[AllowedOn(typeof(BlueprintAnswer))]
[TypeId("185bebc1634848bc9dc78c6268e20ff3")]
public class RequirementResourceHave : Requirement
{
	[SerializeField]
	private ResourceData m_Resource;

	public BlueprintResource Resource => m_Resource?.Resource?.Get();

	public int Count => m_Resource.Count;
}
