using JetBrains.Annotations;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility.StatefulRandom;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("8abf85d8c6ea04343a2e4fe6bb27a3bb")]
public class ContextActionHealTarget : ContextAction
{
	public DamageStrategy HealStrategy;

	public ContextValue MinHealing;

	public ContextValue MaxHealing;

	public ContextValue Bonus;

	public override string GetCaption()
	{
		if (MinHealing.IsZero && MaxHealing.IsZero)
		{
			return $"Heal {Bonus} of hit points damage";
		}
		if (Bonus.IsZero)
		{
			return $"Heal [{MinHealing}-{MaxHealing}] of hit points damage";
		}
		return $"Heal [{MinHealing}-{MaxHealing}]+{Bonus} of hit points damage";
	}

	protected override void RunAction()
	{
		if (base.Target.Entity == null)
		{
			Element.LogError(this, "Invalid target for effect '{0}'", GetType().Name);
		}
		else if (base.Context.MaybeCaster == null)
		{
			Element.LogError(this, "Caster is missing");
		}
		else
		{
			RuleHealDamage rule = RuleHealDamage.Setup(base.Context.MaybeCaster, base.Target.Entity).WithMinMax(MinHealing.Calculate(base.Context), MaxHealing.Calculate(base.Context)).Base(Bonus.Calculate(base.Context))
				.Strategy(HealStrategy)
				.Create();
			base.Context.TriggerRule(rule);
		}
	}

	public HealPredictionData GetHealPrediction([NotNull] AbilityExecutionContext context, [CanBeNull] MechanicEntity target)
	{
		if (context.MaybeCaster == null || target == null)
		{
			return new HealPredictionData();
		}
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			RuleCalculateHeal ruleCalculateHeal = RuleCalculateHeal.Setup(context.Caster, target).WithMinMax(MinHealing.Calculate(context), MaxHealing.Calculate(context)).Base(Bonus.Calculate(context))
				.Strategy(HealStrategy)
				.Create();
			Rulebook.Trigger(ruleCalculateHeal);
			return new HealPredictionData
			{
				Bonus = ruleCalculateHeal.Base,
				MinValue = ruleCalculateHeal.MinHealingModified,
				MaxValue = ruleCalculateHeal.MaxHealingModified,
				HealStrategy = HealStrategy
			};
		}
	}
}
