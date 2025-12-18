using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.Blueprints.Quests;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.View.Spawners;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Rewards;

[Obsolete]
[AllowedOn(typeof(BlueprintColonyProject))]
[AllowedOn(typeof(BlueprintQuestContract))]
[TypeId("9b7199fa7cbf4ad1ab8b763180d49ea6")]
public class RewardActivateSpawners : Reward
{
	[SerializeField]
	[AllowedEntityType(typeof(UnitSpawnerBase))]
	private List<EntityReference> m_Spawners;

	[SerializeField]
	private RewardActivateSpawnersType m_Type;

	public RewardActivateSpawnersType Type => m_Type;
}
