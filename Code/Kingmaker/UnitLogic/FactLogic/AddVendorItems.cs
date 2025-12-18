using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Loot;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[ComponentName("Add vendor items")]
[AllowedOn(typeof(BlueprintUnit))]
[AllowedOn(typeof(BlueprintAnomaly))]
[AllowMultipleComponents]
[TypeId("467fddff82e2032428ce9ceb134b552e")]
public class AddVendorItems : UnitFactComponentDelegate
{
	[ValidateNotNull]
	[SerializeField]
	private BlueprintUnitLootReference m_Loot;

	public BlueprintUnitLoot Loot => m_Loot?.Get();

	protected override void OnActivate()
	{
	}
}
