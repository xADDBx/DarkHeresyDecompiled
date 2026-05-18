using System;
using JetBrains.Annotations;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Framework;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace Kingmaker.RuleSystem.Rules.Damage;

public class RuleCalculateHeal : RulebookTargetEvent
{
	public readonly struct Fluent
	{
		private readonly RuleCalculateHeal _src;

		public Fluent(RuleCalculateHeal src)
		{
			_src = src;
		}

		public FluentOptional WithMinMax(int min, int max)
		{
			_src.MinHealing = min;
			_src.MaxHealing = max;
			return new FluentOptional(_src);
		}
	}

	public readonly struct FluentOptional
	{
		private readonly RuleCalculateHeal _src;

		public FluentOptional(RuleCalculateHeal src)
		{
			_src = src;
		}

		public FluentOptional Base(int bonus)
		{
			_src.Base = bonus;
			return this;
		}

		public FluentOptional Strategy(DamageStrategy strategy)
		{
			_src.Strategy = strategy;
			return this;
		}

		public RuleCalculateHeal Create()
		{
			return _src;
		}
	}

	public readonly CompositeModifiersManager Modifiers = new CompositeModifiersManager(0);

	private DamageStrategy _strategy;

	[CanBeNull]
	public IEvalContext Context { get; }

	[CanBeNull]
	public IDamageablePart TargetDamageablePart { get; private set; }

	public int DiceResult { get; private set; }

	public int MinHealing { get; private set; }

	public int MaxHealing { get; private set; }

	public int MinHealingModified { get; private set; }

	public int MaxHealingModified { get; private set; }

	public int ValueWithoutReduction { get; private set; }

	public int Base { get; private set; }

	public int ResultValue { get; private set; }

	public DamageStrategy Strategy
	{
		get
		{
			return _strategy;
		}
		private set
		{
			_strategy = value;
			SetTargetDamageablePart();
		}
	}

	private RuleCalculateHeal([NotNull] MechanicEntity initiator, [NotNull] MechanicEntity target)
		: base(initiator, target)
	{
		Context = EvalContext.Current;
		SetTargetDamageablePart();
	}

	private void SetTargetDamageablePart()
	{
		IDamageablePart targetDamageablePart;
		if (Strategy != DamageStrategy.ArmorOnly)
		{
			IDamageablePart healthOptional = Target.GetHealthOptional();
			targetDamageablePart = healthOptional;
		}
		else
		{
			IDamageablePart healthOptional = Target.GetArmorOptional();
			targetDamageablePart = healthOptional;
		}
		TargetDamageablePart = targetDamageablePart;
	}

	public IDamageablePart GetTargetDamageablePart()
	{
		if (Strategy != DamageStrategy.ArmorOnly)
		{
			return Target.GetHealthOptional();
		}
		return Target.GetArmorOptional();
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		if (TargetDamageablePart != null)
		{
			PartHealth healthOptional = Target.GetHealthOptional();
			if (healthOptional != null && healthOptional.IsForbidDirectHpDamage && Strategy != DamageStrategy.ArmorOnly)
			{
				Modifiers.Add(ModifierType.PctMul_Extra, 0, this, ModifierDescriptor.Mechanism);
			}
			float num = Math.Clamp((float)(int)RulebookEvent.RollD100() / 100f, 0f, 1f);
			MinHealingModified = Modifiers.Apply(MinHealing + Base);
			MaxHealingModified = Modifiers.Apply(MaxHealing + Base);
			DiceResult = MinHealing + Mathf.RoundToInt((float)(MaxHealing - MinHealing) * num);
			ValueWithoutReduction = Math.Max(0, DiceResult + Base);
			ResultValue = Modifiers.Apply(Math.Min(ValueWithoutReduction, TargetDamageablePart.Damage));
		}
	}

	public static Fluent Setup([NotNull] MechanicEntity initiator, [NotNull] MechanicEntity target)
	{
		return new Fluent(new RuleCalculateHeal(initiator, target));
	}
}
