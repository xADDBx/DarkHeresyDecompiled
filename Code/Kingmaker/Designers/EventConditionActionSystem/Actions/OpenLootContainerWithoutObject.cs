using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Loot;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[TypeId("c2703d1959217704e826fc2e38a8852a")]
public class OpenLootContainerWithoutObject : GameAction
{
	[SerializeField]
	private List<LootEntry> m_ExplorationLoot = new List<LootEntry>();

	public override string GetCaption()
	{
		return "Открывает лут контейнер с заданным списком лута";
	}

	protected override void RunAction()
	{
	}
}
