using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("25c4a6865d92b6d4fac323b707b01471")]
public class AddInitiatorHealTrigger : BlueprintComponent
{
	public ActionList Action;

	public ActionList HealerAction;

	public bool OnHealDamage;

	public bool OnHealStatDamage;

	public bool OnHealEnergyDrain;
}
