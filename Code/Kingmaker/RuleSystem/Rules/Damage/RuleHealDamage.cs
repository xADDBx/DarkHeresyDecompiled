using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.ContextContract;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.RuleSystem.Rules.Damage;

[RuleRoles(Initiator = "healer", Target = "healed unit")]
public class RuleHealDamage : RulebookTargetEvent
{
	public readonly struct Fluent
	{
		private readonly RuleHealDamage _src;

		private readonly RuleCalculateHeal.Fluent _calculateHealFluent;

		public Fluent(RuleHealDamage src)
		{
			_src = src;
			_calculateHealFluent = RuleCalculateHeal.Setup(_src.Initiator, _src.Target);
		}

		public FluentOptional WithMinMax(int min, int max)
		{
			return new FluentOptional(_src, _calculateHealFluent.WithMinMax(min, max));
		}

		public FluentOptional Base(int value)
		{
			return WithMinMax(0, 0).Base(value);
		}
	}

	public readonly struct FluentOptional
	{
		private readonly RuleHealDamage _src;

		private readonly RuleCalculateHeal.FluentOptional _calculateHealFluentOptional;

		public FluentOptional(RuleHealDamage src, RuleCalculateHeal.FluentOptional calculateHealFluentOptional)
		{
			_src = src;
			_calculateHealFluentOptional = calculateHealFluentOptional;
		}

		public FluentOptional Ability(AbilityData ability)
		{
			_src.Ability = ability;
			return this;
		}

		public FluentOptional Base(int value)
		{
			_calculateHealFluentOptional.Base(value);
			return this;
		}

		public FluentOptional Strategy(DamageStrategy strategy)
		{
			_calculateHealFluentOptional.Strategy(strategy);
			return this;
		}

		public RuleHealDamage Create()
		{
			_src.CalculateHealRule = _calculateHealFluentOptional.Create();
			return _src;
		}
	}

	public RuleCalculateHeal CalculateHealRule { get; private set; }

	[CanBeNull]
	public IDamageablePart TargetDamageablePart => CalculateHealRule.TargetDamageablePart;

	public int Value => CalculateHealRule.ResultValue;

	public AbilityData Ability { get; set; }

	public override AbilityData MaybeAbility => Ability ?? base.MaybeAbility;

	private RuleHealDamage([NotNull] MechanicEntity initiator, [NotNull] MechanicEntity target)
		: base(initiator, target)
	{
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		if (TargetDamageablePart != null)
		{
			Rulebook.Trigger(CalculateHealRule);
			TargetDamageablePart.HealDamage(Value);
			EventBus.RaiseEvent(delegate(IHealingHandler h)
			{
				h.HandleHealing(this);
			});
		}
	}

	public static Fluent Setup([NotNull] MechanicEntity initiator, [NotNull] MechanicEntity target)
	{
		return new Fluent(new RuleHealDamage(initiator, target));
	}
}
