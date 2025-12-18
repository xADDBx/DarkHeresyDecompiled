using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[ComponentName("Allows character to recover HPs after the battle")]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowedOn(typeof(BlueprintUnit))]
[TypeId("9f64757ab02ac49479f07ae1149b1991")]
public class WarhammerAfterBattleRecovery : UnitFactComponentDelegate
{
	protected override void OnActivate()
	{
	}

	protected override void OnDeactivate()
	{
	}
}
