using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("0b32d441ae614378928e5bab8eec92e8")]
public class PriorityTarget : UnitBuffComponentDelegate
{
	protected override void OnActivate()
	{
		(base.Context?.MaybeCaster)?.GetOrCreate<UnitPartPriorityTarget>().AddTarget(base.Buff);
	}

	protected override void OnDeactivate()
	{
		(base.Context?.MaybeCaster)?.GetOptional<UnitPartPriorityTarget>()?.RemoveTarget(base.Buff);
	}
}
