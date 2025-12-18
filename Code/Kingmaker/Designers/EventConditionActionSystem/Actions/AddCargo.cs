using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Loot;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.UI.Common;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[PlayerUpgraderAllowed(false)]
[TypeId("343049de4e36454c85b36f38485730f3")]
public class AddCargo : GameAction
{
	public ItemsItemOrigin m_Origin;

	public BlueprintLootReference m_Loot;

	public BlueprintLoot Loot => m_Loot?.Get();

	public override string GetCaption()
	{
		return $"Create and add cargo from {Loot}";
	}

	protected override void RunAction()
	{
	}
}
