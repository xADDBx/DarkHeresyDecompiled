using System;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.EntitySystem.Entities;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Serializable]
[TypeId("79d6e29cce6c4e9eb2b9de8624e245ba")]
public sealed class ContextActionChangeMoralePhase : ContextAction
{
	public MoralePhaseType MoralePhaseType;

	public override string GetCaption()
	{
		return "Set morale of target to " + MoralePhaseType;
	}

	protected override void RunAction()
	{
		if (!(base.Target.Entity is BaseUnitEntity baseUnitEntity))
		{
			PFLog.Morale.Error($"[Morale] ContextActionChangeMoralePhase on {base.Owner} cant run on not BaseUnit");
		}
		else
		{
			baseUnitEntity.Morale.SwitchPhase(MoralePhaseType, base.Caster);
		}
	}
}
