using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[AllowedOn(typeof(BlueprintUnit))]
[TypeId("abb987a0bcaf4f668297b79e447f4763")]
public class AddSharedVendor : EntityFactComponentDelegate<MechanicEntity>
{
	[ValidateNotNull]
	[SerializeField]
	private BpRef<BlueprintSharedVendorTable> m_Table;

	[ValidateNotNull]
	[SerializeField]
	private BpRef<BlueprintVendorFaction> m_Faction;

	[SerializeField]
	private bool m_NeedHideCostAndReputation;

	public BlueprintSharedVendorTable Table => m_Table;

	public BlueprintVendorFaction Faction => m_Faction;

	public bool NeedHideCostAndReputation => m_NeedHideCostAndReputation;

	protected override void OnActivate()
	{
		base.Owner.GetOrCreate<PartVendor>().SetSharedInventory(Table);
		base.Owner.GetOrCreate<PartVendor>().SetVendorFaction(Faction);
	}
}
