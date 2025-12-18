using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Loot;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.Exploration;

[Obsolete]
[TypeId("3da4d82fd1064a8c82680cf4008b167a")]
public class AnomalyResearch : AnomalyInteraction
{
	[SerializeField]
	public List<StatDC> Stats;

	[SerializeField]
	public List<LootEntry> CheckPassedLoot;

	[SerializeField]
	public List<LootEntry> CheckFailedLoot;

	[SerializeField]
	public ActionList Fail;

	[SerializeField]
	public ActionList Success;
}
