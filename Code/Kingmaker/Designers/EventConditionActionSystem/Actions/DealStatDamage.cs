using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.RuleSystem;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[TypeId("9060d87f510e38649989a9d350884058")]
public class DealStatDamage : GameAction
{
	public bool NoSource;

	[HideIf("NoSource")]
	[SerializeReference]
	public AbstractUnitEvaluator Source;

	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Target;

	public StatType Stat;

	public bool IsDrain;

	public DiceFormula DamageDice;

	public int DamageBonus;

	public bool DisableBattleLog;

	public override string GetDescription()
	{
		string text = (IsDrain ? "Drain stat" : "Deal damage to");
		return $"{text} {Stat} of {Target} with source {(NoSource ? Target : Source)}\n" + (DisableBattleLog ? "Log disabled" : "Log enabled") + "\n";
	}

	protected override void RunAction()
	{
	}

	public override string GetCaption()
	{
		if (!IsDrain)
		{
			return "Deal stat Damage";
		}
		return "Drain stat";
	}

	public void Validate(ValidationContext context, int parentIndex)
	{
		if (!NoSource && !Source)
		{
			context.AddError("source is missing");
		}
		if (!Stat.IsAttribute())
		{
			context.AddError("attribute is missing");
		}
	}
}
