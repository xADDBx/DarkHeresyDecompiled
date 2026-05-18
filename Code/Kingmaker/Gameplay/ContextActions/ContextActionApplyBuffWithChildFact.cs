using System;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Gameplay.ContextActions;

[Serializable]
[TypeId("7a2964ddc060b2b4194299f4a41b65ad")]
public class ContextActionApplyBuffWithChildFact : ContextAction
{
	[SerializeField]
	[ValidateNotNull]
	private BpRef<BlueprintBuff> _casterBuff;

	[SerializeField]
	[ValidateNotNull]
	private BpRef<BlueprintMechanicEntityFact> _targetFact;

	public Rounds Rounds;

	public BuffEndCondition BuffEndCondition = BuffEndCondition.CombatEnd;

	public BuffExpireMoment BuffExpireMoment;

	public BlueprintBuff CasterBuff => _casterBuff;

	public BlueprintMechanicEntityFact TargetFact => _targetFact;

	public override string GetCaption()
	{
		return "Apply buff to caster: " + (CasterBuff.NameSafe() ?? "???") + " + child fact on target: " + (TargetFact.NameSafe() ?? "???");
	}

	protected override void RunAction()
	{
		MechanicEntity caster = base.Caster;
		MechanicEntity entity = base.Target.Entity;
		if (entity == null)
		{
			Element.LogError(this, "ContextActionApplyBuffWithChildFact: caster or target is null");
			return;
		}
		BlueprintBuff casterBuff = CasterBuff;
		BlueprintMechanicEntityFact targetFact = TargetFact;
		if (casterBuff == null || targetFact == null)
		{
			Element.LogError(this, "ContextActionApplyBuffWithChildFact: CasterBuff or TargetFact blueprint is null");
			return;
		}
		BuffDuration duration = new BuffDuration(Rounds, BuffEndCondition, BuffExpireMoment);
		Buff buff = caster.Buffs.Add(casterBuff, base.Context, duration);
		if (buff != null)
		{
			MechanicEntityFact mechanicEntityFact = entity.Facts.Add(targetFact.CreateFact(base.Context, default(BuffDuration)));
			if (mechanicEntityFact == null)
			{
				buff.Remove();
			}
			else
			{
				buff.AddChildFact(mechanicEntityFact);
			}
		}
	}
}
