using System;
using System.Collections.Generic;
using System.Linq;
using Code.Visual.Animation;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.FlagCountable;
using Kingmaker.View;
using Kingmaker.View.Animation;
using Kingmaker.View.Covers;
using Kingmaker.View.Equipment;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.Visual.Animation.Actions;
using Kingmaker.Visual.Animation.CustomIdleComponents;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using Kingmaker.Visual.CharacterSystem.Dismemberment;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;
using UnityEngine.Playables;

namespace Kingmaker.Visual.Animation.Kingmaker;

public class UnitAnimationManager : AnimationManager, IEntitySubscriber, IUnitCombatHandler<EntitySubscriber>, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IUnitCombatHandler, EntitySubscriber>
{
	private enum ExclusiveStateType
	{
		None,
		Prone,
		StandUp,
		Disabled,
		Dead,
		ExitingDisabled,
		Climb,
		Leap
	}

	private static readonly LogChannel Logger = PFLog.Animations;

	public Queue<Transform> AimIKTargetsQueue = new Queue<Transform>();

	private bool m_NecessaryHandlesInitialized;

	[CanBeNull]
	private UnitAnimationActionHandle m_LocoMotionHandle;

	[CanBeNull]
	private UnitAnimationActionHandle m_CoverHandle;

	[CanBeNull]
	private UnitAnimationActionHandle m_TurnAroundHandle;

	[CanBeNull]
	private UnitAnimationActionHandle m_MoodHandle;

	[CanBeNull]
	private UnitAnimationActionHandle m_WeaponWieldingHandle;

	[CanBeNull]
	private UnitAnimationActionHandle m_CurrentMainHandAttackForPrepare;

	private ExclusiveStateType m_ExclusiveState;

	[CanBeNull]
	private UnitAnimationActionHandle m_ExclusiveHandle;

	private UnitAnimationDecoratorManager m_DecoratorManager;

	private bool m_IsAnimating = true;

	private bool m_InCutsceneCoverAvailable;

	private bool m_BlockAttackAnimation;

	private Playable m_OldSource;

	private WeaponType m_MainHandWeaponType;

	private WeaponType m_OffHandWeaponType;

	private WeaponAnimationStyle m_ActiveWeaponStyle;

	private bool m_IsInCombat;

	public readonly CountableFlag HitAnimationIsActive = new CountableFlag();

	private LosCalculations.CoverType m_CoverType;

	public AbstractUnitEntityView View { get; private set; }

	public bool HasView => View != null;

	public BlueprintRace AnimationRace { get; private set; }

	public bool IsStopping { get; set; }

	public bool IsInCombat
	{
		get
		{
			return m_IsInCombat;
		}
		set
		{
			if (m_IsInCombat != value)
			{
				m_IsInCombat = value;
				UpdateActiveWeaponStyle();
			}
		}
	}

	public WeaponType ActiveMainHandWeaponType
	{
		get
		{
			return m_MainHandWeaponType;
		}
		set
		{
			if (m_MainHandWeaponType != value)
			{
				m_MainHandWeaponType = value;
				UpdateActiveWeaponStyle();
			}
		}
	}

	public WeaponType ActiveOffHandWeaponType
	{
		get
		{
			return m_OffHandWeaponType;
		}
		set
		{
			if (m_OffHandWeaponType != value)
			{
				m_OffHandWeaponType = value;
				UpdateActiveWeaponStyle();
			}
		}
	}

	public WeaponAnimationStyle ActiveWeaponStyle => m_ActiveWeaponStyle;

	public float Speed { get; set; }

	public float NewSpeed { get; set; }

	public float Orientation { get; set; }

	public float DesiredOrientation { get; set; }

	public List<AnimationClipWrapper> CustomIdleWrappers { get; set; }

	public WalkSpeedType WalkSpeedType { get; set; }

	public bool IsDead { get; set; }

	public bool IsProne { get; set; }

	public bool IsSleeping { get; set; }

	public bool IsUnconscious { get; set; }

	public bool IsDisabled { get; set; }

	public bool IsClimbing { get; private set; }

	public bool IsLeaping { get; private set; }

	public bool PreviousInCombat { get; set; }

	public bool IsMechadendrite { get; set; }

	public bool IsMoodMaskCanBeApplied { get; set; }

	public bool BlockAttackAnimation
	{
		get
		{
			if (!Mathf.Approximately(DesiredOrientation, Orientation))
			{
				return true;
			}
			if (m_CoverHandle != null && NeedStepOut)
			{
				return m_BlockAttackAnimation;
			}
			return false;
		}
		set
		{
			m_BlockAttackAnimation = value;
		}
	}

	public LosCalculations.CoverType CoverType
	{
		get
		{
			return m_CoverType;
		}
		set
		{
			m_CoverType = value;
		}
	}

	public UnitAnimationActionCover.StepOutDirectionAnimationType StepOutDirectionAnimationType { get; set; }

	public bool AbilityIsSpell { get; set; }

	public float UseAbilityDirection { get; set; }

	public bool InCutsceneCoverAvailable
	{
		get
		{
			if (Game.Instance.CurrentModeType == GameModeType.Cutscene || (View is UnitEntityView { Data: not null } unitEntityView && unitEntityView.Data.Body.InCombatVisual && !unitEntityView.Data.IsInCombat))
			{
				return m_InCutsceneCoverAvailable;
			}
			m_InCutsceneCoverAvailable = false;
			return true;
		}
		set
		{
			m_InCutsceneCoverAvailable = value;
		}
	}

	public bool IsWaitingForIncomingAttackOfOpportunity { get; set; }

	public bool InCover
	{
		get
		{
			if (IsInCombat && InCutsceneCoverAvailable && HasView && View is UnitEntityView unitEntityView && (unitEntityView.Data == null || unitEntityView.Data != Game.Instance.Controllers.TurnController.CurrentUnit || unitEntityView.Data.IsInPlayerParty) && !unitEntityView.AgentASP.IsCharging)
			{
				UnitViewHandsEquipment handsEquipment = unitEntityView.HandsEquipment;
				if (handsEquipment == null || !handsEquipment.AreHandsBusyWithAnimation.Value)
				{
					return CoverType != LosCalculations.CoverType.Obstacle;
				}
			}
			return false;
		}
	}

	public bool NeedStepOut
	{
		get
		{
			if (IsInCombat && InCutsceneCoverAvailable && HasView && View.Data?.Commands.Current is UnitUseAbility unitUseAbility && unitUseAbility.Target != unitUseAbility.Executor)
			{
				return CoverType == LosCalculations.CoverType.LosBlocker;
			}
			return false;
		}
	}

	public UnitAnimationDecoratorManager DecoratorManager => m_DecoratorManager ?? (m_DecoratorManager = new UnitAnimationDecoratorManager(this));

	public bool IsAnimating
	{
		get
		{
			return m_IsAnimating;
		}
		set
		{
			if (m_IsAnimating != value)
			{
				m_IsAnimating = value;
				base.Disabled = !m_IsAnimating;
			}
		}
	}

	public bool HasActedAnimation => base.ActiveActions.Any((AnimationActionHandle a) => a.IsActed);

	public bool IsGoingProne
	{
		get
		{
			if (m_ExclusiveHandle?.Action is UnitAnimationActionProne unitAnimationActionProne)
			{
				return !unitAnimationActionProne.IsActuallyProne(m_ExclusiveHandle);
			}
			return false;
		}
	}

	public UnitAnimationActionHandle ExclusiveHandle => m_ExclusiveHandle;

	public UnitAnimationActionHandle CurrentMainHandAttackForPrepare => m_CurrentMainHandAttackForPrepare;

	public bool IsGoingCover => ((UnitAnimationActionCover)(m_CoverHandle?.Action))?.IsCoverForceExitingFinished(m_CoverHandle) ?? false;

	public bool IsPreventingMovement => m_ExclusiveState switch
	{
		ExclusiveStateType.None => false, 
		ExclusiveStateType.Disabled => false, 
		ExclusiveStateType.Climb => false, 
		ExclusiveStateType.Leap => false, 
		ExclusiveStateType.Prone => false, 
		_ => true, 
	};

	public bool IsPreventingRotation
	{
		get
		{
			if (!IsProne && !IsStandUp && !base.IsRotationForbidden)
			{
				return IsGoingProne;
			}
			return true;
		}
	}

	public bool IsInExclusiveState => m_ExclusiveState switch
	{
		ExclusiveStateType.None => false, 
		ExclusiveStateType.Climb => false, 
		ExclusiveStateType.Leap => false, 
		_ => true, 
	};

	public CombatMicroIdle CombatMicroIdle { get; set; }

	public UnitAnimationActionHandle CurrentEquipHandle { get; set; }

	public bool IsStandUp => m_ExclusiveState == ExclusiveStateType.StandUp;

	public UnitAnimationActionHandle LocoMotionHandle => m_LocoMotionHandle;

	public void AttachToView(AbstractUnitEntityView view, BlueprintRace animationRace)
	{
		View = view;
		AnimationRace = animationRace;
		if (View != null)
		{
			CustomIdleAnimationBlueprintComponent component = View.Blueprint.GetComponent<CustomIdleAnimationBlueprintComponent>();
			if (component != null)
			{
				List<AnimationClipWrapper> idleClips = component.IdleClips;
				if (idleClips != null && idleClips.Count > 0)
				{
					CustomIdleWrappers = component.IdleClips;
				}
			}
		}
		PreviousInCombat = IsInCombat;
	}

	protected override void OnAnimationSetChanged()
	{
		m_NecessaryHandlesInitialized = false;
	}

	public void ChangeLocoMotion(UnitAnimationActionLocomotion action)
	{
		m_LocoMotionHandle = (UnitAnimationActionHandle)CreateHandle(action);
		if (m_LocoMotionHandle != null)
		{
			Execute(m_LocoMotionHandle);
			m_LocoMotionHandle.ActiveAnimation.Update(1f, 1f);
		}
	}

	internal void ResetLocoMotion()
	{
		if (m_LocoMotionHandle != null)
		{
			m_LocoMotionHandle.Release();
			RemoveActionHandle(m_LocoMotionHandle);
			m_LocoMotionHandle = null;
		}
	}

	private void TryInitNecessaryHandles()
	{
		if (!m_NecessaryHandlesInitialized)
		{
			m_NecessaryHandlesInitialized = true;
			if (IsInDollRoom)
			{
				TryInitDollRoomLocoMotion();
				return;
			}
			TryInitLocoMotion();
			TryInitCover();
			TryInitTurnAround();
			TryInitMood();
			TryInitWeaponWielding();
		}
	}

	private void TryInitLocoMotion()
	{
		m_LocoMotionHandle = CreateHandle(UnitAnimationType.LocoMotion);
		if (m_LocoMotionHandle != null)
		{
			Execute(m_LocoMotionHandle);
			m_LocoMotionHandle.ActiveAnimation?.Update(1f, 1f);
		}
	}

	private void TryInitDollRoomLocoMotion()
	{
		m_LocoMotionHandle = CreateHandle(UnitAnimationType.DollRoomLocoMotion) ?? CreateHandle(UnitAnimationType.LocoMotion);
		if (m_LocoMotionHandle != null)
		{
			Execute(m_LocoMotionHandle);
			m_LocoMotionHandle.ActiveAnimation?.Update(1f, 1f);
		}
	}

	private void TryInitCover()
	{
		m_CoverHandle = CreateHandle(UnitAnimationType.Cover);
		if (m_CoverHandle != null)
		{
			Execute(m_CoverHandle);
		}
	}

	private void TryInitTurnAround()
	{
		m_TurnAroundHandle = CreateHandle(UnitAnimationType.TurnAround);
		if (m_TurnAroundHandle != null)
		{
			Execute(m_TurnAroundHandle);
		}
	}

	private void TryInitMood()
	{
		m_MoodHandle = CreateHandle(UnitAnimationType.Mood);
		if (m_MoodHandle != null)
		{
			Execute(m_MoodHandle);
		}
	}

	private void TryInitWeaponWielding()
	{
		m_WeaponWieldingHandle = CreateHandle(UnitAnimationType.WeaponWielding);
		if (m_WeaponWieldingHandle != null)
		{
			Execute(m_WeaponWieldingHandle);
		}
	}

	public void Tick(float deltaTime)
	{
		using (ProfileScope.New("UnitAnimationManager.Tick()"))
		{
			try
			{
				TickInternal(deltaTime);
			}
			catch (Exception ex)
			{
				Logger.Exception(this, ex);
			}
		}
	}

	private void TickInternal(float deltaTime)
	{
		if (base.AnimationSet == null)
		{
			return;
		}
		TryInitNecessaryHandles();
		ProcessCoverAction();
		if (IsDead && !IsMechadendrite)
		{
			SetExclusiveAnimation(ExclusiveStateType.Dead);
			return;
		}
		if (IsProne && !IsMechadendrite)
		{
			SetExclusiveAnimation(ExclusiveStateType.Prone);
			return;
		}
		if (IsDisabled)
		{
			SetExclusiveAnimation(ExclusiveStateType.Disabled);
			return;
		}
		if (m_ExclusiveState == ExclusiveStateType.Prone || m_ExclusiveState == ExclusiveStateType.Dead)
		{
			SetExclusiveAnimation(ExclusiveStateType.StandUp);
			return;
		}
		if (m_ExclusiveState == ExclusiveStateType.Disabled)
		{
			SetExclusiveAnimation(ExclusiveStateType.ExitingDisabled);
			return;
		}
		if (IsClimbing)
		{
			SetExclusiveAnimation(ExclusiveStateType.Climb);
			return;
		}
		if (IsLeaping)
		{
			SetExclusiveAnimation(ExclusiveStateType.Leap);
			return;
		}
		UnitAnimationActionHandle exclusiveHandle = m_ExclusiveHandle;
		if (exclusiveHandle == null || exclusiveHandle.IsReleased)
		{
			SetExclusiveAnimation(ExclusiveStateType.None);
		}
	}

	private void ProcessCoverAction()
	{
		if (m_CoverHandle?.Action is UnitAnimationActionCover unitAnimationActionCover && !(View == null))
		{
			if (!InCover)
			{
				unitAnimationActionCover.ExitCover(m_CoverHandle);
			}
			else if (NeedStepOut && !HasMovingCommand(View.Data) && StepOutDirectionAnimationType != 0)
			{
				unitAnimationActionCover.DoSideStep(m_CoverHandle);
			}
			else if (HasMovingCommand(View.Data))
			{
				unitAnimationActionCover.ExitCover(m_CoverHandle);
			}
			else if (View is UnitEntityView unitEntityView && (unitEntityView.HandsEquipment == null || !unitEntityView.HandsEquipment.AreHandsBusyWithAnimation) && !HasMovingCommand(View.Data) && !View.MovementAgent.IsReallyMoving)
			{
				unitAnimationActionCover.EnterCover(m_CoverHandle);
			}
		}
	}

	public static bool HasMovingCommand([CanBeNull] PartUnitCommands.IOwner unit)
	{
		return unit?.Commands.Contains((AbstractUnitCommand x) => x.IsMoveUnit) ?? false;
	}

	private void SetExclusiveAnimation(ExclusiveStateType state)
	{
		if (state == m_ExclusiveState)
		{
			return;
		}
		if (m_ExclusiveState == ExclusiveStateType.StandUp)
		{
			UnitAnimationActionHandle exclusiveHandle = m_ExclusiveHandle;
			if (exclusiveHandle != null && !exclusiveHandle.IsReleased)
			{
				return;
			}
		}
		if (state != ExclusiveStateType.StandUp && state != ExclusiveStateType.ExitingDisabled)
		{
			m_ExclusiveHandle?.Release();
		}
		switch (state)
		{
		case ExclusiveStateType.Prone:
			m_ExclusiveHandle = Execute(UnitAnimationType.Prone);
			break;
		case ExclusiveStateType.StandUp:
		{
			UnitAnimationActionProne unitAnimationActionProne = m_ExclusiveHandle?.Action as UnitAnimationActionProne;
			if ((bool)unitAnimationActionProne)
			{
				unitAnimationActionProne.SwitchToExit(m_ExclusiveHandle);
				break;
			}
			m_ExclusiveHandle?.Release();
			state = ExclusiveStateType.None;
			break;
		}
		case ExclusiveStateType.Disabled:
			m_ExclusiveHandle = Execute(UnitAnimationType.Disabled);
			break;
		case ExclusiveStateType.Dead:
			ResetTransitionsAndActions();
			m_ExclusiveHandle = CreateHandle(UnitAnimationType.Death);
			if (m_ExclusiveHandle != null)
			{
				Execute(m_ExclusiveHandle);
			}
			else
			{
				HandleDeathWithoutAnimation();
			}
			break;
		case ExclusiveStateType.ExitingDisabled:
		{
			UnitAnimationActionDisabled unitAnimationActionDisabled = m_ExclusiveHandle?.Action as UnitAnimationActionDisabled;
			if ((bool)unitAnimationActionDisabled)
			{
				unitAnimationActionDisabled.SwitchToExit(m_ExclusiveHandle);
				break;
			}
			m_ExclusiveHandle?.Release();
			state = ExclusiveStateType.None;
			break;
		}
		case ExclusiveStateType.Climb:
			m_ExclusiveHandle = Execute(UnitAnimationType.Climb);
			break;
		case ExclusiveStateType.Leap:
			m_ExclusiveHandle = Execute(UnitAnimationType.Leap);
			break;
		default:
			throw new ArgumentOutOfRangeException("state", state, null);
		case ExclusiveStateType.None:
			break;
		}
		if (m_ExclusiveState != state)
		{
			string arg = ((View == null) ? "View is null" : ((View.EntityData == null) ? "View.EntityData is null" : $"{View.EntityData}"));
			Logger.Log($"{arg}: {m_ExclusiveState} -> {state}");
		}
		m_ExclusiveState = state;
	}

	[CanBeNull]
	public UnitAnimationAction GetAction(UnitAnimationType type)
	{
		return base.AnimationSet.GetAction(type);
	}

	protected override void UpdateAnimations(float dt)
	{
		if (!HasView || !(View.EntityData?.ControlledByDirector))
		{
			base.UpdateAnimations(dt);
			m_DecoratorManager?.Update(dt);
		}
	}

	public override AnimationActionHandle CreateHandle(AnimationActionBase animationAction)
	{
		if (!animationAction)
		{
			Logger.Error(this, "Animation action is null");
			return null;
		}
		UnitAnimationAction unitAnimationAction = animationAction as UnitAnimationAction;
		if (!unitAnimationAction)
		{
			Logger.Error(this, "{0} doesn't support actions of type {1}", this, animationAction.GetType().Name);
			return null;
		}
		return new UnitAnimationActionHandle(unitAnimationAction, this);
	}

	public UnitAnimationActionHandle CreateHandle(UnitAnimationType type, bool errorOnEmpty = true)
	{
		UnitAnimationAction action = GetAction(type);
		if (!action)
		{
			return null;
		}
		return (UnitAnimationActionHandle)CreateHandle(action);
	}

	public UnitAnimationActionHandle Execute(UnitAnimationType type)
	{
		UnitAnimationActionHandle unitAnimationActionHandle = CreateHandle(type, errorOnEmpty: false);
		if (unitAnimationActionHandle != null)
		{
			Execute(unitAnimationActionHandle);
		}
		return unitAnimationActionHandle;
	}

	public void CreateMainHandAttackHandlerForPrepare()
	{
		m_CurrentMainHandAttackForPrepare?.Release();
		m_CurrentMainHandAttackForPrepare = CreateHandle(UnitAnimationType.Attack);
		if (m_CurrentMainHandAttackForPrepare != null)
		{
			m_CurrentMainHandAttackForPrepare.NeedPreparingForShooting = true;
			m_CurrentMainHandAttackForPrepare.IsPreparingForShooting = true;
			base.Execute(m_CurrentMainHandAttackForPrepare);
		}
	}

	public override void Execute(AnimationActionHandle handle)
	{
		TryInitNecessaryHandles();
		if (CanExecute(handle))
		{
			if (IsSleeping)
			{
				PFLog.Default.Warning($"Trying to Execute animation action on a sleeping unit, UnitAnimationManager={this}");
			}
			base.Execute(handle);
		}
	}

	private bool CanExecute(AnimationActionHandle handle)
	{
		UnitAnimationAction unitAnimationAction = handle?.Action as UnitAnimationAction;
		if (!unitAnimationAction)
		{
			return true;
		}
		UnitAnimationType type = unitAnimationAction.Type;
		if (type == UnitAnimationType.Defence || type == UnitAnimationType.Hit)
		{
			if (!IsWaitingForIncomingAttackOfOpportunity && !IsProne)
			{
				return !IsGoingProne;
			}
			return false;
		}
		return true;
	}

	public bool CanRunIdleAction()
	{
		if (NewSpeed > 0f)
		{
			return false;
		}
		if (CutsceneLock.Active)
		{
			return false;
		}
		if (HasView && (bool)View.EntityData?.ControlledByDirector)
		{
			return false;
		}
		return base.ActiveActions.All((AnimationActionHandle h) => h == m_LocoMotionHandle || h == m_CoverHandle || h.IsAdditive || h.IsReleased);
	}

	public void OnCommandActEvent()
	{
		UnitUseAbility unitUseAbility = (UnitUseAbilityAbstract<UnitUseAbilityParams>.TestPauseOnCast ? (View.Data?.Commands.Current as UnitUseAbility) : null);
		foreach (AnimationActionHandle activeAction in base.ActiveActions)
		{
			activeAction.ActEventsCounter++;
			if (unitUseAbility != null && unitUseAbility.Animation == activeAction && unitUseAbility.Executor.Faction.IsPlayer && unitUseAbility.Ability.TargetAnchor != 0)
			{
				Game.Instance.IsPaused = true;
				Game.Instance.Controllers.SelectedAbilityHandler.SetAbility(unitUseAbility.Ability);
				string notification = "Ready to resolve: " + unitUseAbility.Ability.Blueprint.Name;
				EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler l)
				{
					l.HandleWarning(notification);
				});
			}
		}
	}

	public void FastForwardProneAnimation(bool forceDeadFromProne = false)
	{
		if (m_ExclusiveState != ExclusiveStateType.Prone && m_ExclusiveState != ExclusiveStateType.Dead)
		{
			SetExclusiveAnimation(ExclusiveStateType.Prone);
		}
		if (forceDeadFromProne && m_ExclusiveHandle != null)
		{
			m_ExclusiveHandle.DeathFromProne = true;
		}
		UnitAnimationActionProne unitAnimationActionProne = m_ExclusiveHandle?.Action as UnitAnimationActionProne;
		if ((bool)unitAnimationActionProne)
		{
			unitAnimationActionProne.FastForward(m_ExclusiveHandle);
		}
		else
		{
			Logger.Warning(this, $"{this} cannot fast-forward prone animation: not prone ({m_ExclusiveState})");
		}
	}

	public void FastForwardDeathAnimation()
	{
		if (m_ExclusiveState != ExclusiveStateType.Prone && m_ExclusiveState != ExclusiveStateType.Dead)
		{
			SetExclusiveAnimation(ExclusiveStateType.Dead);
		}
		UnitAnimationActionDeath unitAnimationActionDeath = m_ExclusiveHandle?.Action as UnitAnimationActionDeath;
		if ((bool)unitAnimationActionDeath)
		{
			unitAnimationActionDeath.FastForward(m_ExclusiveHandle);
			return;
		}
		Logger.Warning(this, "{0} cannot fast-forward prone animation: not prone ({1})", this, m_ExclusiveState);
	}

	private void HandleDeathWithoutAnimation()
	{
		base.Animator.enabled = false;
		StopEvents();
		DismembermentHandler.UseWithoutAnimationDeath(View.Data);
	}

	public void StandUpImmediately()
	{
		if (m_ExclusiveState == ExclusiveStateType.Prone || m_ExclusiveState == ExclusiveStateType.Dead || m_ExclusiveState == ExclusiveStateType.StandUp)
		{
			m_ExclusiveHandle?.Release(0f);
			SetExclusiveAnimation(ExclusiveStateType.None);
		}
	}

	public void StartClimb()
	{
		IsClimbing = true;
		SetExclusiveAnimation(ExclusiveStateType.Climb);
	}

	public void StopClimb()
	{
		IsClimbing = false;
	}

	public void StartLeap()
	{
		IsLeaping = true;
		SetExclusiveAnimation(ExclusiveStateType.Leap);
	}

	public void StopLeap()
	{
		IsLeaping = false;
	}

	protected override void OnDisable()
	{
		if (View is UnitEntityView unitEntityView)
		{
			unitEntityView.HandsEquipment?.ForceEndChangeEquipment();
		}
		base.OnDisable();
	}

	public IEntity GetSubscribingEntity()
	{
		return View.EntityData;
	}

	public void HandleUnitJoinCombat()
	{
		PrepareForCombat();
	}

	public void HandleUnitLeaveCombat()
	{
	}

	public void UpdateActiveWeaponStyle()
	{
		m_ActiveWeaponStyle = (m_IsInCombat ? (m_MainHandWeaponType.IsTwoHanded() ? WeaponAnimationStyleHelper.DetectTwoHandedWeaponStyle(m_MainHandWeaponType) : WeaponAnimationStyleHelper.DetectDualWieldingStyle(m_MainHandWeaponType, m_OffHandWeaponType)) : WeaponAnimationStyle.NonCombat);
	}
}
