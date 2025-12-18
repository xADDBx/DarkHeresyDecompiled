using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Buffs.Components;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnitFact))]
[AllowedOn(typeof(BlueprintUnit))]
[ComponentName("CriticalEffects/IncreaseCriticalHitMultiplier")]
[TypeId("fd97df42621d478398a398971d171ef8")]
public class IncreaseCriticalHitMultiplier : UnitBuffComponentDelegate
{
}
