using System;
using System.Text;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Framework;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/AttachBuff")]
[PlayerUpgraderAllowed(false)]
[AllowMultipleComponents]
[TypeId("0c996f778c13abb408bdd05f7f6fe317")]
public class AttachBuff : GameAction
{
	[ValidateNotNull]
	[SerializeField]
	private BlueprintBuffReference m_Buff;

	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Target;

	[SerializeReference]
	public IntEvaluator Duration;

	public BuffEndCondition EndCondition = BuffEndCondition.CombatEnd;

	public BuffExpireMoment ExpireMoment;

	[Tooltip("If action runs in AbilityExecutionContext - add ability fact as source")]
	public bool AddFactSource;

	public BlueprintBuff Buff => m_Buff?.Get();

	public override string GetDescription()
	{
		string arg = (Duration ? Duration.ToString() : "бесконечно");
		return $"Навешивает на цель {Target} бафф {Buff} на время в раундах: {arg}";
	}

	protected override void RunAction()
	{
		if (Target == null)
		{
			throw new Exception("Trying to attach buff to null target");
		}
		Rounds? rounds = (Duration ? new Rounds?(Duration.GetValue().Rounds()) : null);
		BuffDuration duration = new BuffDuration(rounds, EndCondition, ExpireMoment);
		Buff buff = Target.GetValue().Buffs.Add(Buff, null, null, duration);
		AddSource(buff);
	}

	private void AddSource(Buff buff)
	{
		if (buff != null && !TryAddAbilitySource(buff))
		{
			buff?.TryAddSource(this);
		}
	}

	private bool TryAddAbilitySource(Buff buff)
	{
		if (!AddFactSource)
		{
			return false;
		}
		if (buff == null)
		{
			return false;
		}
		AbilityExecutionContext abilityExecution = EvalContext.Current.AbilityExecution;
		if (abilityExecution == null)
		{
			return false;
		}
		Ability fact = abilityExecution.Ability.Fact;
		if (fact == null)
		{
			return false;
		}
		buff.AddSource(fact);
		return true;
	}

	public override string GetCaption()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append($"Attach ({Buff}) to ({Target}) ");
		if (Duration == null)
		{
			return stringBuilder.ToString();
		}
		if (Duration.GetValue() == 0)
		{
			stringBuilder.Append("until ");
		}
		if (Duration.GetValue() == 0)
		{
			if (EndCondition == BuffEndCondition.RemainAfterCombat)
			{
				stringBuilder.Append("premanently");
			}
			if (EndCondition == BuffEndCondition.CombatEnd)
			{
				stringBuilder.Append("until combat end");
			}
		}
		else if (Duration.GetValue() == 1)
		{
			if (ExpireMoment == BuffExpireMoment.TurnStart)
			{
				stringBuilder.Append("until next TurnStart");
			}
			else if (ExpireMoment == BuffExpireMoment.TurnEnd)
			{
				stringBuilder.Append("until next TurnEnd");
			}
		}
		else if (ExpireMoment == BuffExpireMoment.TurnStart)
		{
			stringBuilder.Append($"for {Duration.GetValue() - 1} full rounds until TurnStart");
		}
		else if (ExpireMoment == BuffExpireMoment.TurnEnd)
		{
			stringBuilder.Append($"for {Duration.GetValue()} full rounds until TurnEnd");
		}
		return stringBuilder.ToString();
	}
}
