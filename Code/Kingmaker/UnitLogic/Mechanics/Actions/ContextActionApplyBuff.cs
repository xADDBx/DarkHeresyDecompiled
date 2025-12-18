using System;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
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
public class ContextActionApplyBuff : ContextAction
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
		MechanicsContext context = base.Context;
		MechanicEntity buffTarget = GetBuffTarget(context);
		if (buffTarget == null)
		{
			Element.LogError(this, "Can't apply buff: target is null");
			return;
		}
		int num = ((!Buff.HasRanks) ? 1 : Ranks.Calculate(context));
		if (DoNotApplyZeroRanks && num <= 0)
		{
			return;
		}
		num = Math.Max(1, num);
		BuffDuration duration = new BuffDuration(CalculateDuration(context), BuffEndCondition, BuffExpireMoment);
		Buff buff = buffTarget.Buffs.Add(Buff, context, duration, num);
		if (buff == null)
		{
			using (base.Context.SetScope(buffTarget.ToITargetWrapper()))
			{
				ActionsOnImmune?.Run();
				return;
			}
		}
		if (buff.FirstSource == null && !TryAddAbilitySource(buff))
		{
			AreaEffectEntity current = SimpleContextData<AreaEffectEntity, MechanicsContext.Scope.AreaEffect>.Current;
			if (current != null)
			{
				buff.AddSource(current);
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
		using (base.Context.SetScope(buffTarget.ToITargetWrapper()))
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
		if (!(SimpleContextData<MechanicsContext, MechanicsContext.Scope>.Current is AbilityExecutionContext abilityExecutionContext))
		{
			return false;
		}
		Ability fact = abilityExecutionContext.Ability.Fact;
		if (fact == null)
		{
			return false;
		}
		buff.AddSource(fact);
		return true;
	}

	public MechanicEntity GetBuffTarget(MechanicsContext context)
	{
		if (!ToCaster)
		{
			return base.Target.Entity;
		}
		return context.MaybeCaster;
	}

	private Rounds? CalculateDuration(MechanicsContext context)
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
		return DurationValue.Calculate(context);
	}
}
