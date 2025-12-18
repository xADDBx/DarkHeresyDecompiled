using System;
using Kingmaker.ElementsSystem;
using Kingmaker.Utility.Attributes;
using Owlcat.Fmw.Blueprints;

namespace Kingmaker.Blueprints.Items;

[Serializable]
public sealed class VendorTableSlot
{
	public int Id;

	public bool Disable;

	[HideIf("Disable")]
	public BpRef<BlueprintItem> Item;

	[HideIf("Disable")]
	public int Count;

	[HideIf("Disable")]
	public bool Replenishable;

	[HideIf("Disable")]
	public int OverrideCost;

	[HideIf("Disable")]
	public ReputationRestriction ReputationRestriction;

	[HideIf("Disable")]
	public ConditionsChecker Restriction;
}
