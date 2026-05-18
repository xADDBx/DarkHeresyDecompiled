using System;
using System.Collections.Generic;
using System.Linq;
using Animancer;
using Code.Visual.Animation;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Gameplay.Parts;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.FlagCountable;
using Kingmaker.Utility.Random;
using Kingmaker.Utility.StatefulRandom;
using Kingmaker.View;
using Kingmaker.View.Covers;
using Kingmaker.View.Equipment;
using Kingmaker.View.Mechadendrites;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.Visual.Animation.Actions;
using Kingmaker.Visual.Animation.CustomIdleComponents;
using Kingmaker.Visual.Animation.Decorators;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using Kingmaker.Visual.CharacterSystem.Dismemberment;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker;

public class UnitAnimationManager : AnimationManager
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

	[NonSerialized]
	public bool IsInDollRoom;

	[SerializeField]
	private AnimationSet m_AnimationSet;

	private DirectionalMixerState m_LocomotionMixerState;

	public List<Vector3> AimIKTargets = new List<Vector3>();

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

	private ExclusiveStateType m_ExclusiveState;

	private UnitAnimationDecoratorManager m_DecoratorManager;

	private List<IDecoratorVisibilityRequest> m_DecoratorVisibilityRequests = new List<IDecoratorVisibilityRequest>();

	private bool m_IsAnimating = true;

	private bool m_InCutsceneCoverAvailable;

	private WeaponType m_MainHandWeaponType;

	private WeaponType m_OffHandWeaponType;

	private bool m_IsInCombat;

	public readonly CountableFlag HitAnimationIsActive = new CountableFlag();

	public override StatefulRandom StatefulRandom
	{
		get
		{
			if (!IsInDollRoom)
			{
				return PFStatefulRandom.Visuals.Animation3;
			}
			return PFStatefulRandom.Visuals.DollRoom;
		}
	}

	private AnimancerLayer m_LocomotionLayer => m_Animancer.Layers[0];

	public AnimationSet AnimationSet
	{
		get
		{
			return m_AnimationSet;
		}
		set
		{
			UpdateAnimationSet(value);
		}
	}

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
				if (m_IsInCombat)
				{
					ReleaseDecorators();
				}
				else
				{
					AddDecorators();
				}
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

	public WeaponAnimationStyle ActiveWeaponStyle { get; private set; }

	public float Speed { get; set; }

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

	public bool BlockAttackAnimation => !Mathf.Approximately(DesiredOrientation, Orientation);

	public LosCalculations.CoverType CoverType { get; set; }

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
			if (IsInCombat && InCutsceneCoverAvailable && HasView && View is UnitEntityView unitEntityView && (unitEntityView.Data == null || unitEntityView.Data != Game.Instance.Controllers.TurnController.CurrentUnit || unitEntityView.Data.IsInPlayerParty))
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
			if (ExclusiveHandle?.Action is UnitAnimationActionProne unitAnimationActionProne)
			{
				return !unitAnimationActionProne.IsActuallyProne(ExclusiveHandle);
			}
			return false;
		}
	}

	[CanBeNull]
	public UnitAnimationActionHandle ExclusiveHandle { get; private set; }

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

	public bool HasBallisticMechadendrite { get; set; }

	public bool IsStandUp => m_ExclusiveState == ExclusiveStateType.StandUp;

	protected override void OnInitialize()
	{
		CreateLayers();
		InitializeAnimationSet();
	}

	protected override void OnBeforeDisabled()
	{
		if (View is UnitEntityView unitEntityView)
		{
			unitEntityView.HandsEquipment?.ForceEndChangeEquipment();
		}
	}

	private void CreateLayers()
	{
		int num = 11;
		for (int i = 0; i < num; i++)
		{
			_ = m_Animancer.Layers[i];
		}
		m_LocomotionLayer.SetWeight(1f);
		m_LocomotionMixerState = new DirectionalMixerState();
		m_LocomotionLayer.Play(m_LocomotionMixerState);
		m_Animancer.Layers[4].IsAdditive = true;
		m_Animancer.Layers[9].IsAdditive = true;
	}

	private void UpdateAnimationSet(AnimationSet animationSet)
	{
		if (!(m_AnimationSet == animationSet))
		{
			ReleaseDecorators();
			m_AnimationSet = animationSet;
			AddDecorators();
			ResetTransitionsAndActions();
			if (!(m_AnimationSet == null))
			{
				m_NecessaryHandlesInitialized = false;
				InitializeAnimationSet();
				TryInitNecessaryHandles();
				RestartBuffLoopActions();
			}
		}
	}

	private void AddDecorators()
	{
		UnitAnimationDecoratorObject[] array = m_AnimationSet.Decorators.EmptyIfNull();
		foreach (UnitAnimationDecoratorObject decorator in array)
		{
			IDecoratorVisibilityRequest item = DecoratorManager.ShowDecorator(decorator, m_AnimationSet);
			m_DecoratorVisibilityRequests.Add(item);
		}
	}

	private void ReleaseDecorators()
	{
		foreach (IDecoratorVisibilityRequest decoratorVisibilityRequest in m_DecoratorVisibilityRequests)
		{
			decoratorVisibilityRequest.Release();
		}
		m_DecoratorVisibilityRequests.Clear();
	}

	private void InitializeAnimationSet()
	{
		if (m_AnimationSet == null)
		{
			return;
		}
		m_Animancer.Transitions = m_AnimationSet.TransitionLibrary;
		foreach (AnimationActionBase item in m_AnimationSet.Actions.Where((AnimationActionBase a) => a))
		{
			if (item is UnitAnimationActionLocomotion unitAnimationActionLocomotion)
			{
				unitAnimationActionLocomotion.PreloadWeaponStyles();
			}
		}
	}

	private void RestartBuffLoopActions()
	{
		(View?.Data?.GetOptional<PartBuffAnimation>())?.RestartAnimations();
	}

	public void AttachToView(AbstractUnitEntityView view, BlueprintRace animationRace)
	{
		View = view;
		AnimationRace = animationRace;
		PreviousInCombat = IsInCombat;
		if (View == null)
		{
			return;
		}
		CustomIdleAnimationBlueprintComponent component = View.Blueprint.GetComponent<CustomIdleAnimationBlueprintComponent>();
		if (component != null)
		{
			List<AnimationClipWrapper> idleClips = component.IdleClips;
			if (idleClips != null && idleClips.Count > 0)
			{
				CustomIdleWrappers = component.IdleClips;
			}
		}
		UpdateBallisticMechadendriteAvailability();
	}

	public void UpdateBallisticMechadendriteAvailability()
	{
		if (!IsMechadendrite && !(View == null) && View.Data != null)
		{
			HasBallisticMechadendrite = View.Data.HasMechadendriteOfType(MechadendritesType.Ballistic);
			UpdateActiveWeaponStyle();
		}
	}

	public void ChangeLocoMotion(UnitAnimationActionLocomotion action)
	{
		ResetLocoMotion();
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

	public void UpdateLocomotionMixerAnimations([NotNull] LocomotionMixerAnimations animations)
	{
		m_LocomotionMixerState.DestroyChildren();
		foreach (var item in animations.Where(((AnimationClipWrapper ClipWrapper, Vector2 Threshold) entry) => ObjectExtensions.Or(entry.ClipWrapper, null)?.AnimationClip != null))
		{
			m_LocomotionMixerState.Add(item.ClipWrapper.AnimationClip, item.Threshold);
		}
		if (animations.ClipWithFootstepEvents != null)
		{
			SetupAnimationEvents(animations.ClipWithFootstepEvents, ClipDurationType.Endless, m_LocomotionMixerState);
		}
		m_LocomotionMixerState.DontSynchronize(m_LocomotionMixerState.GetChild(0));
	}

	public void UpdateLocomotionParameters(Vector2 faceDirection, Vector2 moveDirection, float speed, float maxSpeed)
	{
		Vector2 normalized = faceDirection.normalized;
		Vector2 normalized2 = moveDirection.normalized;
		float x = Vector2.Dot(normalized, normalized2);
		float y = Vector2.Dot(Vector2.Perpendicular(normalized), normalized2);
		float num = ((maxSpeed > 0f) ? (speed / maxSpeed) : 0f);
		m_LocomotionMixerState.Parameter = new Vector2(x, y) * num;
	}

	public void PlayLocomotionMixer(float fadeDuration = 0.2f)
	{
		m_LocomotionLayer.Play(m_LocomotionMixerState, fadeDuration);
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
		if (AnimationSet == null)
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
		UnitAnimationActionHandle exclusiveHandle = ExclusiveHandle;
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
			UnitAnimationActionHandle exclusiveHandle = ExclusiveHandle;
			if (exclusiveHandle != null && !exclusiveHandle.IsReleased)
			{
				return;
			}
		}
		if (state != ExclusiveStateType.StandUp && state != ExclusiveStateType.ExitingDisabled)
		{
			ExclusiveHandle?.Release();
		}
		switch (state)
		{
		case ExclusiveStateType.Prone:
			ExclusiveHandle = Execute(UnitAnimationType.Prone);
			break;
		case ExclusiveStateType.StandUp:
		{
			UnitAnimationActionProne unitAnimationActionProne = ExclusiveHandle?.Action as UnitAnimationActionProne;
			if ((bool)unitAnimationActionProne)
			{
				unitAnimationActionProne.SwitchToExit(ExclusiveHandle);
				break;
			}
			ExclusiveHandle?.Release();
			state = ExclusiveStateType.None;
			break;
		}
		case ExclusiveStateType.Disabled:
			ExclusiveHandle = Execute(UnitAnimationType.Disabled);
			break;
		case ExclusiveStateType.Dead:
			ExclusiveHandle = CreateHandle(UnitAnimationType.Death);
			if (ExclusiveHandle != null)
			{
				Execute(ExclusiveHandle);
			}
			else
			{
				HandleDeathWithoutAnimation();
			}
			break;
		case ExclusiveStateType.ExitingDisabled:
		{
			UnitAnimationActionDisabled unitAnimationActionDisabled = ExclusiveHandle?.Action as UnitAnimationActionDisabled;
			if ((bool)unitAnimationActionDisabled)
			{
				unitAnimationActionDisabled.SwitchToExit(ExclusiveHandle);
				break;
			}
			ExclusiveHandle?.Release();
			state = ExclusiveStateType.None;
			break;
		}
		case ExclusiveStateType.Climb:
			ExclusiveHandle = Execute(UnitAnimationType.Climb);
			break;
		case ExclusiveStateType.Leap:
			ExclusiveHandle = Execute(UnitAnimationType.Leap);
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
		return AnimationSet.GetAction(type);
	}

	protected override void UpdateAnimations(float dt)
	{
		if (!HasView || !(View.EntityData?.ControlledByDirector))
		{
			base.UpdateAnimations(dt);
			m_DecoratorManager?.Update(dt);
		}
	}

	protected override AnimationActionHandle CreateHandle(AnimationActionBase animationAction)
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

	public override bool TryExecute(AnimationActionBase animationAction, Action<AnimationActionHandle> initializer, out AnimationActionHandle handle)
	{
		handle = CreateHandle(animationAction);
		return TryExecuteInternal((UnitAnimationActionHandle)handle, initializer);
	}

	public bool TryExecute(UnitAnimationType type)
	{
		UnitAnimationActionHandle handle = CreateHandle(type);
		return TryExecuteInternal(handle, null);
	}

	public bool TryExecute(UnitAnimationType type, Action<UnitAnimationActionHandle> initializer, out UnitAnimationActionHandle handle)
	{
		handle = CreateHandle(type);
		return TryExecuteInternal(handle, initializer);
	}

	private bool TryExecuteInternal(UnitAnimationActionHandle handle, Action<UnitAnimationActionHandle> initializer)
	{
		if (handle == null)
		{
			return false;
		}
		initializer?.Invoke(handle);
		Execute(handle);
		if (IsMechadendrite)
		{
			return true;
		}
		if (!(View is UnitEntityView { Data: not null } unitEntityView))
		{
			return true;
		}
		UnitPartMechadendrites optional = unitEntityView.Data.GetOptional<UnitPartMechadendrites>();
		if (optional == null)
		{
			return true;
		}
		bool flag = true;
		UnitAnimationType type = handle.Action.Type;
		foreach (MechadendriteSettings value in optional.Mechadendrites.Values)
		{
			UnitAnimationManager animationManager = value.AnimationManager;
			if ((object)animationManager != null)
			{
				flag &= animationManager.TryExecute(type, initializer, out var _);
			}
		}
		return flag;
	}

	private UnitAnimationActionHandle CreateHandle(UnitAnimationType type)
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
		UnitAnimationActionHandle unitAnimationActionHandle = CreateHandle(type);
		if (unitAnimationActionHandle != null)
		{
			Execute(unitAnimationActionHandle);
		}
		return unitAnimationActionHandle;
	}

	protected override void Execute(AnimationActionHandle handle)
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
		if (Speed > 0f)
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
		return base.ActiveActions.All((AnimationActionHandle h) => h == m_LocoMotionHandle || h == m_CoverHandle || h.IsReleased);
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
		if (forceDeadFromProne && ExclusiveHandle != null)
		{
			ExclusiveHandle.DeathFromProne = true;
		}
		UnitAnimationActionProne unitAnimationActionProne = ExclusiveHandle?.Action as UnitAnimationActionProne;
		if ((bool)unitAnimationActionProne)
		{
			unitAnimationActionProne.FastForward(ExclusiveHandle);
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
		UnitAnimationActionDeath unitAnimationActionDeath = ExclusiveHandle?.Action as UnitAnimationActionDeath;
		if ((bool)unitAnimationActionDeath)
		{
			unitAnimationActionDeath.FastForward(ExclusiveHandle);
			return;
		}
		Logger.Warning(this, "{0} cannot fast-forward prone animation: not prone ({1})", this, m_ExclusiveState);
	}

	public void HandleDeathWithoutAnimation()
	{
		base.Disabled = true;
		DismembermentHandler.UseWithoutAnimationDeath(View.Data);
		base.Animator.enabled = false;
	}

	public void StandUpImmediately()
	{
		if (m_ExclusiveState == ExclusiveStateType.Prone || m_ExclusiveState == ExclusiveStateType.Dead || m_ExclusiveState == ExclusiveStateType.StandUp)
		{
			ExclusiveHandle?.Release(0f);
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

	public IEntity GetSubscribingEntity()
	{
		return View.EntityData;
	}

	public void UpdateActiveWeaponStyle()
	{
		WeaponType offHandWeapon = ((!HasBallisticMechadendrite) ? m_OffHandWeaponType : WeaponType.Fist);
		ActiveWeaponStyle = (m_IsInCombat ? (m_MainHandWeaponType.IsTwoHanded() ? WeaponAnimationStyleHelper.DetectTwoHandedWeaponStyle(m_MainHandWeaponType) : WeaponAnimationStyleHelper.DetectDualWieldingStyle(m_MainHandWeaponType, offHandWeapon)) : WeaponAnimationStyle.NonCombat);
	}
}
