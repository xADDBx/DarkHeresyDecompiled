using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[AllowedOn(typeof(BlueprintUnit))]
[AllowedOn(typeof(BlueprintAnomaly))]
[TypeId("0cc6d74312fa44a69eaacfe59935c4c4")]
public class AddVendorFaction : UnitFactComponentDelegate
{
	[ValidateNotNull]
	[SerializeField]
	private BpRef<BlueprintVendorFaction> m_Faction;

	public BlueprintVendorFaction Faction => m_Faction;

	protected override void OnActivate()
	{
		base.Owner.GetOrCreate<PartVendor>().SetVendorFaction(Faction);
	}
}
