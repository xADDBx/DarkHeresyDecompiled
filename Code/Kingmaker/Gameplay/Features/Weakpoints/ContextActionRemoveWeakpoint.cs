using System;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Features.Weakpoints;

[Serializable]
[TypeId("1de6bcac80c04887aeea6d13a80a6a82")]
public sealed class ContextActionRemoveWeakpoint : ContextAction
{
	public WeakpointSideSelector Side;

	public override string GetCaption()
	{
		return $"Remove weakpoint from {Side}";
	}

	protected override void RunAction()
	{
		base.Target.Entity?.GetOptional<PartWeakpoints>()?.RemoveAll(Side.Select(base.Caster, base.Target.Entity));
	}
}
