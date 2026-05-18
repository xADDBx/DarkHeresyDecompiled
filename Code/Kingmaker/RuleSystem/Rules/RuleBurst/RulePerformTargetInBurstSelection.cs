using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.Random;

namespace Kingmaker.RuleSystem.Rules.RuleBurst;

public class RulePerformTargetInBurstSelection : RulebookEvent
{
	public readonly struct Fluent
	{
		private readonly RulePerformTargetInBurstSelection _src;

		private readonly RuleCalculateTargetInBurst.Fluent _builder;

		public Fluent(RulePerformTargetInBurstSelection src)
		{
			_src = src;
			_builder = RuleCalculateTargetInBurst.Setup(src.Initiator);
		}

		public FluentOptional WithTargets([NotNull] List<MechanicEntity> secondaryTargets)
		{
			return new FluentOptional(_src, _builder.WithTargets(secondaryTargets));
		}
	}

	public struct FluentOptional
	{
		private readonly RulePerformTargetInBurstSelection _src;

		private RuleCalculateTargetInBurst.FluentOptional _builder;

		public FluentOptional(RulePerformTargetInBurstSelection src, RuleCalculateTargetInBurst.FluentOptional builder)
		{
			_src = src;
			_builder = builder;
		}

		public FluentOptional UseMissWeight(bool state)
		{
			_builder = _builder.UseMissWeight(state);
			return this;
		}

		public RulePerformTargetInBurstSelection Create()
		{
			_src.RuleCalculate = _builder.Create();
			return _src;
		}
	}

	public RuleCalculateTargetInBurst RuleCalculate { get; private set; }

	public MechanicEntity ResultTarget { get; private set; }

	private RulePerformTargetInBurstSelection([NotNull] IMechanicEntity initiator)
		: base(initiator)
	{
	}

	public static Fluent Setup([NotNull] IMechanicEntity initiator)
	{
		return new Fluent(new RulePerformTargetInBurstSelection(initiator));
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		Rulebook.Trigger(RuleCalculate);
		int num = PFStatefulRandom.UnitLogic.Abilities.Range(0, RuleCalculate.TotalWeight);
		if (RuleCalculate.UseMissWeight && num < RuleCalculate.BurstSettings.MissWeight)
		{
			return;
		}
		foreach (var (num3, resultTarget) in RuleCalculate.WeightToTargetMap)
		{
			if (num < num3)
			{
				ResultTarget = resultTarget;
				break;
			}
		}
	}
}
