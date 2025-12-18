using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.Blueprints.Quests;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Gameplay.Features.Reputation;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Rewards;

[Obsolete]
[AllowedOn(typeof(BlueprintColonyProject))]
[AllowedOn(typeof(BlueprintQuestContract))]
[AllowedOn(typeof(BlueprintAnswer))]
[AllowMultipleComponents]
[TypeId("6b753da00905420996556f7cd398267c")]
public class RewardReputation : Reward
{
	[SerializeField]
	public int Reputation;

	[SerializeField]
	public FactionType Faction;
}
