using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Loot;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence.Versioning.PlayerUpgraderOnlyActions;

[Obsolete]
[TypeId("4ffd0b6c9d8e49ab8236f8877261ca6e")]
public class FixAddingChapterLootToVendorTables : PlayerUpgraderOnlyAction
{
	[ValidateNotNull]
	[SerializeField]
	private BlueprintSharedVendorTableReference m_Table;

	[ValidateNotNull]
	[SerializeField]
	private BlueprintUnitLootReference m_Loot;

	private BlueprintSharedVendorTable Table => m_Table?.Get();

	private BlueprintUnitLoot Loot => m_Loot?.Get();

	public override string GetCaption()
	{
		if (Table != null)
		{
			return $"Fix Loot not added properly to {Table}";
		}
		return "FixAddingChapterLootToVendorTables not configured properly";
	}

	protected override void RunActionOverride()
	{
	}
}
