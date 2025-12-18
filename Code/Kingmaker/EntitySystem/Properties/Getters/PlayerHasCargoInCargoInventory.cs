using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Cargo;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Obsolete]
[TypeId("a27392a3622347b98de33636db16f1db")]
public class PlayerHasCargoInCargoInventory : IntPropertyGetter
{
	[ValidateNotNull]
	[SerializeField]
	private BlueprintCargoReference m_Cargo;

	[SerializeField]
	private int m_CargoAmount;

	private BlueprintCargo Cargo => m_Cargo.Get();

	protected override int GetBaseValue()
	{
		return 0;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		string text = ((Cargo == null) ? "NULL" : (Cargo.name ?? ""));
		return "Player has " + text + " in his Cargo Inventory";
	}
}
