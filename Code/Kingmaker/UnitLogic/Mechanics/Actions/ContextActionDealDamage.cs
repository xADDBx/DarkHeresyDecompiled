using System;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Gameplay.Utility;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.StatefulRandom;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Serializable]
[ClassInfoBox("Не наносит криты!! Для нанесения критов используй ContextActionCriticalEffectsAdd")]
[TypeId("bdc93cbacbdc05843a933659f15c1302")]
public class ContextActionDealDamage : ContextAction
{
	public struct DamageInfo
	{
		public int Min;

		public int Max;

		public int? PreRolledValue;
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

	protected override void RunAction()
	{
		if (base.Target.Entity == null)
		{
			Element.LogError(this, "Invalid target for effect '{0}'", GetType().Name);
			return;
		}
		DamageInfo damageInfo = GetDamageInfo(base.Context);
		int value = DealHitPointsDamage(damageInfo);
		if (WriteResultToContext)
		{
			base.Context[ResultPropertyName] = value;
		}
	}

	private DamageInfo GetDamageInfo(MechanicsContext context)
	{
		int min = (UseDiceForDamage ? (Value.DiceCountValue.Calculate(context) + Value.BonusValue.Calculate(context)) : (MinDamage.Calculate(context) + BonusDamage.Calculate(context)));
		int max = (UseDiceForDamage ? (Value.DiceCountValue.Calculate(context) * Value.DiceType.Sides() + Value.BonusValue.Calculate(context)) : (MaxDamage.Calculate(context) + BonusDamage.Calculate(context)));
		DamageInfo result = default(DamageInfo);
		result.Min = min;
		result.Max = max;
		result.PreRolledValue = (ReadPreRolledFromContext ? new int?(context[PreRolledValueName]) : null);
		return result;
	}

	private int DealHitPointsDamage(DamageInfo info)
	{
		MechanicEntity maybeCaster = base.Context.MaybeCaster;
		if (maybeCaster == null)
		{
			Element.LogError(this, "Caster is missing");
			return 0;
		}
		MechanicEntity entity = base.Target.Entity;
		if (entity == null)
		{
			Element.LogError(this, "Target is missing");
			return 0;
		}
		int min = info.Min;
		int max = info.Max;
		IntermediateDamage intermediateDamage = DamageType.CreateDamage(min, max);
		intermediateDamage.PushApplyStrategy(ApplyStrategy);
		if (Avoidable)
		{
			intermediateDamage.Avoidable.Add();
		}
		BlueprintBodyPart bodyPart = null;
		if (TargetBodyPart)
		{
			bodyPart = BodyPartSelector.GetBodyParts(entity).FirstOrDefault();
		}
		IntermediateDamage resultDamage = Rulebook.Trigger(new RuleCalculateDamage(maybeCaster, entity, base.AbilityContext?.Ability, null, intermediateDamage, bodyPart)).ResultDamage;
		resultDamage.CalculatedValue = info.PreRolledValue;
		RuleDealDamage ruleDealDamage = new RuleDealDamage(maybeCaster, entity, resultDamage)
		{
			Projectile = base.Projectile,
			SourceAbility = (base.AbilityContext?.Ability ?? base.Context.SourceAbility)
		};
		base.Context.TriggerRule(ruleDealDamage);
		return ruleDealDamage.ResultValue;
	}

	public DamagePredictionData GetDamagePrediction([NotNull] AbilityExecutionContext context, [CanBeNull] MechanicEntity target)
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			AbilityData ability = context.Ability;
			DamageInfo damageInfo = GetDamageInfo(context);
			int min = damageInfo.Min;
			int max = damageInfo.Max;
			IntermediateDamage baseDamageOverride = DamageType.CreateDamage(min, max);
			Penetration?.Calculate(context);
			baseDamageOverride = Rulebook.Trigger(new RuleCalculateDamage(ability.Caster, target, ability, null, baseDamageOverride)).ResultDamage;
			return new DamagePredictionData
			{
				MinDamage = baseDamageOverride.MinValue,
				MaxDamage = baseDamageOverride.MaxValue
			};
		}
	}

	public void Convert()
	{
	}
}
