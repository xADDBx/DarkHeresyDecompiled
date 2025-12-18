using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Loot;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.Blueprints.Exploration;

[Obsolete]
[TypeId("d65512b706a24e399b8a87cdecbde1ff")]
public class BlueprintPointOfInterestExpedition : BlueprintPointOfInterest
{
	[Serializable]
	public class ExpeditionLoot
	{
		[SerializeField]
		[Range(0f, 1f)]
		public float ExpeditionProportion;

		[SerializeField]
		public List<LootEntry> Loot;
	}

	[SerializeField]
	public List<ExpeditionLoot> Rewards;

	[SerializeField]
	public int MaxExpeditionPeopleCount;
}
