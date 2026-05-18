using System;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework;
using Kingmaker.Framework.Abilities;
using Kingmaker.Framework.ContextContract;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Serializable]
[TypeId("5d13a597de91e4746b804f8233518523")]
[ReadsContext(new ContextField[]
{
	ContextField.Caster,
	ContextField.Target
})]
[SetsContext(ContextField.Target, Availability.Definitely)]
public class ContextActionApplyBuff : ContextAction, IAbilityPrediction
{
	[SerializeField]
	private BlueprintBuffReference m_Buff;

	public BuffEndCondition BuffEndCondition = BuffEndCondition.CombatEnd;

	public BuffExpireMoment BuffExpireMoment;

	public bool Permanent;

	[ShowIf("IsCustomDuration")]
	public ContextDurationValue DurationValue;

	public bool ToCaster;

	public bool AsChild;

	[HideIf("Permanent")]
	public bool SameDuration;

	[Tooltip("Change only if you want Ranks to be more than 1")]
	public ContextValue Ranks;

	public bool DoNotApplyZeroRanks;

	public ActionList ActionsOnApply;

	public ActionList ActionsOnImmune;

	[Tooltip("If action runs in AbilityExecutionContext - add ability fact as source")]
	public bool AddFactSource;

	private bool IsCustomDuration
	{
		get
		{
			if (!Permanent)
			{
				return !SameDuration;
			}
			return false;
		}
	}

	public BlueprintBuff Buff => m_Buff?.Get();

	public void CollectPrediction(AbilityPredictionContext context)
	{
		MechanicEntity buffTarget = GetBuffTarget(base.Context);
		if (buffTarget != null && Buff != null)
		{
			int num = ((!Buff.HasRanks) ? 1 : Ranks.Calculate(base.Context));
			if (!DoNotApplyZeroRanks || num > 0)
			{
				num = Math.Max(1, num);
				context.RecordBuff(duration: new BuffDuration(CalculateDuration(), BuffEndCondition, BuffExpireMoment), target: buffTarget, blueprint: Buff, rank: num, parentContext: base.Context);
			}
		}
	}

	public override string GetCaption()
	{
		string text = "Apply" + (AsChild ? " child" : "") + " Buff" + (ToCaster ? " to caster" : "") + ": " + (Buff.NameSafe() ?? "???");
		if (Permanent)
		{
			return text + " (permanent)";
		}
		string text2 = (SameDuration ? "same duration" : DurationValue.ToString());
		return text + " (for " + text2 + ")";
	}

	protected override void RunAction()
	{
		MechanicEntity buffTarget = GetBuffTarget(base.Context);
		if (buffTarget == null)
		{
			Element.LogError(this, "Can't apply buff: target is null");
			return;
		}
		int num = ((!Buff.HasRanks) ? 1 : Ranks.Calculate(base.Context));
		if (DoNotApplyZeroRanks && num <= 0)
		{
			return;
		}
		num = Math.Max(1, num);
		BuffDuration duration = new BuffDuration(CalculateDuration(), BuffEndCondition, BuffExpireMoment);
		Buff buff = buffTarget.Buffs.Add(Buff, base.Context, duration, num);
		if (buff == null)
		{
			using (base.Context.PushTarget(buffTarget))
			{
				ActionsOnImmune?.Run();
				return;
			}
		}
		if (buff.FirstSource == null && !TryAddAbilitySource(buff))
		{
			AreaEffectEntity areaEffect = base.Context.AreaEffect;
			if (areaEffect != null)
			{
				buff.AddSource(areaEffect);
			}
			else
			{
				buff.AddSource(base.Context.Blueprint);
			}
		}
		if (AsChild)
		{
			(SimpleContextData<MechanicEntityFact, Kingmaker.UnitLogic.Buffs.Buff.Scope.Parent>.Current ?? base.Context.Fact)?.AddChildFact(buff);
		}
		using (base.Context.PushTarget(buffTarget))
		{
			ActionsOnApply?.Run();
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
		AbilityExecutionContext abilityContext = base.AbilityContext;
		if (abilityContext == null)
		{
			return false;
		}
		Ability fact = abilityContext.Ability.Fact;
		if (fact == null)
		{
			return false;
		}
		buff.AddSource(fact);
		return true;
	}

	public MechanicEntity GetBuffTarget(IEvalContext context)
	{
		if (!ToCaster)
		{
			return base.Target.Entity;
		}
		return context.Caster;
	}

	private Rounds? CalculateDuration()
	{
		if (Permanent)
		{
			return null;
		}
		if (SameDuration)
		{
			Buff buff = base.Context.Fact as Buff;
			if (buff == null || !buff.IsPermanent)
			{
				return buff?.ExpirationInRounds.Rounds();
			}
			return null;
		}
		return DurationValue.Calculate(base.Context);
	}
}
