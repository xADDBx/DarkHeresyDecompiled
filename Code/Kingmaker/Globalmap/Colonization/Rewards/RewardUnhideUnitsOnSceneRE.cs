using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
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
[TypeId("108f649ae88b4fc1aa06d5453b3071e9")]
public class RewardUnhideUnitsOnSceneRE : Reward
{
	[Serializable]
	private class SpawnersByEnterPoint
	{
		[SerializeField]
		private BlueprintAreaEnterPointReference m_EnterPoint;

		[SerializeField]
		[AllowedEntityType(typeof(UnitSpawnerBase))]
		public EntityReference Spawner;

		public BlueprintAreaEnterPoint EnterPoint => m_EnterPoint?.Get();
	}

	[SerializeField]
	private BlueprintUnitReference m_Unit;

	[SerializeField]
	private List<SpawnersByEnterPoint> m_Spawners;

	private BlueprintUnit Unit => m_Unit?.Get();
}
