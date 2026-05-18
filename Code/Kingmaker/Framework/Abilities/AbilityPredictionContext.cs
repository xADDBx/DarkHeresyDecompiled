using System;
using System.Collections.Generic;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Predictions;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Framework.Abilities;

public class AbilityPredictionContext : IDisposable
{
	public class TargetPredictionData
	{
		public DamagePredictionData Damage = new DamagePredictionData();

		public HealPredictionData Heal = new HealPredictionData();

		public List<(BlueprintBuff buff, int applyChance)> Buffs = new List<(BlueprintBuff, int)>();

		public int MoraleMinDelta;

		public int MoraleMaxDelta;

		public UIHitChancePredictionData HitChance;
	}

	private readonly AbilityExecutionContext _executionContext;

	private readonly Stack<MechanicEntity> _targetStack = new Stack<MechanicEntity>();

	private readonly Stack<float> _probabilityStack = new Stack<float>();

	private readonly List<AbilityDeliveryTarget> _deliveryTargets = new List<AbilityDeliveryTarget>();

	private readonly Dictionary<MechanicEntity, TargetPredictionData> _perTargetData = new Dictionary<MechanicEntity, TargetPredictionData>();

	public AbilityPredictionState State { get; }

	public MechanicEntity CurrentTarget => _targetStack.Peek();

	public float Probability => _probabilityStack.Peek();

	public AbilityExecutionContext ExecutionContext => _executionContext;

	public ReadonlyList<AbilityDeliveryTarget> DeliveryTargets => _deliveryTargets;

	public AbilityPredictionContext(AbilityData ability, TargetWrapper target)
	{
		State = new AbilityPredictionState();
		try
		{
			MechanicEntity predictionEntity = State.GetPredictionEntity(ability.Caster);
			AbilityData ability2 = CopyAbilityForPrediction(ability, predictionEntity);
			TargetWrapper clickedTarget = ((target.Entity != null) ? new TargetWrapper(State.GetPredictionEntity(target.Entity)) : target);
			_executionContext = AbilityExecutionContext.Claim(ability2, clickedTarget, predictionEntity.Position);
			_probabilityStack.Push(1f);
		}
		catch
		{
			_executionContext?.Dispose();
			State.Dispose();
			throw;
		}
	}

	private static AbilityData CopyAbilityForPrediction(AbilityData ability, MechanicEntity casterCopy)
	{
		if (ability.Fact != null)
		{
			Ability ability2 = casterCopy.Facts.Get<Ability>(ability.Fact.Blueprint);
			if (ability2 != null)
			{
				return new AbilityData(ability2, casterCopy, ability.IndexInItemSettings, ability.Modifiers);
			}
		}
		return new AbilityData(ability.Blueprint.OriginalBlueprint, casterCopy, ability.IndexInItemSettings, ability.Modifiers);
	}

	public void Dispose()
	{
		_executionContext.Dispose();
		State.Dispose();
	}

	public void ProcessActionList(ActionList actionList)
	{
		if (actionList?.Actions == null)
		{
			return;
		}
		GameAction[] actions = actionList.Actions;
		for (int i = 0; i < actions.Length; i++)
		{
			if (actions[i] is IAbilityPrediction abilityPrediction)
			{
				abilityPrediction.CollectPrediction(this);
			}
		}
	}

	public void WithTarget(MechanicEntity targetCopy, Action body)
	{
		_targetStack.Push(targetCopy);
		using (EvalContext.PushContext(_executionContext, targetCopy))
		{
			body();
		}
		_targetStack.Pop();
	}

	public void WithProbability(float branchProbability, Action body)
	{
		_probabilityStack.Push(Probability * branchProbability);
		body();
		_probabilityStack.Pop();
	}

	public void AddDeliveryTarget(AbilityDeliveryTarget target)
	{
		_deliveryTargets.Add(target);
	}

	public void RecordDamage(DamagePredictionData damage)
	{
		MechanicEntity originalEntity = State.GetOriginalEntity(CurrentTarget);
		GetOrCreateTargetData(originalEntity).Damage += damage;
	}

	public void RecordBuff(MechanicEntity target, BlueprintBuff blueprint, BuffDuration duration, int rank, IEvalContext? parentContext)
	{
		if (target.Buffs.Add(blueprint, parentContext, duration, rank) != null)
		{
			MechanicEntity originalEntity = State.GetOriginalEntity(target);
			TargetPredictionData orCreateTargetData = GetOrCreateTargetData(originalEntity);
			int item = (int)(Probability * 100f);
			orCreateTargetData.Buffs.Add((blueprint, item));
		}
	}

	public void RecordHeal(HealPredictionData heal)
	{
		MechanicEntity originalEntity = State.GetOriginalEntity(CurrentTarget);
		GetOrCreateTargetData(originalEntity).Heal += heal;
	}

	public void RecordMorale(int minDelta, int maxDelta)
	{
		MechanicEntity originalEntity = State.GetOriginalEntity(CurrentTarget);
		TargetPredictionData orCreateTargetData = GetOrCreateTargetData(originalEntity);
		orCreateTargetData.MoraleMinDelta += minDelta;
		orCreateTargetData.MoraleMaxDelta += maxDelta;
	}

	public void RecordHitChance(UIHitChancePredictionData hitChance)
	{
		MechanicEntity originalEntity = State.GetOriginalEntity(CurrentTarget);
		GetOrCreateTargetData(originalEntity).HitChance = hitChance;
	}

	public AbilityPredictionResult BuildResult()
	{
		return new AbilityPredictionResult(_perTargetData);
	}

	private TargetPredictionData GetOrCreateTargetData(MechanicEntity original)
	{
		if (!_perTargetData.TryGetValue(original, out TargetPredictionData value))
		{
			value = new TargetPredictionData();
			_perTargetData[original] = value;
		}
		return value;
	}
}
