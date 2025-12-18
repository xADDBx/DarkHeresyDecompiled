using System;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Obsolete]
[TypeId("2de0227f2d0b4e8994f31f8401c579aa")]
public class WarhammerContextActionApplyBuffToPriorityTarget : ContextAction
{
	[SerializeField]
	private BlueprintBuffReference m_Buff;

	public bool Permanent;

	public ContextDurationValue DurationValue;

	[SerializeField]
	private BlueprintBuffReference m_TargetBuff;

	public BlueprintBuff Buff => m_Buff?.Get();

	public BlueprintBuff TargetBuff => m_TargetBuff?.Get();

	protected override void RunAction()
	{
		MechanicsContext current = SimpleContextData<MechanicsContext, MechanicsContext.Scope>.Current;
		if (current == null)
		{
			Element.LogError(this, "Unable to apply buff: no context found");
			return;
		}
		BaseUnitEntity baseUnitEntity = current.MaybeCaster?.GetOptional<UnitPartPriorityTarget>()?.GetPriorityTarget(TargetBuff);
		if (baseUnitEntity == null)
		{
			Element.LogError(this, "Can't apply buff: target is null");
			return;
		}
		Rounds? rounds = (Permanent ? null : new Rounds?(DurationValue.Calculate(current)));
		baseUnitEntity.Buffs.Add(Buff, current, rounds);
	}

	public override string GetCaption()
	{
		string text = "Apply Buff to priority target: " + (Buff.NameSafe() ?? "???");
		if (Permanent)
		{
			return text + " (permanent)";
		}
		string text2 = DurationValue.ToString();
		return text + " (for " + text2 + ")";
	}
}
