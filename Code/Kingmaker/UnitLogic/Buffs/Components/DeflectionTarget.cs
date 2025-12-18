using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Buffs.Components;

[Obsolete]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("7173e6a0fca449aebdcbedbdc2afcde1")]
public class DeflectionTarget : UnitBuffComponentDelegate
{
	public MechanicEntity Caster { get; private set; }

	protected override void OnActivateOrPostLoad()
	{
		Caster = base.Fact.MaybeContext?.MaybeCaster;
	}
}
