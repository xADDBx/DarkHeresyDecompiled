using System;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[Obsolete]
[TypeId("a39b89cb076a6ba4eaaf7dbd22494793")]
[PlayerUpgraderAllowed(false)]
public class PlayerStarshipHasComponent : Condition
{
	[SerializeField]
	private BlueprintStarshipItem.Reference[] m_Items;

	public ReferenceArrayProxy<BlueprintStarshipItem> Items
	{
		get
		{
			BlueprintReference<BlueprintStarshipItem>[] items = m_Items;
			return items;
		}
	}

	protected override string GetConditionCaption()
	{
		return "Player ship has any component from list";
	}

	protected override bool CheckCondition()
	{
		return false;
	}
}
