using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Loot;
using Kingmaker.Globalmap.Exploration;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.Blueprints.Exploration;

[Obsolete]
[TypeId("c7217a916d3f4676859581eebc813640")]
public class BlueprintPointOfInterestStatCheckLoot : BlueprintPointOfInterest
{
	[SerializeField]
	public List<StatDC> Stats;

	[SerializeField]
	private List<LootEntry> m_CheckPassedLoot;

	[SerializeField]
	private List<LootEntry> m_CheckFailedLoot;

	public List<LootEntry> CheckPassedLoot => m_CheckPassedLoot;

	public List<LootEntry> CheckFailedLoot => m_CheckFailedLoot;
}
