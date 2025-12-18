using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Stats.Base;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("d756fbdf6d2d4796b9d953fbd4f8047a")]
public class AddImmunityToAbilityScoreDamage : BlueprintComponent
{
	public bool Drain;

	[ValidateNoNullEntries]
	public StatType[] StatTypes;
}
