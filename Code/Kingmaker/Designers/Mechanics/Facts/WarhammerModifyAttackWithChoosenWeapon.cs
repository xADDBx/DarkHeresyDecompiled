using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("1d90fd0202fe4c54c92d5a2f2b94ed0c")]
public class WarhammerModifyAttackWithChoosenWeapon : BlueprintComponent
{
	public ContextValue AdditionalHitChances;

	public ContextValue AdditionalDamageMin;

	public ContextValue AdditionalDamageMax;

	public ContextValue AdditionalArmorPenetration;

	public ContextValue ParryNegation;

	public ContextValue RecoilBonusPercent;

	public ContextValue BurstCountPercent;

	public ContextValue BurstCountBonusMinimum;
}
