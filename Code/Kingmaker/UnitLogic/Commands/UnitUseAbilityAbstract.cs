using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root.Fx;
using Kingmaker.Controllers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Framework;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.Sound;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.ProjectileAttack;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.View.Covers;
using Kingmaker.View.Mechadendrites;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Animation.Actions;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Particles;
using Kingmaker.Visual.Particles.Blueprints;
using Newtonsoft.Json;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Commands;

public abstract class UnitUseAbilityAbstract<T> : UnitCommand<T>, IUnitUseAbilityAbstract where T : UnitCommandParams, IUnitUseAbilityParamsAbstract
{
	private class HitLocationData
	{
		public readonly List<BlueprintFxLocatorGroup> LocatorGroups = new List<BlueprintFxLocatorGroup>();

		public readonly List<Vector3> ProjectileHitPositions = new List<Vector3>();
	}

	[JsonProperty]
	private AbilityAnimationStyle m_AbilityStyle;

	[JsonProperty]
	private float m_CastTime;

	[JsonProperty]
	private bool m_AfterPrecastFxSpawned;

	[JsonProperty]
	private List<GameObject> m_HandFxObjects;

	[JsonProperty]
	private List<GameObject> m_GroundFxObjects;

	[JsonProperty]
	private SpecialBehaviourType m_Special;

	private PlayLoopAnimationByBuff m_loopingAnimationBuff;

	private List<Vector3> m_ProjectileHitPositions = new List<Vector3>();

	private RulePerformAbility m_RulePerformAbility;

	[JsonProperty]
	public static bool TestPauseOnCast { get; set; }

	public AbilityExecutionProcess ExecutionProcess { get; private set; }

	public bool IgnoreCooldown => base.Params.IgnoreCooldown;

	[NotNull]
	public AbilityData Ability => base.Params.Ability;

	public bool IgnoreAbilityUsingInThreateningArea => base.Params.IgnoreAbilityUsingInThreateningArea;

	public bool IsInstantDeliver => Ability.IsInstantDeliver;

	public int ActionsCount => Ability.ActionsCount;

	public int CurrentActionIndex => ExecutionProcess?.Context.ActionIndex ?? 0;

	public bool ForceCastOnBadTarget
	{
		get
		{
			if (!base.FromCutscene)
			{
				return (base.Target?.Entity?.IsCheater).GetValueOrDefault();
			}
			return true;
		}
	}

	private bool IsTargetingDeadUnit
	{
		get
		{
			MechanicEntity mechanicEntity = base.Target?.Entity;
			if (mechanicEntity != null && mechanicEntity.IsDead && !Ability.Blueprint.CanCastToDeadTarget)
			{
				return !Ability.CanTargetPoint;
			}
			return false;
		}
	}

	public bool DisableLog => base.Params.DisableLog;

	public AttackHitPolicyType HitPolicy => base.Params.HitPolicy;

	public DamagePolicyType DamagePolicy => base.Params.DamagePolicy;

	public bool KillTarget => base.Params.KillTarget;

	public override bool ShouldBeInterrupted
	{
		get
		{
			if (!base.IsActed)
			{
				if (!IsTargetingDeadUnit && !base.Executor.IsProne)
				{
					return base.Executor.IsDeadOrUnconscious;
				}
				return true;
			}
			return false;
		}
	}

	public override bool IsUnitEnoughClose
	{
		get
		{
			if (base.Target == null || Ability.IsRangeUnrestrictedForTarget(base.Target))
			{
				return true;
			}
			if (!base.Executor.InRangeInCells(base.Target, Ability.RangeCells) && !base.FromCutscene)
			{
				return false;
			}
			GridNode currentUnwalkableNode = base.Executor.CurrentUnwalkableNode;
			int distance;
			LosCalculations.CoverType los;
			if (base.NeedLoS)
			{
				return Ability.CanTargetFromNode(currentUnwalkableNode, null, base.Target, out distance, out los);
			}
			return true;
		}
	}

	public override bool IsMoveUnit => Ability.Blueprint.IsMoveUnit;

	public override bool NeedEquipWeapons
	{
		get
		{
			if (!(Ability.SourceItem is ItemEntityWeapon))
			{
				return Ability.Blueprint.NeedEquipWeapons;
			}
			return true;
		}
	}

	public override bool DontWaitForHands => Ability.Blueprint.GetComponent<AbilityCustomCharge>();

	public override bool ShouldTurnToTarget
	{
		get
		{
			if (base.Executor != base.Target?.Entity && Ability.Blueprint.ShouldTurnToTarget)
			{
				return !IsMoveUnit;
			}
			return false;
		}
	}

	public override bool MarkManualTarget => false;

	public override bool IsInterruptible
	{
		get
		{
			if (ExecutionProcess == null || !ExecutionProcess.IsEngageUnit)
			{
				return base.IsInterruptible;
			}
			return false;
		}
	}

	public override bool CanStart
	{
		get
		{
			if (!base.IsFreeAction && !base.FromCutscene)
			{
				return Ability.HasEnoughActionPoint;
			}
			return true;
		}
	}

	protected override int ExpectedActEventsCount => ActionsCount;

	protected UnitUseAbilityAbstract([NotNull] T @params)
		: base(@params)
	{
	}

	protected override void OnInit(AbstractUnitEntity executor)
	{
		base.OnInit(executor);
		if (!base.FromCutscene && !Ability.CanTarget(base.Target, out var unavailableReason))
		{
			PFLog.Default.Error($"{Ability.Blueprint.NameSafe()}: cannot target {base.Target} because of {unavailableReason}. Cast by {base.Executor}");
			QAModeExceptionReporter.MaybeShowError($"{Ability.Blueprint.NameSafe()}: cannot target {base.Target} because of {unavailableReason}. Cast by {base.Executor}");
		}
		if (!base.FromCutscene)
		{
			Ability fact = Ability.Fact;
			if (fact != null && !fact.Active)
			{
				PFLog.Default.Error(base.Executor.View.AsUnitEntityView(), $"Unit {base.Executor} casting spell {Ability.Blueprint} that is not in spellbook or unit abilities");
				Interrupt();
				return;
			}
		}
		if (!base.IsOneFrameCommand)
		{
			m_AbilityStyle = Ability.Blueprint.SpellAnimation;
			m_CastTime = 2.5f;
		}
		else
		{
			m_Special = SpecialBehaviourType.NoPrecast;
			m_CastTime = 0f;
		}
		if (TurnController.IsInTurnBasedCombat() && base.Executor.IsInCombat)
		{
			m_CastTime = Math.Min(5f, m_CastTime);
		}
		base.Executor.MaybeUnitAnimationManager.Or(null)?.AimIKTargets.Clear();
	}

	public override void OnRun()
	{
		base.OnRun();
		TryDelegateCommandToMount();
	}

	private void TryDelegateCommandToMount()
	{
		BaseUnitEntity saddledUnit = base.Executor.GetSaddledUnit();
		if (saddledUnit != null && Ability.ShouldDelegateToMount && Ability.SameMountAbility != null)
		{
			saddledUnit.Commands.Run(new UnitUseAbilityParams(Ability.SameMountAbility, base.Target));
		}
	}

	protected override void TriggerAnimation()
	{
		if (Ability.Weapon == null && Ability.Blueprint.GetComponent<AbilityCustomBladeDance>() == null)
		{
			base.Executor.View.HideOffWeapon(value: true);
		}
		AbilityCustomBladeDance component = Ability.Blueprint.GetComponent<AbilityCustomBladeDance>();
		if (component != null && !component.UseOnSourceWeapon)
		{
			Ability.OverrideWeapon = (component.UseSecondWeapon ? (base.Executor.GetSecondWeapon() ?? base.Executor.GetFirstWeapon()) : base.Executor.GetFirstWeapon());
		}
		if (base.Target != null)
		{
			Vector3 vector = base.Target.Point - base.Executor.Position;
			BlueprintAbilityFXSettings fXSettings = Ability.FXSettings;
			if (fXSettings != null && fXSettings.ShouldOffsetTargetRelativePosition)
			{
				Vector3 vector2 = Quaternion.LookRotation((base.ApproachPoint - base.Executor.Position).normalized) * Ability.FXSettings.OffsetTargetPosition;
				vector = base.ApproachPoint + vector2;
				if (base.Executor.MaybeUnitAnimationManager != null)
				{
					base.Executor.MaybeUnitAnimationManager.UseAbilityDirection = base.Executor.GetOrientationTo(vector);
				}
			}
			else
			{
				vector.y = 0f;
				if (base.Executor.MaybeUnitAnimationManager != null)
				{
					base.Executor.MaybeUnitAnimationManager.UseAbilityDirection = ((vector == Vector3.zero) ? 0f : Quaternion.LookRotation(vector).eulerAngles.y);
				}
			}
		}
		if (Ability.NeedLoS)
		{
			GridNodeBase castNode = (GridNodeBase)(GraphNode)base.Executor.CurrentNode;
			Ability.GetBestShootingPosition(castNode, base.Target);
			if ((object)base.Executor.MaybeUnitAnimationManager != null)
			{
				base.Executor.MaybeUnitAnimationManager.AbilityIsSpell = Ability.Blueprint.IsSpell;
			}
		}
		bool isCornerAttack = false;
		ItemEntityWeapon weapon = Ability.Weapon;
		if (weapon != null && weapon.Blueprint.IsMelee)
		{
			Int3 position = base.Executor.CurrentNode.node.position;
			Int3? @int = base.Target?.NearestNode.position;
			if (@int.HasValue)
			{
				Int3 value = position;
				Int3? int2 = @int;
				Int3? int3 = value - int2;
				isCornerAttack = (int3.Value.x != 0 && int3.Value.z != 0) || (int3.Value.x == 0 && int3.Value.z == 0);
			}
		}
		IAbilityCustomAnimation abilityCustomAnimation = base.Params.CustomAnimationOverride ?? Ability.Blueprint.GetComponent<IAbilityCustomAnimation>();
		if (abilityCustomAnimation != null)
		{
			UnitAnimationAction unitAnimationAction = abilityCustomAnimation.GetAbilityAction(base.Executor)?.Load();
			if (!unitAnimationAction)
			{
				ScheduleAct();
			}
			else
			{
				PFLog.Animations.Log(base.Executor.View.AsUnitEntityView(), $"Ability {Ability.Blueprint} uses custom animation from IAbilityCustomAnimation-component or overriden animation from cutscene command: {unitAnimationAction}");
				if (!TryStartAnimation(unitAnimationAction, isCornerAttack))
				{
					PFLog.Animations.Error(base.Executor.View.AsUnitEntityView(), $"{base.Executor} cannot start custom animation {unitAnimationAction} for {Ability.Blueprint}");
					ScheduleAct();
				}
			}
		}
		else if (Ability.Blueprint.HasAnimation)
		{
			StartAnimation(Ability.Blueprint.IsSpell ? UnitAnimationType.Ability : UnitAnimationType.Attack, UnitAnimationActionHandleInitializer);
		}
		else
		{
			ScheduleAct();
		}
		base.TriggerAnimation();
		void UnitAnimationActionHandleInitializer(UnitAnimationActionHandle h)
		{
			SetHandlesParameters(h);
			h.AbilityStyle = m_AbilityStyle;
			h.CastInOffhand = Ability.Blueprint.CastInOffHand || (Ability.Blueprint.IsSpell && Ability.Blueprint.SpellAnimation == AbilityAnimationStyle.Reload && GetAttackingLimb(Ability) == AttackingLimb.OffHand);
			h.IsCornerAttack = isCornerAttack;
			h.IsBladeDance = HasTwoMeleeForBladeDance();
			h.IsTargetSelf = base.Target?.Entity == base.Executor;
		}
	}

	private bool TryStartAnimation(UnitAnimationAction action, bool isCornerAttack)
	{
		bool isUnitAnimationHandle = false;
		UnitAnimationManager animationManager = base.Executor.View.AnimationManager;
		if (animationManager == null)
		{
			return false;
		}
		AnimationActionHandle handle;
		bool num = animationManager.TryExecute(action, delegate(AnimationActionHandle h)
		{
			if (h is UnitAnimationActionHandle unitAnimationActionHandle)
			{
				isUnitAnimationHandle = true;
				SetHandlesParameters(unitAnimationActionHandle);
				unitAnimationActionHandle.IsCornerAttack = isCornerAttack;
			}
		}, out handle);
		if (num && isUnitAnimationHandle)
		{
			base.Animation = (UnitAnimationActionHandle)handle;
			base.HasAnimation = true;
			m_IsAnimationActHandled = false;
		}
		return num && isUnitAnimationHandle;
	}

	private static Vector3 GetHitPosition(IMechanicEntityView targetView, List<BpRef<BlueprintFxLocatorGroup>> locatorGroups)
	{
		ParticlesSnapMap snapMap = targetView.ParticlesSnapMap;
		if (snapMap == null)
		{
			return targetView.transform.position;
		}
		BpRef<BlueprintFxLocatorGroup> bpRef = locatorGroups.Where((BpRef<BlueprintFxLocatorGroup> g) => !g.IsNull() && snapMap.GetLocators(g.Blueprint) != null).Random(PFStatefulRandom.Visuals.Fx);
		Transform transform = null;
		if (bpRef != null)
		{
			transform = (from b in snapMap.GetLocators(bpRef.Blueprint)
				where b != null
				select b).Random(PFStatefulRandom.Visuals.Fx)?.Transform;
		}
		if (transform == null)
		{
			FxBone locatorFirst = snapMap.GetLocatorFirst(FxRoot.Instance.LocatorGroupDefaultHit);
			if (locatorFirst != null)
			{
				transform = locatorFirst.Transform;
			}
		}
		if (!(transform != null))
		{
			return targetView.transform.position;
		}
		return transform.position;
	}

	private static Vector3 GetHitPosition(IMechanicEntityView targetView, BlueprintFxLocatorGroup locatorGroup)
	{
		ParticlesSnapMap particlesSnapMap = targetView.ParticlesSnapMap;
		if (particlesSnapMap == null)
		{
			return targetView.transform.position;
		}
		TempList.Get<Transform>();
		Transform transform = particlesSnapMap.GetLocators(locatorGroup)?.Random(PFStatefulRandom.Visuals.Fx).Transform;
		if (transform == null)
		{
			transform = particlesSnapMap.GetLocatorFirst(FxRoot.Instance.LocatorGroupDefaultHit).Transform;
		}
		return transform.position;
	}

	private void SetHandlesParameters(UnitAnimationActionHandle h)
	{
		h.CastingTime = m_CastTime;
		h.AttackWeaponType = ((h.Manager.ActiveWeaponStyle == WeaponAnimationStyle.Creature) ? WeaponType.Creature : (Ability.Weapon?.WeaponType ?? WeaponType.Fist));
		h.AttackingLimb = GetAttackingLimb(Ability);
		h.Spell = Ability.Blueprint.OriginalBlueprint;
		h.SpecialCastBehaviour = m_Special;
		h.AlternativeStyle = Ability.Blueprint.GetComponent<AttackAlternativeAnimationStyle>()?.WeaponAnimationStyle ?? AnimationAlternativeStyle.None;
		h.AttackTargetDistance = (base.Target.Point - base.Executor.Position).magnitude;
		h.IsBurst = Ability.IsBurstAttack;
		h.BurstCount = Ability.BurstAttacksCount;
		h.CustomRpm = Ability.CustomRpm;
		h.AttackType = Ability.AttackAnimationType;
	}

	private bool HasTwoMeleeForBladeDance()
	{
		AbilityCustomBladeDance component = Ability.Blueprint.GetComponent<AbilityCustomBladeDance>();
		if (component == null)
		{
			return false;
		}
		ItemEntityWeapon maybeWeapon = base.Executor.Body.PrimaryHand.MaybeWeapon;
		ItemEntityWeapon maybeWeapon2 = base.Executor.Body.SecondaryHand.MaybeWeapon;
		if (!component.UseSpecificWeaponClassification)
		{
			if (!component.UseSpecificWeapon && maybeWeapon != null)
			{
				return maybeWeapon2 != null;
			}
			return false;
		}
		if (!component.UseSpecificWeapon && maybeWeapon?.Blueprint?.Classification == component.Classification)
		{
			return maybeWeapon2?.Blueprint?.Classification == component.Classification;
		}
		return false;
	}

	private static AttackingLimb GetAttackingLimb(AbilityData ability)
	{
		PartUnitBody bodyOptional = ability.Caster.GetBodyOptional();
		if (bodyOptional == null || ability.Weapon == bodyOptional.PrimaryHand?.MaybeWeapon)
		{
			return AttackingLimb.MainHand;
		}
		if (ability.Caster?.GetOptional<UnitPartMechadendrites>() == null)
		{
			return AttackingLimb.OffHand;
		}
		return AttackingLimb.Mechadendrite;
	}

	protected override Vector3 GetTargetPoint()
	{
		return base.Target.Entity?.GetInnerNodeNearestToTarget(base.Executor.Position).Vector3Position() ?? base.Target.Point;
	}

	public override void TurnToTarget()
	{
		BlueprintAbilityFXSettings fXSettings = Ability.FXSettings;
		if (fXSettings != null && fXSettings.ShouldOffsetTargetRelativePosition)
		{
			Vector3 vector = Quaternion.LookRotation((base.ApproachPoint - base.Executor.Position).normalized) * Ability.FXSettings.OffsetTargetPosition;
			Vector3 point = base.ApproachPoint + vector;
			base.Executor.TurnTo(point);
		}
		else
		{
			base.TurnToTarget();
		}
	}

	protected override void OnStart()
	{
		RulePerformAbility rulePerformAbility = new RulePerformAbility(Ability, base.Target, base.Params.ParentContext);
		rulePerformAbility.IsCutscene = base.FromCutscene;
		rulePerformAbility.DisableGameLog = DisableLog;
		rulePerformAbility.IgnoreCooldown = IgnoreCooldown;
		rulePerformAbility.ForceFreeAction = base.IsFreeAction;
		rulePerformAbility.Context.DisableLog = DisableLog;
		rulePerformAbility.Context.HitPolicy = HitPolicy;
		rulePerformAbility.Context.DamagePolicy = DamagePolicy;
		rulePerformAbility.Context.KillTarget = KillTarget;
		m_RulePerformAbility = rulePerformAbility;
		CalculateHitPositions();
		base.OnStart();
		Ability.IgnoreUsingInThreateningArea = IgnoreAbilityUsingInThreateningArea;
		if (m_Special != SpecialBehaviourType.NoPrecast)
		{
			CastAbilityFx(AbilitySpawnFxTime.OnPrecastStart);
		}
		EventBus.RaiseEvent(delegate(IVisualWeaponStateChangeHandle h)
		{
			h.VisualWeaponStateChangeHandle(VFXSpeedUpdater.WeaponVisualState.InAttack, (base.Executor?.View == null) ? null : base.Executor?.View.HandsEquipment?.GetWeaponModel(offHand: false));
		});
	}

	private void CalculateHitPositions()
	{
		if (!CanAimWithIK())
		{
			return;
		}
		m_RulePerformAbility.Context.PreparedDeliveryProcess = TryPrepareDeliveryProcess(m_RulePerformAbility.Context, base.Target);
		if (Ability.IsPrecise)
		{
			List<BpRef<BlueprintFxLocatorGroup>> fxLocatorGroups = Ability.PreciseBodyPart.FxLocatorGroups;
			if (fxLocatorGroups != null)
			{
				Vector3 hitPosition = GetHitPosition(base.Target.Entity.View, fxLocatorGroups);
				m_ProjectileHitPositions.Add(hitPosition);
				goto IL_0180;
			}
		}
		if (m_RulePerformAbility.Context.PreparedDeliveryProcess is AbilityProjectileAttack { Attacks: var attacks } abilityProjectileAttack)
		{
			foreach (AbilityProjectileAttackLine abilityProjectileAttackLine in attacks)
			{
				GridNodeBase node = abilityProjectileAttackLine.Nodes.Last();
				AbilityProjectileAttackLine.HitData hitData = abilityProjectileAttackLine.Hits.LastItem();
				RulePerformAttackRoll rollPerformAttackRule = hitData.RollPerformAttackRule;
				if (!hitData.Empty)
				{
					RulePerformAttackRoll rollPerformAttackRule2 = hitData.RollPerformAttackRule;
					if (rollPerformAttackRule2 != null && rollPerformAttackRule2.ResultIsHit && rollPerformAttackRule2.ResultHitLocation != null)
					{
						Vector3 hitPosition2 = GetHitPosition(hitData.Entity.View, rollPerformAttackRule.ResultHitLocation.FxLocatorGroups);
						m_ProjectileHitPositions.Add(hitPosition2);
						continue;
					}
				}
				Vector3 item = node.Vector3Position();
				item.y = abilityProjectileAttackLine.ToNode.Vector3Position().y + 1f;
				m_ProjectileHitPositions.Add(item);
			}
			BeautifyAttackLinesOrder(abilityProjectileAttack.Attacks, m_ProjectileHitPositions);
		}
		goto IL_0180;
		IL_0180:
		if (m_ProjectileHitPositions.Empty())
		{
			Vector3 hitPosition3 = GetHitPosition(base.Target.Entity.View, FxRoot.Instance.LocatorGroupDefaultHit);
			m_ProjectileHitPositions.Add(hitPosition3);
		}
		foreach (Vector3 projectileHitPosition in m_ProjectileHitPositions)
		{
			base.Executor.MaybeUnitAnimationManager.Or(null)?.AimIKTargets.Add(projectileHitPosition);
		}
	}

	private bool CanAimWithIK()
	{
		if ((Ability.Weapon?.WeaponType ?? WeaponType.Fist).IsRanged())
		{
			return !Ability.Blueprint.OriginalBlueprint.IsAoE;
		}
		return false;
	}

	private IEnumerator<AbilityDeliveryTarget> TryPrepareDeliveryProcess(AbilityExecutionContext context, TargetWrapper target)
	{
		if (!Ability.IsBurst)
		{
			return AbilityProjectileAttack.CreateSingleTarget(context, context.Caster.CurrentUnwalkableNode, target.Entity, 1);
		}
		if (!context.Ability.CanTargetPoint)
		{
			return AbilityProjectileAttack.CreateBurst(context, context.Caster.CurrentUnwalkableNode, target, context.Ability.BurstAttacksCount, Ability.Blueprint.IsControlledBurst);
		}
		return AbilityProjectileAttack.CreatePatternBurst(context, context.Caster.CurrentUnwalkableNode, target, context.Ability.BurstAttacksCount, Ability.Blueprint.IsControlledBurst);
	}

	private void BeautifyAttackLinesOrder(AbilityProjectileAttackLine[] attackLines, List<Vector3> hitPositions)
	{
		Dictionary<AbilityProjectileAttackLine, (float cos, Vector3 hitPosition)> dict = new Dictionary<AbilityProjectileAttackLine, (float, Vector3)>();
		Vector2 vector = base.Executor.Position.To2D();
		Vector2 normalized = (base.Target.Point.To2D() - vector).normalized;
		Vector2 lhs = new Vector2(normalized.y, 0f - normalized.x);
		for (int i = 0; i < attackLines.Length; i++)
		{
			AbilityProjectileAttackLine key = attackLines[i];
			Vector3 vector2 = hitPositions[i];
			Vector2 normalized2 = (vector2.To2D() - vector).normalized;
			float item = Vector2.Dot(lhs, normalized2);
			dict[key] = (item, vector2);
		}
		Array.Sort(attackLines, (AbilityProjectileAttackLine _1, AbilityProjectileAttackLine _2) => dict[_1].cos.CompareTo(dict[_2].cos));
		for (int j = 0; j < attackLines.Length; j++)
		{
			attackLines[j].Index = j;
			hitPositions[j] = dict[attackLines[j]].hitPosition;
		}
	}

	[NotNull]
	private List<GameObject> SureGroundFxList()
	{
		return m_GroundFxObjects ?? (m_GroundFxObjects = new List<GameObject>());
	}

	private void CastAbilityFx(AbilitySpawnFxTime time)
	{
		IEnumerable<AbilitySpawnFx> components = Ability.Blueprint.GetComponents<AbilitySpawnFx>();
		IEvalContext ctx;
		using (EvalContext.PushAbility(Ability, base.Target, null, Ability.Caster.Position).Get(out ctx))
		{
			foreach (AbilitySpawnFx item in components)
			{
				if (item.Time == time)
				{
					item.Spawn(ctx, null, item.DestroyOnCast ? SureGroundFxList() : null);
				}
			}
		}
	}

	protected override void OnTick()
	{
		base.OnTick();
		if (base.Animation != null && base.Animation.IsPrecastFinished && !m_AfterPrecastFxSpawned)
		{
			m_AfterPrecastFxSpawned = true;
			CastAbilityFx(AbilitySpawnFxTime.OnPrecastFinished);
		}
		AbilityExecutionProcess executionProcess = ExecutionProcess;
		if (executionProcess != null && executionProcess.IsEngageUnit && executionProcess.IsEnded)
		{
			UnitAnimationActionHandle animation = base.Animation;
			if (animation == null || animation.IsReleased)
			{
				ForceFinish(ResultType.Success);
			}
		}
		if (!base.IsActed && !Ability.CanTarget(base.Target) && !base.FromCutscene)
		{
			Interrupt();
		}
	}

	protected override ResultType OnAction()
	{
		if (CurrentActionIndex > 0)
		{
			if (CurrentActionIndex >= ActionsCount)
			{
				PFLog.Default.Error("CurrentActionIndex >= ActionsCount");
				return ResultType.Fail;
			}
			if (ExecutionProcess == null)
			{
				PFLog.Default.Error("ExecutionProcess == null");
				return ResultType.Fail;
			}
			ExecutionProcess.Context.NextAction();
			if (!ExecutionProcess.IsEngageUnit && CurrentActionIndex >= ActionsCount)
			{
				return ResultType.Success;
			}
			return ResultType.None;
		}
		using (ContextData<AbilityData.IgnoreCooldown>.RequestIf(IgnoreCooldown))
		{
			using (ContextData<AbilityData.ForceFreeAction>.RequestIf(base.IsFreeAction))
			{
				if (!ForceCastOnBadTarget && !Ability.IsAvailable)
				{
					return ResultType.Fail;
				}
			}
		}
		if (IsTargetingDeadUnit)
		{
			return ResultType.Fail;
		}
		if (!ForceCastOnBadTarget)
		{
			MechanicEntity entity = base.Target.Entity;
			if (entity != null && !entity.IsInGame)
			{
				return ResultType.Fail;
			}
		}
		if (!base.FromCutscene && !Ability.CanTarget(base.Target))
		{
			return ResultType.Fail;
		}
		if (!base.FromCutscene)
		{
			ItemEntity sourceItem = Ability.SourceItem;
			if (sourceItem != null && sourceItem.IsSpendCharges && sourceItem.Charges <= 0)
			{
				return ResultType.Fail;
			}
		}
		bool isBonusUsage = Ability.IsBonusUsage;
		int num = Ability.CalculateActionPointCost();
		m_RulePerformAbility.Context.ProjectileHitPositions = m_ProjectileHitPositions;
		Rulebook.Trigger(m_RulePerformAbility);
		ExecutionProcess = m_RulePerformAbility.Result;
		if (ExecutionProcess == null)
		{
			return ResultType.Fail;
		}
		if (IsInstantDeliver)
		{
			ExecutionProcess.InstantDeliver();
		}
		if (!base.FromCutscene)
		{
			Ability.Spend();
		}
		if (!m_RulePerformAbility.Success)
		{
			return ResultType.Fail;
		}
		if (m_HandFxObjects != null)
		{
			foreach (GameObject handFxObject in m_HandFxObjects)
			{
				FxHelper.Destroy(handFxObject);
			}
			m_HandFxObjects = null;
		}
		if (m_GroundFxObjects != null)
		{
			foreach (GameObject groundFxObject in m_GroundFxObjects)
			{
				FxHelper.Destroy(groundFxObject);
			}
			m_GroundFxObjects = null;
		}
		if (!base.IsFreeAction && num > 0)
		{
			base.Executor.CombatState.SpendActionPoints(num);
			EventBus.RaiseEvent(delegate(IUnitActionPointsHandler h)
			{
				h.HandleActionPointsSpent(base.Executor);
			});
		}
		if (!base.FromCutscene && !IgnoreCooldown && !isBonusUsage)
		{
			base.Executor.GetAbilityCooldownsOptional()?.StartCooldown(Ability);
		}
		ExecutionProcess.Context.NextAction();
		if (!ExecutionProcess.IsEngageUnit && CurrentActionIndex >= ActionsCount)
		{
			return ResultType.Success;
		}
		return ResultType.None;
	}

	protected override void OnEnded()
	{
		base.OnEnded();
		Ability.IgnoreUsingInThreateningArea = false;
		EventBus.RaiseEvent(delegate(IVisualWeaponStateChangeHandle h)
		{
			h.VisualWeaponStateChangeHandle(VFXSpeedUpdater.WeaponVisualState.InHand, (base.Executor?.View == null) ? null : base.Executor?.View.HandsEquipment?.GetWeaponModel(offHand: false));
		});
		if (base.Executor?.View != null)
		{
			base.Executor.View.HideOffWeapon(value: false);
		}
		if ((m_Special != SpecialBehaviourType.NoCast || base.Result != ResultType.Success) && m_HandFxObjects != null)
		{
			foreach (GameObject handFxObject in m_HandFxObjects)
			{
				FxHelper.Destroy(handFxObject);
			}
			m_HandFxObjects = null;
		}
		if (m_GroundFxObjects == null)
		{
			return;
		}
		foreach (GameObject groundFxObject in m_GroundFxObjects)
		{
			FxHelper.Destroy(groundFxObject);
		}
		m_GroundFxObjects = null;
	}

	protected override string GetInnerDataDescription()
	{
		return Ability.Blueprint.NameSafe();
	}
}
