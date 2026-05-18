using JetBrains.Annotations;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.ContextContract;
using Kingmaker.Framework.Mechanics.Utility.Damage;
using Kingmaker.Gameplay.Features.Encounter;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.RuleSystem.Rules.Damage;

[RuleRoles(Initiator = "who dealt the damage", Target = "who took the damage")]
public class RuleDealDamage : RulebookTargetEvent, IDamageHolderRule
{
	[CanBeNull]
	public readonly PartHealth TargetHealth;

	[CanBeNull]
	public readonly PartArmor TargetArmor;

	[NotNull]
	public readonly RuleRollDamage RollDamageRule;

	private bool m_IsFake;

	public bool CancelDamage;

	public bool IsDot { get; private set; }

	public int HPBeforeDamage { get; private set; }

	public int DurabilityBeforeDamage { get; private set; }

	public int DurabilityAfterDamage { get; private set; }

	public int ResultValue => ResultDamage.ResultDamageValue;

	public RolledDamage ResultDamage => RollDamageRule.Result;

	public bool ResultIsCritical { get; private set; }

	public bool ResultArmorCrack { get; private set; }

	public bool ResultUnitDied { get; private set; }

	[CanBeNull]
	public Projectile Projectile { get; set; }

	public bool IsFake
	{
		get
		{
			if (!m_IsFake)
			{
				return CancelDamage;
			}
			return true;
		}
	}

	[CanBeNull]
	public AbilityData SourceAbility { get; set; }

	public bool DisableFxAndSound { get; set; }

	public bool FromRuleWarhammerAttackRoll { get; set; }

	public bool IsDispersedDamage { get; set; }

	public bool IsCollisionDamage { get; set; }

	DamageType IDamageHolderRule.DamageType => RollDamageRule.Damage.Type;

	public IntermediateDamage IntermediateDamage => RollDamageRule.Damage;

	public override AbilityData MaybeAbility => SourceAbility ?? base.MaybeAbility;

	public RuleDealDamage([NotNull] IMechanicEntity initiator, [NotNull] IMechanicEntity target, [NotNull] RuleRollDamage rollDamage)
		: this((MechanicEntity)initiator, (MechanicEntity)target, rollDamage)
	{
	}

	public RuleDealDamage([NotNull] MechanicEntity initiator, [NotNull] MechanicEntity target, [NotNull] IntermediateDamage damage)
		: this(initiator, target, new RuleRollDamage(initiator, target, damage))
	{
	}

	public RuleDealDamage([NotNull] MechanicEntity initiator, [NotNull] MechanicEntity target, [NotNull] RuleRollDamage rollDamage)
		: base(initiator, target)
	{
		TargetHealth = target.GetHealthOptional();
		TargetArmor = target.GetOptional<PartArmor>();
		m_IsFake = DamagePolicyContextData.Current == DamagePolicyType.FxOnly;
		RollDamageRule = rollDamage;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		Rulebook.Trigger(RollDamageRule);
		if (TargetHealth == null)
		{
			return;
		}
		HPBeforeDamage = TargetHealth.HitPointsLeft;
		DurabilityBeforeDamage = TargetArmor?.DurabilityLeft ?? 0;
		DurabilityAfterDamage = DurabilityBeforeDamage - ResultDamage.ResultDamageToArmorValue;
		EventBus.RaiseEvent(delegate(IDamageFXHandler h)
		{
			h.HandleDamageDealt(this);
		});
		if (AbstractUnitCommand.CommandTargetUntargetable(base.Initiator, Target, this) || (bool)Game.Instance.LoadedAreaState.Settings.Peaceful || (!IsFake && base.Initiator.IsAttackingGreenNPC(Target)))
		{
			return;
		}
		IsDot = base.Reason.Context?.Blueprint is BlueprintBuff;
		TargetHealth.LastHandledDamage = this;
		if (IsFake)
		{
			EventBus.RaiseEvent(delegate(IDamageHandler h)
			{
				h.HandleDamageDealt(this);
			});
			return;
		}
		int resultDamageToArmorValue = ResultDamage.ResultDamageToArmorValue;
		if (resultDamageToArmorValue > 0 && TargetArmor != null)
		{
			int durabilityLeft = TargetArmor.DurabilityLeft;
			TargetArmor.DealDamage(resultDamageToArmorValue);
			if (TargetArmor.DurabilityLeft == 0 && durabilityLeft > 0)
			{
				ResultArmorCrack = true;
			}
		}
		int resultDamageToHealthValue = ResultDamage.ResultDamageToHealthValue;
		if (resultDamageToHealthValue > 0 && TargetHealth != null)
		{
			int hitPointsLeft = TargetHealth.HitPointsLeft;
			DealDamageToHealth(resultDamageToHealthValue);
			if (TargetHealth.HitPointsLeft == 0 && hitPointsLeft > 0 && TargetHealth.Owner is AbstractUnitEntity)
			{
				ResultUnitDied = true;
			}
		}
		ActiveEncounter.Current?.GetOptional<PartEncounterMetrics>()?.HandleIncomingDamage(base.TargetUnit, ResultValue);
		if (ResultDamage.ResultCritsCountValue > 0)
		{
			BlueprintBodyPart bodyPart = ResultDamage.BodyPart;
			if (bodyPart != null && bodyPart.CriticalEffectStagesCount > 0)
			{
				RulePerformCriticalEffects rulePerformCriticalEffects = Rulebook.Trigger(new RulePerformCriticalEffects(base.Initiator, Target, ResultDamage));
				ResultIsCritical = rulePerformCriticalEffects.ResultAmount > 0;
			}
		}
		EventBus.RaiseEvent(delegate(IDamageHandler h)
		{
			h.HandleDamageDealt(this);
		});
	}

	private void DealDamageToHealth(int value)
	{
		if (value >= TargetHealth.HitPointsLeft && TrySaveFromDeath())
		{
			TargetHealth.SetHitPointsLeft(1);
		}
		else
		{
			TargetHealth.DealDamage(value);
		}
	}

	private bool TrySaveFromDeath()
	{
		BaseUnitEntity baseUnitEntity = TargetHealth?.Owner as BaseUnitEntity;
		AddResurrectChance.ResurrectChanceUnitPart resurrectChanceUnitPart = baseUnitEntity?.GetOptional<AddResurrectChance.ResurrectChanceUnitPart>();
		if (resurrectChanceUnitPart == null)
		{
			return false;
		}
		return RuleRollChance.Roll(ChanceRollType.AvoidDeath, baseUnitEntity, resurrectChanceUnitPart.ResurrectChance).Success;
	}
}
