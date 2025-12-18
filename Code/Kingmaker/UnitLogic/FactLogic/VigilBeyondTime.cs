using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowedOn(typeof(BlueprintUnit))]
[TypeId("0ec14c9118234c84bf37e3fc1b56ece6")]
public class VigilBeyondTime : UnitBuffComponentDelegate
{
	public BlueprintAbilityReference TeleportAbility;

	protected override void OnActivate()
	{
		base.Context.MaybeCaster?.GetOrCreate<UnitPartVigilBeyondTime>().AddEntry(base.Buff, TeleportAbility);
	}

	protected override void OnDeactivate()
	{
		base.Context.MaybeCaster?.GetOrCreate<UnitPartVigilBeyondTime>().RemoveEntry(base.Buff);
	}
}
