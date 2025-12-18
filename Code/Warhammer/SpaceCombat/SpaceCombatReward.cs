using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Warhammer.SpaceCombat;

[Obsolete]
[AllowedOn(typeof(BlueprintArea))]
[AllowedOn(typeof(BlueprintStarship))]
[AllowMultipleComponents]
[TypeId("a1ed0cc8887c45d299b2774d4c525329")]
public class SpaceCombatReward : BlueprintComponent
{
	[Serializable]
	public class Reward
	{
		[Serializable]
		public struct RewardCondition
		{
			[SerializeField]
			public bool Not;

			[SerializeField]
			public bool HasBuff;

			[SerializeField]
			public bool CheckOnPlayerShipInstead;

			[SerializeField]
			[ShowIf("HasBuff")]
			private BlueprintBuffReference m_Buff;

			public BlueprintBuff Buff => m_Buff?.Get();
		}

		public List<BlueprintItemReference> Items;

		public List<int> ItemCounts;

		public List<BlueprintCargoReference> Cargoes;

		public int Scrap;

		[SerializeField]
		public List<RewardCondition> Condition;
	}

	[SerializeField]
	public List<Reward> Rewards;
}
