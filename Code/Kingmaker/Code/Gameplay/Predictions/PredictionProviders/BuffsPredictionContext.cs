using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.Gameplay.Components;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;

namespace Kingmaker.Code.Gameplay.Predictions.PredictionProviders;

public sealed record BuffsPredictionContext : IDisposable
{
	public AbilityData Ability { get; }

	public MechanicEntity Target { get; }

	public List<(BlueprintBuff, int)> PredictedBuffs { get; private set; }

	public AbilityExecutionContext AbilityExecutionContext => m_AbilityExecutionContext ?? (m_AbilityExecutionContext = Ability.ClaimExecutionContext(Target));

	private static HashSet<BlueprintBuff> m_SpecificImmunities;

	private static List<BuffImmunity> m_BuffRestrictions;

	private AbilityExecutionContext m_AbilityExecutionContext;

	private PredictionHackContext m_HackContext;

	public BuffsPredictionContext(UIPredictionContext uiCtx)
	{
		Ability = uiCtx.Ability;
		Target = uiCtx.Target;
		CollectImmunities();
	}

	void IDisposable.Dispose()
	{
		m_AbilityExecutionContext?.Dispose();
		m_HackContext?.Dispose();
		m_HackContext = null;
	}

	public bool IsTargetImmuneToBuff(BlueprintBuff buff)
	{
		if (m_SpecificImmunities != null && m_SpecificImmunities.Contains(buff))
		{
			return true;
		}
		if (m_BuffRestrictions != null)
		{
			IEvalContext current = EvalContext.Current;
			foreach (BuffImmunity buffRestriction in m_BuffRestrictions)
			{
				if (buffRestriction.IsImmune(current))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void AddBuffWithChance(BlueprintBuff buff, int chance)
	{
		if (PredictedBuffs == null)
		{
			List<(BlueprintBuff, int)> list2 = (PredictedBuffs = new List<(BlueprintBuff, int)>());
		}
		PredictedBuffs.Add((buff, chance));
	}

	public void AddGuaranteedBuff(BlueprintBuff buff)
	{
		if (buff.Stacking == StackingType.Rank)
		{
			if (m_HackContext == null)
			{
				m_HackContext = ContextData<PredictionHackContext>.Request();
			}
			m_HackContext.FactRanksToIncrement.Add(buff);
		}
		foreach (AddStatModifier component in buff.GetComponents<AddStatModifier>())
		{
			AddStatModifier(component, buff);
		}
	}

	private void CollectImmunities()
	{
		m_SpecificImmunities?.Clear();
		m_BuffRestrictions?.Clear();
		foreach (EntityFact item2 in Target.Facts.List)
		{
			BlueprintComponent[] componentsArray = item2.Blueprint.ComponentsArray;
			foreach (BlueprintComponent blueprintComponent in componentsArray)
			{
				if (!(blueprintComponent is SpecificBuffImmunity specificBuffImmunity))
				{
					if (blueprintComponent is BuffImmunity item)
					{
						if (m_BuffRestrictions == null)
						{
							m_BuffRestrictions = new List<BuffImmunity>();
						}
						m_BuffRestrictions.Add(item);
					}
				}
				else
				{
					if (m_SpecificImmunities == null)
					{
						m_SpecificImmunities = new HashSet<BlueprintBuff>();
					}
					m_SpecificImmunities.Add(specificBuffImmunity.Buff);
				}
			}
		}
	}

	private void AddStatModifier(AddStatModifier statModifier, BlueprintBuff sourceBuff)
	{
		EvalContext frame;
		using (EvalContext.Build().Blueprint(sourceBuff).Caster(Ability.Caster)
			.Owner(Target)
			.Push(out frame))
		{
			int statValue = statModifier.Value.Calculate(frame);
			ModifierType modifierType = statModifier.Value.ModifierType;
			if (statModifier.StatSelector == AddStatModifierRestrictionCalculator.StatSelectorType.Single)
			{
				AddModifier(statModifier.Stat, statValue, modifierType, sourceBuff);
				return;
			}
			StatType[] array = ((statModifier.StatSelector == AddStatModifierRestrictionCalculator.StatSelectorType.AllAttributes) ? StatTypeHelper.Attributes : StatTypeHelper.Skills);
			foreach (StatType statType in array)
			{
				AddModifier(statType, statValue, modifierType, sourceBuff);
			}
		}
	}

	private void AddModifier(StatType statType, int statValue, ModifierType modifierType, BlueprintBuff sourceBuff)
	{
		DecreaseByExistingStacks(statType, ref statValue, modifierType, sourceBuff);
		GetModifiersManager(statType).Add(modifierType, statValue);
	}

	private void DecreaseByExistingStacks(StatType statType, ref int statValue, ModifierType modifierType, BlueprintBuff sourceBuff)
	{
		StatQueryOutput statQueryOutput = new StatQueryOutput();
		Target.Actor.GetStat(statType, statQueryOutput, default(StatContext), "DecreaseByExistingStacks");
		foreach (Modifier modifier in statQueryOutput.Modifiers)
		{
			if (modifier.Fact?.Blueprint == sourceBuff && modifier.Type == modifierType)
			{
				statValue -= modifier.Value;
			}
		}
	}

	private CompositeModifiersManager GetModifiersManager(StatType statType)
	{
		if (m_HackContext == null)
		{
			m_HackContext = ContextData<PredictionHackContext>.Request();
		}
		return m_HackContext.GetModifiersManager(statType);
	}

	[CompilerGenerated]
	private bool PrintMembers(StringBuilder builder)
	{
		RuntimeHelpers.EnsureSufficientExecutionStack();
		builder.Append("Ability = ");
		builder.Append(Ability);
		builder.Append(", Target = ");
		builder.Append(Target);
		builder.Append(", PredictedBuffs = ");
		builder.Append(PredictedBuffs);
		builder.Append(", AbilityExecutionContext = ");
		builder.Append(AbilityExecutionContext);
		return true;
	}

	[CompilerGenerated]
	public override int GetHashCode()
	{
		return ((((EqualityComparer<Type>.Default.GetHashCode(EqualityContract) * -1521134295 + EqualityComparer<AbilityExecutionContext>.Default.GetHashCode(m_AbilityExecutionContext)) * -1521134295 + EqualityComparer<PredictionHackContext>.Default.GetHashCode(m_HackContext)) * -1521134295 + EqualityComparer<AbilityData>.Default.GetHashCode(Ability)) * -1521134295 + EqualityComparer<MechanicEntity>.Default.GetHashCode(Target)) * -1521134295 + EqualityComparer<List<(BlueprintBuff, int)>>.Default.GetHashCode(PredictedBuffs);
	}

	[CompilerGenerated]
	public bool Equals(BuffsPredictionContext? other)
	{
		if ((object)this != other)
		{
			if ((object)other != null && EqualityContract == other.EqualityContract && EqualityComparer<AbilityExecutionContext>.Default.Equals(m_AbilityExecutionContext, other.m_AbilityExecutionContext) && EqualityComparer<PredictionHackContext>.Default.Equals(m_HackContext, other.m_HackContext) && EqualityComparer<AbilityData>.Default.Equals(Ability, other.Ability) && EqualityComparer<MechanicEntity>.Default.Equals(Target, other.Target))
			{
				return EqualityComparer<List<(BlueprintBuff, int)>>.Default.Equals(PredictedBuffs, other.PredictedBuffs);
			}
			return false;
		}
		return true;
	}

	[CompilerGenerated]
	private BuffsPredictionContext(BuffsPredictionContext original)
	{
		m_AbilityExecutionContext = original.m_AbilityExecutionContext;
		m_HackContext = original.m_HackContext;
		Ability = original.Ability;
		Target = original.Target;
		PredictedBuffs = original.PredictedBuffs;
	}
}
