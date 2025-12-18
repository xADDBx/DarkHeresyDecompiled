using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.Random;

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
			_src.m_UseMissWeight = state;
			return this;
		}

		public RuleCalculateTargetInBurst Create()
		{
			return _src;
		}
	}

	private List<MechanicEntity> m_SecondaryTargets = new List<MechanicEntity>();

	private bool m_UseMissWeight = true;

	public MechanicEntity ResultTarget;

	public Dictionary<MechanicEntity, int> UnitsWeights = new Dictionary<MechanicEntity, int>();

	public BurstWeightSettings OverridenBurstSettings;

	public int TotalWeight;

	private BurstWeightSettings BurstSettings => OverridenBurstSettings ?? BurstRoot.Instance.DefaultSettings;

	public static Fluent Setup([NotNull] IMechanicEntity initiator)
	{
		return new Fluent(new RuleCalculateTargetInBurst(initiator));
	}

	private RuleCalculateTargetInBurst([NotNull] IMechanicEntity initiator)
		: base(initiator)
	{
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		TotalWeight = (m_UseMissWeight ? BurstSettings.MissWeight : 0);
		Dictionary<int, MechanicEntity> dictionary = new Dictionary<int, MechanicEntity>();
		foreach (MechanicEntity secondaryTarget in m_SecondaryTargets)
		{
			int result = Rulebook.Trigger(new RuleCalculateWeightBurstTarget(base.ConcreteInitiator, secondaryTarget, BurstSettings)).Result;
			if (result != 0)
			{
				dictionary[TotalWeight + result] = secondaryTarget;
				UnitsWeights[secondaryTarget] = result;
				TotalWeight += result;
			}
		}
		int num = PFStatefulRandom.UnitLogic.Abilities.Range(0, TotalWeight);
		if (m_UseMissWeight && num < BurstSettings.MissWeight)
		{
			return;
		}
		foreach (KeyValuePair<int, MechanicEntity> item in dictionary)
		{
			if (num < item.Key)
			{
				ResultTarget = item.Value;
				break;
			}
		}
	}
}
