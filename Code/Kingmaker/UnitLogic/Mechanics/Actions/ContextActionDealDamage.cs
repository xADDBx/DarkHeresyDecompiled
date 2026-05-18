using System;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Framework;
using Kingmaker.Framework.Abilities;
using Kingmaker.Framework.ContextContract;
using Kingmaker.Framework.Mechanics.Utility.Damage;
using Kingmaker.Gameplay.Utility;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.StatefulRandom;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Serializable]
[ClassInfoBox("Не наносит криты!! Для нанесения критов используй ContextActionCriticalEffectsAdd")]
[TypeId("bdc93cbacbdc05843a933659f15c1302")]
[ReadsContext(new ContextField[]
{
	ContextField.Caster,
	ContextField.Target,
	ContextField.Ability
})]
public class ContextActionDealDamage : ContextAction, IAbilityPrediction
{
	private readonly struct DealDamageInfo
	{
		public readonly MechanicEntity Caster;

		public readonly MechanicEntity Target;

		public readonly IntermediateDamage Damage;

		public DealDamageInfo(MechanicEntity caster, MechanicEntity target, IntermediateDamage damage)
		{
			Caster = caster;
			Target = target;
			Damage = damage;
		}
	}

	[Header("Damage")]
	public DamageTypeSettings DamageType;

	public bool TargetBodyPart;

	[ShowIf("TargetBodyPart")]
	public BodyPartsSelector BodyPartSelector;

	public bool UseDiceForDamage;

	public bool ReadPreRolledFromContext;

	[ShowIf("ReadPreRolledFromContext")]
	public ContextPropertyName PreRolledValueName;

	[ShowIf("ShowDiceDamageValue")]
	public ContextDiceValue Value;

	[ShowIf("ShowRangeDamageValue")]
	public ContextValue MinDamage;

	[ShowIf("ShowRangeDamageValue")]
	public ContextValue MaxDamage;

	[ShowIf("ShowRangeDamageValue")]
	public ContextValue BonusDamage;

	public ContextDiceValue Penetration;

	public bool Avoidable;

	public DamageStrategy ApplyStrategy;

	[Header("Misc")]
	public bool Half;

	public bool WriteResultToContext;

	[ShowIf("WriteResultToContext")]
	public ContextPropertyName ResultPropertyName;

	private bool ShowDiceDamageValue
	{
		get
		{
			if (!ReadPreRolledFromContext)
			{
				return UseDiceForDamage;
			}
			return false;
		}
	}

	private bool ShowRangeDamageValue
	{
		get
		{
			if (!ReadPreRolledFromContext)
			{
				return !UseDiceForDamage;
			}
			return false;
		}
	}

	public void CollectPrediction(AbilityPredictionContext context)
	{
		DamagePredictionData damagePrediction = GetDamagePrediction(base.Context, base.Target);
		context.RecordDamage(damagePrediction);
	}

	public override string GetCaption()
	{
		string arg = (UseDiceForDamage ? Value.ToString() : string.Format("[{0} to {1}{2}]", MinDamage, MaxDamage, BonusDamage.IsZero ? "" : $" (+{BonusDamage})"));
		string text = (Half ? $"Deal half {arg} of {DamageType}" : $"Deal {arg} of {DamageType}");
		if (WriteResultToContext)
		{
			text += $" >> {ResultPropertyName}";
		}
		return text;
	}

	public DamagePredictionData GetDamagePrediction([NotNull] IEvalContext actionContext, [CanBeNull] TargetWrapper target)
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			DealDamageInfo info;
			return TryGetDamageInfo(target, actionContext, out info) ? new DamagePredictionData
			{
				MinDamage = Roll(1),
				MaxDamage = Roll(100)
			} : null;
			int Roll(int roll)
			{
				return new RolledDamage(info.Caster, info.Target, info.Damage, roll).ResultDamageValue;
			}
		}
	}

	protected override void RunAction()
	{
		if (base.Target.Entity == null)
		{
			Element.LogError(this, "Invalid target for effect '{0}'", GetType().Name);
			return;
		}
		int value = DealHitPointsDamage();
		if (WriteResultToContext)
		{
			base.Context[ResultPropertyName] = value;
		}
	}

	private int DealHitPointsDamage()
	{
		if (!TryGetDamageInfo(base.Target, base.Context, out var info))
		{
			return 0;
		}
		return DealHitPointsDamage(info.Caster, info.Target, info.Damage);
	}

	private bool TryGetDamageInfo(TargetWrapper targetWrapper, IEvalContext context, out DealDamageInfo info)
	{
		MechanicEntity mechanicEntity = context?.Caster;
		if (mechanicEntity == null)
		{
			Element.LogError(this, "Caster is missing");
			info = default(DealDamageInfo);
			return false;
		}
		MechanicEntity entity = targetWrapper.Entity;
		if (entity == null)
		{
			Element.LogError(this, "Target is missing");
			info = default(DealDamageInfo);
			return false;
		}
		IntermediateDamage intermediateDamage = GetIntermediateDamage(mechanicEntity, entity, context);
		info = new DealDamageInfo(mechanicEntity, entity, intermediateDamage);
		return true;
	}

	private IntermediateDamage GetIntermediateDamage(MechanicEntity caster, MechanicEntity target, IEvalContext context)
	{
		int min = (UseDiceForDamage ? (Value.DiceCountValue.Calculate(context) + Value.BonusValue.Calculate(context)) : (MinDamage.Calculate(context) + BonusDamage.Calculate(context)));
		int max = (UseDiceForDamage ? (Value.DiceCountValue.Calculate(context) * Value.DiceType.Sides() + Value.BonusValue.Calculate(context)) : (MaxDamage.Calculate(context) + BonusDamage.Calculate(context)));
		IntermediateDamage intermediateDamage = DamageType.CreateDamage(min, max);
		intermediateDamage.PushApplyStrategy(ApplyStrategy);
		if (Avoidable)
		{
			intermediateDamage.Avoidable.Add();
		}
		BlueprintBodyPart bodyPart = (TargetBodyPart ? BodyPartSelector.GetBodyParts(target).FirstOrDefault() : null);
		RuleCalculateDamage ruleCalculateDamage = Rulebook.Trigger(new RuleCalculateDamage(caster, target, base.Context.Ability, null, intermediateDamage, bodyPart));
		ruleCalculateDamage.ResultDamage.CalculatedValue = (ReadPreRolledFromContext ? new int?(context[PreRolledValueName]) : null);
		return ruleCalculateDamage.ResultDamage;
	}

	private int DealHitPointsDamage(MechanicEntity caster, MechanicEntity target, IntermediateDamage damage)
	{
		if (damage == null)
		{
			return 0;
		}
		RuleDealDamage ruleDealDamage = new RuleDealDamage(caster, target, damage)
		{
			Projectile = base.Projectile,
			SourceAbility = (base.Context.Ability ?? base.Context.SourceAbility)
		};
		base.Context.TriggerRule(ruleDealDamage);
		return ruleDealDamage.ResultValue;
	}

	public void Convert()
	{
	}
}
