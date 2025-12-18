using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.Blueprints.Quests;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Requirements;

[Obsolete]
[AllowedOn(typeof(BlueprintQuestContract))]
[TypeId("feb2e0d661b64c6d8d255a0b8c3fb239")]
[AllowMultipleComponents]
public class RequirementResourceUseOrder : Requirement
{
	[SerializeField]
	private ResourceData Resource;

	public BlueprintResource ResourceBlueprint => Resource?.Resource?.Get();

	public int Count => Resource?.Count ?? 0;
}
