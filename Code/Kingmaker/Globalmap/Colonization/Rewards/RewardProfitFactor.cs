using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.Blueprints.Quests;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Rewards;

[Obsolete]
[TypeId("d5f96b38298e4c8992d6c72731bc9ca7")]
[AllowedOn(typeof(BlueprintColonyProject))]
[AllowedOn(typeof(BlueprintQuestContract))]
[AllowedOn(typeof(BlueprintAnswer))]
[AllowedOn(typeof(BlueprintCue))]
public class RewardProfitFactor : Reward
{
	[SerializeField]
	private int m_ProfitFactor;

	public int ProfitFactor => m_ProfitFactor;
}
