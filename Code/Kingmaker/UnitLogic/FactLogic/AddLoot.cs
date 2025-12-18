using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Loot;
using Kingmaker.ElementsSystem.ContextData;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[ComponentName("Add loot")]
[AllowedOn(typeof(BlueprintUnit))]
[AllowMultipleComponents]
[TypeId("aea59f4a6ffae1e45a67d731f3f7908f")]
public class AddLoot : UnitFactComponentDelegate
{
	[SerializeField]
	private BlueprintUnitLootReference m_Loot;

	public BlueprintUnitLootReference LootReference => m_Loot;

	protected override void OnFactAttached()
	{
		if (!ContextData<UnitHelper.DoNotCreateItems>.Current && !m_Loot.IsEmpty())
		{
			m_Loot.Get().GenerateItems().ForEach(delegate(LootEntry i)
			{
				base.Owner.Inventory.Add(i.Item, i.Count);
			});
		}
	}
}
