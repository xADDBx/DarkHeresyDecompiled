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
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Covers;
using Kingmaker.View.Mechadendrites;
using Kingmaker.View.Mechanics;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using Kingmaker.Visual.Particles;
using Kingmaker.Visual.Particles.Blueprints;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Commands;

public abstract class UnitUseAbilityAbstract<T> : UnitCommand<T> where T : UnitCommandParams, IUnitUseAbilityParamsAbstract
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

	private HitLocationData m_HitLocationData = new HitLocationData();

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

	public override bool ShouldTurnToTarget => Ability.Blueprint.ShouldTurnToTarget;

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
				PFLog.Default.Error(base.Executor.View, $"Unit {base.Executor} casting spell {Ability.Blueprint} that is not in spellbook or unit abilities");
				Interrupt();
				return;
			}
		}
		if (!base.IsOneFrameCommand)
		{
			m_AbilityStyle = Ability.Blueprint.Animation;
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
		base.Executor.MaybeAnimationManager.Or(null)?.AimIKTargetsQueue.Clear();
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
			base.Executor.View.HideOffWeapon(hide: true);
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
				if (base.Executor.MaybeAnimationManager != null)
				{
					base.Executor.MaybeAnimationManager.UseAbilityDirection = base.Executor.GetOrientationTo(vector);
				}
			}
			else
			{
				vector.y = 0f;
				if (base.Executor.MaybeAnimationManager != null)
				{
					base.Executor.MaybeAnimationManager.UseAbilityDirection = ((vector == Vector3.zero) ? 0f : Quaternion.LookRotation(vector).eulerAngles.y);
				}
			}
		}
		if (Ability.NeedLoS)
		{
			GridNodeBase gridNodeBase = (GridNodeBase)(GraphNode)base.Executor.CurrentNode;
			GridNodeBase bestShootingPosition = Ability.GetBestShootingPosition(gridNodeBase, base.Target);
			if ((object)base.Executor.MaybeAnimationManager != null)
			{
				int num = (bestShootingPosition.XCoordinateInGrid - gridNodeBase.XCoordinateInGrid) * (base.Target.NearestNode.ZCoordinateInGrid - gridNodeBase.ZCoordinateInGrid) - (bestShootingPosition.ZCoordinateInGrid - gridNodeBase.ZCoordinateInGrid) * (base.Target.NearestNode.XCoordinateInGrid - gridNodeBase.XCoordinateInGrid);
				UnitAnimationActionCover.StepOutDirectionAnimationType stepOutDirectionAnimationType = ((num > 0) ? UnitAnimationActionCover.StepOutDirectionAnimationType.Right : ((num < 0) ? UnitAnimationActionCover.StepOutDirectionAnimationType.Left : UnitAnimationActionCover.StepOutDirectionAnimationType.None));
				UnitAnimationActionCover.StepOutDirectionAnimationType stepOutDirectionAnimationType2 = stepOutDirectionAnimationType;
				base.Executor.MaybeAnimationManager.StepOutDirectionAnimationType = stepOutDirectionAnimationType2;
				base.Executor.MaybeAnimationManager.AbilityIsSpell = Ability.Blueprint.IsSpell;
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
				PFLog.Animations.Log(base.Executor.View, $"Ability {Ability.Blueprint} uses custom animation from IAbilityCustomAnimation-component or overriden animation from cutscene command: {unitAnimationAction}");
				if (!TryStartAnimation(unitAnimationAction, isCornerAttack))
				{
					PFLog.Animations.Error(base.Executor.View, $"{base.Executor} cannot start custom animation {unitAnimationAction} for {Ability.Blueprint}");
					ScheduleAct();
				}
			}
		}
		else if (base.Executor.IsInSquad && !Ability.Blueprint.IsSpell && IsMainHandAttack(Ability) && Ability.Weapon != null && !Ability.Weapon.Blueprint.IsMelee && base.Executor.View != null && base.Executor.View.AnimationManager != null && base.Executor.View.AnimationManager.CurrentMainHandAttackForPrepare != null)
		{
			base.Animation = base.Executor.View.AnimationManager.CurrentMainHandAttackForPrepare;
			base.HasAnimation = true;
			if (base.Animation == null)
			{
				ScheduleAct();
				return;
			}
			base.Animation.AlternativeStyle = Ability.Blueprint.GetComponent<AttackAlternativeAnimationStyle>()?.WeaponAnimationStyle ?? AnimationAlternativeStyle.None;
			base.Animation.IsBurst = Ability.IsBurstAttack;
			base.Animation.BurstCount = Ability.BurstAttacksCount;
			base.Animation.CustomRpm = Ability.CustomRpm;
			base.Animation.BurstAnimationDelay = Ability.Weapon?.Blueprint.VisualParameters.BurstAnimationDelay ?? 0f;
			base.Animation.IsPreparingForShooting = false;
			base.Animation.StartInternal();
		}
		else
		{
			if (base.Executor.View?.AnimationManager?.CurrentMainHandAttackForPrepare != null)
			{
				base.Executor.View.AnimationManager.CurrentMainHandAttackForPrepare.Release();
			}
			StartAnimation(Ability.Blueprint.IsSpell ? UnitAnimationType.Ability : UnitAnimationType.Attack, UnitAnimationActionHandleInitializer);
		}
		base.TriggerAnimation();
		void UnitAnimationActionHandleInitializer(UnitAnimationActionHandle h)
		{
			SetHandlesParameters(h);
			h.AbilityStyle = m_AbilityStyle;
			h.CastInOffhand = Ability.Blueprint.CastInOffHand;
			if (Ability.Blueprint.IsSpell && Ability.Blueprint.Animation == AbilityAnimationStyle.Reload && !IsMainHandAttack(Ability))
			{
				h.CastInOffhand = true;
			}
			h.IsCornerAttack = isCornerAttack;
			h.BurstAnimationDelay = Ability.Weapon?.Blueprint.VisualParameters.BurstAnimationDelay ?? 0f;
			h.IsBladeDance = HasTwoMeleeForBladeDance();
			h.IsTargetSelf = base.Target?.Entity == base.Executor;
			SetupAimIKData(base.Target?.Entity?.View, m_HitLocationData);
		}
	}

	private bool TryStartAnimation(UnitAnimationAction action, bool isCornerAttack)
	{
		if (!(base.Executor.View.AnimationManager.Or(null)?.CreateHandle(action) is UnitAnimationActionHandle unitAnimationActionHandle))
		{
			return false;
		}
		SetHandlesParameters(unitAnimationActionHandle);
		unitAnimationActionHandle.IsCornerAttack = isCornerAttack;
		SetupAimIKData(base.Target?.Entity?.View, m_HitLocationData);
		StartAnimation(unitAnimationActionHandle);
		return true;
	}

	private void SetupAimIKData(MechanicEntityView targetView, HitLocationData hitLocationData)
	{
		Transform hitLocationTransform = GetHitLocationTransform(targetView, hitLocationData.LocatorGroups);
		if (hitLocationTransform != null)
		{
			base.Executor.MaybeAnimationManager.Or(null)?.AimIKTargetsQueue.Enqueue(hitLocationTransform);
		}
		Vector3? vector = hitLocationTransform.Or(null)?.position;
		if (vector.HasValue)
		{
			Vector3 valueOrDefault = vector.GetValueOrDefault();
			hitLocationData.ProjectileHitPositions.Add(valueOrDefault);
		}
	}

	private static Transform GetHitLocationTransform(MechanicEntityView targetView, IReadOnlyList<BlueprintFxLocatorGroup> locatorGroups)
	{
		ParticlesSnapMap particlesSnapMap = targetView.Or(null)?.ParticlesSnapMap;
		if (particlesSnapMap == null)
		{
			return null;
		}
		Transform transform = (particlesSnapMap.GetLocators(locatorGroups[0])?.FirstOrDefault()?.Transform).Or(null);
		if ((object)transform == null)
		{
			FxBone locatorFirst = particlesSnapMap.GetLocatorFirst(FxRoot.Instance.LocatorGroupDefaultHit);
			if (locatorFirst == null)
			{
				return null;
			}
			transform = locatorFirst.Transform;
		}
		return transform;
	}

	private void SetHandlesParameters(UnitAnimationActionHandle h)
	{
		h.CastingTime = m_CastTime;
		h.AttackWeaponType = ((h.Manager.ActiveWeaponStyle == WeaponAnimationStyle.Creature) ? WeaponType.Creature : (Ability.Weapon?.WeaponType ?? WeaponType.Fist));
		h.IsMainHandAttack = IsMainHandAttack(Ability);
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

	protected override void StartAnimation(UnitAnimationActionHandle handle)
	{
		ItemEntityWeapon weapon = Ability.Weapon;
		MechadendritesType mechadendritesType = ((weapon == null || weapon.Blueprint.IsMelee) ? Ability.Blueprint.UsedMechadendrite : MechadendritesType.Ballistic);
		ItemEntityWeapon weapon2 = Ability.Weapon;
		if (((weapon2 != null && !weapon2.Blueprint.IsMelee) || Ability.Blueprint.UseOnMechadendrite) && base.Executor.GetOptional<UnitPartMechadendrites>() != null)
		{
			UnitPartMechadendrites optional = base.Executor.GetOptional<UnitPartMechadendrites>();
			if (optional != null && optional.Mechadendrites.ContainsKey(mechadendritesType))
			{
				base.Animation = handle;
				base.HasAnimation = true;
				UnitAnimationActionHandle unitAnimationActionHandle = (UnitAnimationActionHandle)handle.Clone();
				handle.AbilityStyle = AbilityAnimationStyle.Mechadendrites;
				unitAnimationActionHandle.ChangeManager(base.Executor.GetOptional<UnitPartMechadendrites>()?.Mechadendrites[mechadendritesType]?.AnimationManager);
				if (unitAnimationActionHandle.Action != null)
				{
					base.Executor.GetOptional<UnitPartMechadendrites>()?.Mechadendrites[mechadendritesType]?.AnimationManager.Execute(unitAnimationActionHandle);
					Dictionary<MechadendritesType, MechadendriteSettings>.ValueCollection valueCollection = base.Executor.GetOptional<UnitPartMechadendrites>()?.Mechadendrites.Values;
					if (valueCollection != null && !valueCollection.Empty())
					{
						foreach (MechadendriteSettings item in valueCollection)
						{
							if (item.MechadendritesType != mechadendritesType)
							{
								UnitAnimationActionHandle unitAnimationActionHandle2 = (UnitAnimationActionHandle)handle.Clone();
								unitAnimationActionHandle2.AbilityStyle = AbilityAnimationStyle.Mechadendrites;
								unitAnimationActionHandle2.ChangeManager(item.AnimationManager);
								if (unitAnimationActionHandle2.Action != null)
								{
									item.AnimationManager.Execute(unitAnimationActionHandle2);
								}
							}
						}
					}
				}
			}
		}
		base.StartAnimation(handle);
	}

	private bool IsMainHandAttack(AbilityData ability)
	{
		PartUnitBody bodyOptional = ability.Caster.GetBodyOptional();
		if (bodyOptional == null)
		{
			return true;
		}
		if (ability.Caster?.GetOptional<UnitPartMechadendrites>() == null)
		{
			return ability.Weapon == bodyOptional.PrimaryHand?.MaybeWeapon;
		}
		return true;
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
		CalculateHitLocations();
		base.OnStart();
		Ability.IgnoreUsingInThreateningArea = IgnoreAbilityUsingInThreateningArea;
		if (m_Special != SpecialBehaviourType.NoPrecast)
		{
			CastAbilityFx(AbilitySpawnFxTime.OnPrecastStart);
		}
		EventBus.RaiseEvent(delegate(IVisualWeaponStateChangeHandle h)
		{
			h.VisualWeaponStateChangeHandle(VFXSpeedUpdater.WeaponVisualState.InAttack, (!(base.Executor?.View != null)) ? null : base.Executor?.View.HandsEquipment?.GetWeaponModel(offHand: false));
		});
	}

	private void CalculateHitLocations()
	{
		m_HitLocationData.LocatorGroups.Add(FxRoot.Instance.LocatorGroupDefaultHit);
	}

	[NotNull]
	private List<GameObject> SureGroundFxList()
	{
		return m_GroundFxObjects ?? (m_GroundFxObjects = new List<GameObject>());
	}

	private void CastAbilityFx(AbilitySpawnFxTime time)
	{
		IEnumerable<AbilitySpawnFx> components = Ability.Blueprint.GetComponents<AbilitySpawnFx>();
		AbilityExecutionContext context = AbilityExecutionContext.Claim(Ability, base.Target, Ability.Caster.Position);
		foreach (AbilitySpawnFx item in components)
		{
			if (item.Time == time)
			{
				item.Spawn(context, null, item.DestroyOnCast ? SureGroundFxList() : null);
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
		RulePerformAbility rulePerformAbility = new RulePerformAbility(Ability, base.Target, base.Params.ParentContext);
		rulePerformAbility.IsCutscene = base.FromCutscene;
		rulePerformAbility.DisableGameLog = DisableLog;
		rulePerformAbility.IgnoreCooldown = IgnoreCooldown;
		rulePerformAbility.ForceFreeAction = base.IsFreeAction;
		rulePerformAbility.Context.DisableLog = DisableLog;
		rulePerformAbility.Context.HitPolicy = HitPolicy;
		rulePerformAbility.Context.DamagePolicy = DamagePolicy;
		rulePerformAbility.Context.KillTarget = KillTarget;
		rulePerformAbility.Context.ProjectileHitPositions = m_HitLocationData.ProjectileHitPositions;
		RulePerformAbility rulePerformAbility2 = rulePerformAbility;
		Rulebook.Trigger(rulePerformAbility2);
		ExecutionProcess = rulePerformAbility2.Result;
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
		if (!rulePerformAbility2.Success)
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
			h.VisualWeaponStateChangeHandle(VFXSpeedUpdater.WeaponVisualState.InHand, (!(base.Executor?.View != null)) ? null : base.Executor?.View.HandsEquipment?.GetWeaponModel(offHand: false));
		});
		if (base.Executor?.View != null)
		{
			base.Executor.View.HideOffWeapon(hide: false);
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
