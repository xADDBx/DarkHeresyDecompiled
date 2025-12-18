using System;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[TypeId("76f8f6b48eec283439f3eeb67fd7898e")]
public class ForceDismembermentOnDeath : UnitFactComponentDelegate
{
	public UnitDismemberType Dismember;

	protected override void OnActivate()
	{
		base.OnActivate();
		base.Fact.Owner.LifeState.ForceDismember = Dismember;
	}

	protected override void OnDeactivate()
	{
		base.OnDeactivate();
		base.Fact.Owner.LifeState.ForceDismember = UnitDismemberType.None;
	}
}
