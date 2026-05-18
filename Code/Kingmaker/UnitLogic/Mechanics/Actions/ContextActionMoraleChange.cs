using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Serializable]
[TypeId("00eb56da272d4951a64f9aa38affe31a")]
public sealed class ContextActionMoraleChange : ContextAction
{
	[SerializeField]
	private ContextValue m_Amount;

	public override string GetCaption()
	{
		return $"Change morale of target by {m_Amount}";
	}

	protected override void RunAction()
	{
		if (!(base.Target.Entity is BaseUnitEntity target))
		{
			PFLog.Morale.Error($"[Morale] ContextActionMoraleChange on {base.Owner} cant run on not BaseUnit");
			return;
		}
		int baseValue = m_Amount.Calculate(base.Context);
		Rulebook.Trigger(new RulePerformMoraleChange(base.Caster, target, MoraleEventType.ForcedChangeMorale, baseValue)
		{
			SourceContext = base.Context
		});
	}

	public MoralePredictionData GetMoralePrediction([NotNull] IEvalContext context)
	{
		MoralePredictionData result = default(MoralePredictionData);
		result.MoraleDelta = m_Amount.Calculate(context);
		return result;
	}
}
