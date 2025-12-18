using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Loot;
using Kingmaker.Enums;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.Blueprints.Exploration;

[Obsolete]
[TypeId("9f5179678cb74615a98b6222787ab6ea")]
public class BlueprintPointOfInterestLoot : BlueprintPointOfInterest
{
	public TrashLootAmount TrashLootAmount = TrashLootAmount.Percent100;

	public LootSetting Setting;

	[SerializeField]
	private List<LootEntry> m_ExplorationLoot = new List<LootEntry>();

	public List<LootEntry> ExplorationLoot => m_ExplorationLoot;

	public float CargoVolumePercent => 0f;

	public void FillExplorationLoot(LootEntry[] loot)
	{
		ExplorationLoot.Clear();
		ExplorationLoot.AddRange(loot);
	}
}
