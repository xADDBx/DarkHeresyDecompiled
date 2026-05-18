using System;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Obsolete]
[TypeId("5e2f2046f3bd6984c8833bf019c8e8ad")]
public class ContextActionSavingThrow : ContextAction
{
	[Serializable]
	private struct ConditionalDCIncrease
	{
		public ConditionsChecker Condition;

		public ContextValue Value;
	}

	public SavingThrowType Type;

	public bool FromBuff;

	[SerializeField]
	private ConditionalDCIncrease[] m_ConditionalDCIncrease = new ConditionalDCIncrease[0];

	public bool HasCustomDC;

	[ShowIf("HasCustomDC")]
	public ContextValue CustomDC;

	public ActionList Actions;

	public override string GetCaption()
	{
		return "Saving throw " + Type;
	}

	protected override void RunAction()
	{
	}

	[CanBeNull]
	public static ContextActionApplyBuff FindApplyBuffAction(ActionList actions)
	{
		GameAction[] actions2 = actions.Actions;
		foreach (GameAction gameAction in actions2)
		{
			ContextActionApplyBuff contextActionApplyBuff = ((gameAction is ContextActionConditionalSaved contextActionConditionalSaved) ? FindApplyBuffAction(contextActionConditionalSaved.Succeed) : (gameAction as ContextActionApplyBuff));
			if (contextActionApplyBuff != null)
			{
				return contextActionApplyBuff;
			}
		}
		return null;
	}

	private RulePerformSavingThrow CreateSavingThrow(MechanicEntity unit, int dc, bool persistentSpell)
	{
		return new RulePerformSavingThrow(unit, Type, dc)
		{
			Buff = ((!FromBuff) ? null : FindApplyBuffAction(Actions)?.Buff),
			PersistentSpell = persistentSpell
		};
	}
}
