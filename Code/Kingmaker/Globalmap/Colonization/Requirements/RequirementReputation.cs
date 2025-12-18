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
[AllowMultipleComponents]
[TypeId("103ff90d034f4fa29f5250f54ba296ed")]
public class RequirementReputation : Requirement
{
	[SerializeField]
	private FactionReputation m_Reputation;

	public FactionReputation Reputation => m_Reputation;
}
