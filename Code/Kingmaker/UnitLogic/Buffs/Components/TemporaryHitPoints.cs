using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Buffs.Components;

[Serializable]
[Obsolete]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("30bafd907bef48d0890a6e4a9265aa19")]
public class TemporaryHitPoints : UnitBuffComponentDelegate
{
	public ContextValue Value = new ContextValue();

	protected override void OnActivate()
	{
		PartHealth healthOptional = base.Owner.GetHealthOptional();
		if (healthOptional == null)
		{
			base.Buff.MarkExpired();
		}
		else
		{
			healthOptional.AddTemporaryHitPoints(Value.Calculate(base.Context), base.Buff);
		}
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetHealthOptional()?.RemoveTemporaryHitPoints(base.Buff);
	}
}
