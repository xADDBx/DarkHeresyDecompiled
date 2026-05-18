using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.RuleSystem.Rules.RuleBurst;

public class RuleCalculateTargetInBurst : RulebookEvent
{
	public readonly struct Fluent
	{
		private readonly RuleCalculateTargetInBurst _src;

		public Fluent(RuleCalculateTargetInBurst src)
		{
			_src = src;
		}

		public FluentOptional WithTargets([NotNull] List<MechanicEntity> secondaryTargets)
		{
			_src.m_SecondaryTargets = secondaryTargets;
			return new FluentOptional(_src);
		}
	}

	public readonly struct FluentOptional
	{
		private readonly RuleCalculateTargetInBurst _src;

		public FluentOptional(RuleCalculateTargetInBurst src)
		{
			_src = src;
		}

		public FluentOptional UseMissWeight(bool state)
		{
			_src.UseMissWeight = state;
			return this;
		}

		public RuleCalculateTargetInBurst Create()
		{
			return _src;
		}
	}

	public BurstWeightSettings OverridenBurstSettings;

	private List<MechanicEntity> m_SecondaryTargets = new List<MechanicEntity>();

	public Dictionary<int, MechanicEntity> WeightToTargetMap { get; } = new Dictionary<int, MechanicEntity>();


	public Dictionary<MechanicEntity, int> TargetWeights { get; } = new Dictionary<MechanicEntity, int>();


	public bool UseMissWeight { get; private set; } = true;


	public int TotalWeight { get; private set; }

	public BurstWeightSettings BurstSettings => OverridenBurstSettings ?? BurstRoot.Instance.DefaultSettings;

	public RuleCalculateTargetInBurst([NotNull] IMechanicEntity initiator)
		: base(initiator)
	{
	}

	public static Fluent Setup([NotNull] IMechanicEntity initiator)
	{
		return new Fluent(new RuleCalculateTargetInBurst(initiator));
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		TotalWeight = (UseMissWeight ? BurstSettings.MissWeight : 0);
		foreach (MechanicEntity secondaryTarget in m_SecondaryTargets)
		{
			int result = Rulebook.Trigger(new RuleCalculateWeightBurstTarget(base.ConcreteInitiator, secondaryTarget, BurstSettings)).Result;
			if (result != 0)
			{
				WeightToTargetMap[TotalWeight + result] = secondaryTarget;
				TargetWeights[secondaryTarget] = result;
				TotalWeight += result;
			}
		}
	}
}
