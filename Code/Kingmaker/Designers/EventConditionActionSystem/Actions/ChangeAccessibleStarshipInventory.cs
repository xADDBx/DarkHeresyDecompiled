using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Serializable]
[Obsolete]
[PlayerUpgraderAllowed(false)]
[TypeId("106327c1089dd0242b12633b4dc0f8e3")]
public class ChangeAccessibleStarshipInventory : GameAction
{
	[Serializable]
	public enum StarshipInventoryAvailability
	{
		Available,
		NotAvailable
	}

	[SerializeField]
	public StarshipInventoryAvailability Value;

	public override string GetCaption()
	{
		return "Change Starship Inventory Accessibility";
	}

	protected override void RunAction()
	{
	}
}
