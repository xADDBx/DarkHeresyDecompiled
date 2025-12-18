using System;
using System.Collections.Generic;
using Animancer.FSM;
using Code.Visual.Animation;
using Kingmaker.Pathfinding;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.Visual.Animation.WeaponStyles;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "UnitAnimationLocoMotion", menuName = "Animation Manager/Actions/Unit Locomotion")]
public class UnitAnimationActionLocomotion : UnitAnimationAction
{
	public class CommonLocomotionData
	{
		public float Speed;
	}

	public class ActionData
	{
		public CommonLocomotionData Data;

		public StateMachine<LocomotionStateType, LocomotionState>.WithDefault Fsm;
	}

	public enum CustomCombatWalkType
	{
		None,
		OneCell,
		OneDiagonal,
		TwoCells,
		TwoDiagonals
	}

	public class WalkingTypeData
	{
		public AnimationClipWrapper Out;

		public AnimationClipWrapper Loop;

		public MovingTypeParameters Parameters;
	}

	[Serializable]
	public class MovingTypeParameters
	{
		public float Speed;

		public float OutTransition = 0.35f;
	}

	[Serializable]
	public class LocomotionMixerAnimationsList
	{
		public AnimationClipWrapper Idle;

		public AnimationClipWrapper Walk;

		public AnimationClipWrapper WalkLeft;

		public AnimationClipWrapper WalkRight;

		public AnimationClipWrapper WalkBack;

		public AnimationClipWrapper Run;

		public AnimationClipWrapper RunLeft;

		public AnimationClipWrapper RunRight;

		public AnimationClipWrapper RunBack;

		public AnimationClipWrapper Sprint;
	}

	private const float epsilon = 0.05f;

	private static readonly List<float> DistanceList = new List<float>
	{
		0f,
		1f * GraphParamsMechanicsCache.GridCellSize + 0.05f,
		1.41f * GraphParamsMechanicsCache.GridCellSize + 0.05f,
		2f * GraphParamsMechanicsCache.GridCellSize + 0.05f,
		2.83f * GraphParamsMechanicsCache.GridCellSize + 0.05f
	};

	[SerializeField]
	private bool m_ForDollRoom;

	[Header("Variant idle trigger probability")]
	[SerializeField]
	private TimedProbabilityCurve m_TriggerProbability;

	[Header("Movement parameters")]
	public MovingTypeParameters WalkParameters;

	public MovingTypeParameters RunParameters;

	public MovingTypeParameters SprintParameters;

	public MovingTypeParameters CoverParameters;

	[Header("Common animations")]
	public bool UseCommonLocomotionAnimations;

	[ShowIf("UseCommonLocomotionAnimations")]
	public LocomotionMixerAnimationsList NonCombatAnimations;

	[ShowIf("UseCommonLocomotionAnimations")]
	public bool UseDifferentAnimationsInCombat;

	[ShowIf("UseDifferentAnimationsInCombat")]
	public LocomotionMixerAnimationsList InCombatAnimations;

	[Header("Traverse parameters")]
	public float SpeedAfterLeaped = 2f;

	public float SpeedAfterClimbedUp = 2f;

	public float SpeedAfterClimbedDown;

	[Space(25f)]
	[Header("Blending parameters")]
	[SerializeField]
	private bool m_UseCustomBlending;

	[ShowIf("m_UseCustomBlending")]
	[Range(0f, 1f)]
	public float m_WalkBlendParam = 0.17f;

	[ShowIf("m_UseCustomBlending")]
	[Range(0f, 1f)]
	public float m_RunBlendParam = 0.78f;

	[ShowIf("m_UseCustomBlending")]
	[Range(0f, 1f)]
	public float m_SprintBlendParam = 1f;

	[Space(25f)]
	[SerializeField]
	[ValidateNotNull]
	private BlueprintWeaponStyleList.Reference m_WeaponStyleSettings;

	private HashSet<AnimationClipWrapper> m_ClipWrappersHashSet;

	public bool IsForDollRoom => m_ForDollRoom;

	public TimedProbabilityCurve TriggerProbability => m_TriggerProbability;

	public override bool DontReleaseOnInterrupt => true;

	public override bool SupportCaching => true;

	private AnimationLayerType AnimationLayer => AnimationLayerType.Locomotion;

	public BlueprintWeaponStyleList WeaponStyleSettings => m_WeaponStyleSettings?.Get();

	public WeaponStyleLocomotionData NonCombatLocomotionData => WeaponStyleSettings[WeaponAnimationStyle.NonCombat].Locomotion;

	public override UnitAnimationType Type
	{
		get
		{
			if (!m_ForDollRoom)
			{
				return UnitAnimationType.LocoMotion;
			}
			return UnitAnimationType.DollRoomLocoMotion;
		}
	}

	public override IEnumerable<AnimationClipWrapper> ClipWrappers => CollectClipWrappers();

	public void PreloadWeaponStyles()
	{
		BlueprintWeaponStyleList weaponStyleSettings = WeaponStyleSettings;
		if (weaponStyleSettings == null)
		{
			return;
		}
		foreach (BlueprintWeaponStyleList.WeaponStyleEntry weaponStyle in weaponStyleSettings.WeaponStyles)
		{
			weaponStyle.Preload();
		}
	}

	private IEnumerable<AnimationClipWrapper> CollectClipWrappers()
	{
		if (m_ClipWrappersHashSet != null)
		{
			return m_ClipWrappersHashSet;
		}
		m_ClipWrappersHashSet = new HashSet<AnimationClipWrapper>();
		if (UseCommonLocomotionAnimations && !IsForDollRoom)
		{
			CollectClipWrappers(ref m_ClipWrappersHashSet, NonCombatAnimations);
			if (UseDifferentAnimationsInCombat)
			{
				CollectClipWrappers(ref m_ClipWrappersHashSet, InCombatAnimations);
			}
		}
		if (WeaponStyleSettings != null)
		{
			m_ClipWrappersHashSet.AddRange(IsForDollRoom ? WeaponStyleSettings.EnumerateDollRoomClips() : WeaponStyleSettings.EnumerateLocomotionClips());
		}
		return m_ClipWrappersHashSet;
	}

	private static void CollectClipWrappers(ref HashSet<AnimationClipWrapper> hashSet, LocomotionMixerAnimationsList data)
	{
		AddIfNotNull(ref hashSet, data.Idle);
		AddIfNotNull(ref hashSet, data.Walk);
		AddIfNotNull(ref hashSet, data.WalkLeft);
		AddIfNotNull(ref hashSet, data.WalkRight);
		AddIfNotNull(ref hashSet, data.WalkBack);
		AddIfNotNull(ref hashSet, data.Run);
		AddIfNotNull(ref hashSet, data.RunLeft);
		AddIfNotNull(ref hashSet, data.RunRight);
		AddIfNotNull(ref hashSet, data.RunBack);
		AddIfNotNull(ref hashSet, data.Sprint);
		static void AddIfNotNull(ref HashSet<AnimationClipWrapper> hashSet, AnimationClipWrapper clip)
		{
			if (clip != null)
			{
				hashSet.Add(clip);
			}
		}
	}

	public override void OnTransitionOutStarted(UnitAnimationActionHandle handle)
	{
	}

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		handle.AnimationLayer = AnimationLayer;
		handle.ActionData = new ActionData
		{
			Data = new CommonLocomotionData(),
			Fsm = CreateFsm(handle)
		};
	}

	public override void OnUpdate(UnitAnimationActionHandle handle, float deltaTime)
	{
		ActionData actionData = (ActionData)handle.ActionData;
		if (actionData != null)
		{
			StateMachine<LocomotionStateType, LocomotionState>.WithDefault fsm = actionData.Fsm;
			LocomotionStateType locomotionStateType = fsm.CurrentState.Tick(deltaTime);
			if (locomotionStateType != fsm.CurrentKey)
			{
				fsm.TrySetState(locomotionStateType);
			}
		}
	}

	private StateMachine<LocomotionStateType, LocomotionState>.WithDefault CreateFsm(UnitAnimationActionHandle handle)
	{
		StateMachine<LocomotionStateType, LocomotionState>.WithDefault withDefault = new StateMachine<LocomotionStateType, LocomotionState>.WithDefault();
		withDefault.Add(LocomotionStateType.Idle, new LocomotionIdle(this, handle));
		withDefault.Add(LocomotionStateType.Run, new LocomotionRun(this, handle));
		withDefault.Add(LocomotionStateType.Out, new LocomotionOut(this, handle));
		withDefault.Add(LocomotionStateType.Traverse, new LocomotionTraverse(this, handle));
		withDefault.DefaultKey = LocomotionStateType.Idle;
		return withDefault;
	}

	public WalkingTypeData GetWalkingTypeData(WeaponStyleLocomotionData locomotionData, WalkSpeedType walkingType)
	{
		return GetWalkingTypeData(locomotionData, walkingType, CustomCombatWalkType.None);
	}

	public WalkingTypeData GetWalkingTypeData(WeaponStyleLocomotionData locomotionData, WalkSpeedType walkingType, CustomCombatWalkType combatWalkType)
	{
		WalkingTypeData customCombatWalkData = GetCustomCombatWalkData(locomotionData, walkingType, combatWalkType);
		if (customCombatWalkData != null)
		{
			return customCombatWalkData;
		}
		switch (walkingType)
		{
		case WalkSpeedType.Walk:
			if (locomotionData.Walk != null)
			{
				return new WalkingTypeData
				{
					Out = locomotionData.WalkOut,
					Loop = locomotionData.Walk,
					Parameters = WalkParameters
				};
			}
			break;
		case WalkSpeedType.Run:
			if (locomotionData.Run != null)
			{
				return new WalkingTypeData
				{
					Out = locomotionData.RunOut,
					Loop = locomotionData.Run,
					Parameters = RunParameters
				};
			}
			break;
		case WalkSpeedType.Sprint:
			if (locomotionData.Sprint != null)
			{
				return new WalkingTypeData
				{
					Out = locomotionData.SprintOut,
					Loop = locomotionData.Sprint,
					Parameters = SprintParameters
				};
			}
			break;
		case WalkSpeedType.Crouch:
			if (locomotionData.Cover != null)
			{
				return new WalkingTypeData
				{
					Out = locomotionData.CoverOut,
					Loop = locomotionData.Cover,
					Parameters = CoverParameters
				};
			}
			break;
		}
		if (locomotionData.Run != null)
		{
			return new WalkingTypeData
			{
				Out = locomotionData.RunOut,
				Loop = locomotionData.Run,
				Parameters = RunParameters
			};
		}
		WalkingTypeData walkingTypeData = ((locomotionData != NonCombatLocomotionData) ? GetWalkingTypeData(NonCombatLocomotionData, walkingType) : null);
		if (walkingTypeData == null)
		{
			if (NonCombatLocomotionData.Idle != null)
			{
				return new WalkingTypeData
				{
					Loop = NonCombatLocomotionData.Idle
				};
			}
			PFLog.Animations.Error(this, $"Moving data not found for {walkingType}");
		}
		return walkingTypeData;
	}

	private WalkingTypeData GetCustomCombatWalkData(WeaponStyleLocomotionData locomotionData, WalkSpeedType walkingType, CustomCombatWalkType combatWalkType)
	{
		if (walkingType != WalkSpeedType.Walk || combatWalkType == CustomCombatWalkType.None)
		{
			return null;
		}
		if (locomotionData == NonCombatLocomotionData)
		{
			return null;
		}
		if (locomotionData.WalkOneCell == null && locomotionData.WalkTwoCell == null && locomotionData.WalkOneDiagonalCell == null && locomotionData.WalkTwoDiagonalCell == null)
		{
			return null;
		}
		AnimationClipWrapper animationClipWrapper = combatWalkType switch
		{
			CustomCombatWalkType.OneCell => locomotionData.WalkOneCell, 
			CustomCombatWalkType.TwoCells => locomotionData.WalkTwoCell, 
			CustomCombatWalkType.OneDiagonal => locomotionData.WalkOneDiagonalCell, 
			CustomCombatWalkType.TwoDiagonals => locomotionData.WalkTwoDiagonalCell, 
			_ => null, 
		};
		MovingTypeParameters movingTypeParameters = new MovingTypeParameters();
		if (animationClipWrapper != null && animationClipWrapper.Length > 0f)
		{
			float num = DistanceList[(int)combatWalkType];
			movingTypeParameters.Speed = num / animationClipWrapper.Length;
		}
		return new WalkingTypeData
		{
			Loop = animationClipWrapper,
			Parameters = movingTypeParameters
		};
	}

	public CustomCombatWalkType GetCustomCombatWalkType(PathCursor path)
	{
		if (path == null || !path.HasPath)
		{
			return CustomCombatWalkType.None;
		}
		float pathLength = path.GetPathLength(3);
		for (int i = 1; i < DistanceList.Count; i++)
		{
			if (pathLength < DistanceList[i])
			{
				return (CustomCombatWalkType)i;
			}
		}
		return CustomCombatWalkType.None;
	}

	public AnimationClipWrapper GetIdleClip(WeaponAnimationStyle style, bool isForDollRoom)
	{
		if (isForDollRoom)
		{
			BlueprintWeaponStyleList weaponStyleSettings = WeaponStyleSettings;
			object obj;
			if (weaponStyleSettings == null)
			{
				obj = null;
			}
			else
			{
				BlueprintWeaponStyleAnimationSet blueprintWeaponStyleAnimationSet = weaponStyleSettings[style];
				obj = ((blueprintWeaponStyleAnimationSet != null) ? blueprintWeaponStyleAnimationSet.DollRoom.Idle.Or(null) : null);
			}
			if (obj == null)
			{
				BlueprintWeaponStyleList weaponStyleSettings2 = WeaponStyleSettings;
				if (weaponStyleSettings2 == null)
				{
					return null;
				}
				BlueprintWeaponStyleAnimationSet blueprintWeaponStyleAnimationSet2 = weaponStyleSettings2[WeaponAnimationStyle.NonCombat];
				if (blueprintWeaponStyleAnimationSet2 == null)
				{
					return null;
				}
				obj = blueprintWeaponStyleAnimationSet2.DollRoom.Idle;
			}
			return (AnimationClipWrapper)obj;
		}
		BlueprintWeaponStyleList weaponStyleSettings3 = WeaponStyleSettings;
		object obj2;
		if (weaponStyleSettings3 == null)
		{
			obj2 = null;
		}
		else
		{
			BlueprintWeaponStyleAnimationSet blueprintWeaponStyleAnimationSet3 = weaponStyleSettings3[style];
			obj2 = ((blueprintWeaponStyleAnimationSet3 != null) ? blueprintWeaponStyleAnimationSet3.Locomotion.Idle.Or(null) : null);
		}
		if (obj2 == null)
		{
			BlueprintWeaponStyleList weaponStyleSettings4 = WeaponStyleSettings;
			if (weaponStyleSettings4 == null)
			{
				return null;
			}
			BlueprintWeaponStyleAnimationSet blueprintWeaponStyleAnimationSet4 = weaponStyleSettings4[WeaponAnimationStyle.NonCombat];
			if (blueprintWeaponStyleAnimationSet4 == null)
			{
				return null;
			}
			obj2 = blueprintWeaponStyleAnimationSet4.Locomotion.Idle;
		}
		return (AnimationClipWrapper)obj2;
	}

	public AnimationClipWrapper GetVariantIdleClip(UnitAnimationActionHandle handle)
	{
		List<AnimationClipWrapper> variantIdleClipList = GetVariantIdleClipList(handle, m_ForDollRoom);
		if (variantIdleClipList == null || variantIdleClipList.Count == 0)
		{
			return null;
		}
		return variantIdleClipList[handle.Manager.StatefulRandom.Range(0, variantIdleClipList.Count)];
	}

	private List<AnimationClipWrapper> GetVariantIdleClipList(UnitAnimationActionHandle handle, bool isForDollRoom)
	{
		if (!isForDollRoom)
		{
			if (handle.WeaponStyle == WeaponAnimationStyle.NonCombat)
			{
				AbstractUnitEntityView abstractUnitEntityView = handle.Unit.Or(null);
				List<AnimationClipWrapper> list = (((object)abstractUnitEntityView == null) ? null : abstractUnitEntityView.AnimationManager.Or(null)?.CustomIdleWrappers);
				if (list != null && list.Count > 0)
				{
					return list;
				}
			}
			object obj = WeaponStyleSettings?[handle.WeaponStyle]?.Locomotion.Variants;
			if (obj == null)
			{
				BlueprintWeaponStyleList weaponStyleSettings = WeaponStyleSettings;
				if (weaponStyleSettings == null)
				{
					return null;
				}
				BlueprintWeaponStyleAnimationSet blueprintWeaponStyleAnimationSet = weaponStyleSettings[WeaponAnimationStyle.NonCombat];
				if (blueprintWeaponStyleAnimationSet == null)
				{
					return null;
				}
				obj = blueprintWeaponStyleAnimationSet.Locomotion.Variants;
			}
			return (List<AnimationClipWrapper>)obj;
		}
		object obj2 = WeaponStyleSettings?[handle.WeaponStyle]?.DollRoom.Variants;
		if (obj2 == null)
		{
			BlueprintWeaponStyleList weaponStyleSettings2 = WeaponStyleSettings;
			if (weaponStyleSettings2 == null)
			{
				return null;
			}
			BlueprintWeaponStyleAnimationSet blueprintWeaponStyleAnimationSet2 = weaponStyleSettings2[WeaponAnimationStyle.NonCombat];
			if (blueprintWeaponStyleAnimationSet2 == null)
			{
				return null;
			}
			obj2 = blueprintWeaponStyleAnimationSet2.DollRoom.Variants;
		}
		return (List<AnimationClipWrapper>)obj2;
	}

	public WeaponStyleLocomotionData GetLocomotionData(WeaponAnimationStyle style)
	{
		return WeaponStyleSettings[style]?.Locomotion ?? NonCombatLocomotionData;
	}

	public LocomotionMixerAnimations GetLocomotionMixerAnimations(WeaponAnimationStyle style)
	{
		float num = (m_UseCustomBlending ? m_WalkBlendParam : (WalkParameters.Speed / SprintParameters.Speed));
		float num2 = (m_UseCustomBlending ? m_RunBlendParam : (RunParameters.Speed / SprintParameters.Speed));
		float num3 = (m_UseCustomBlending ? m_SprintBlendParam : 1f);
		bool flag = num < num2 && !Mathf.Approximately(num, num2);
		bool flag2 = num3 > num2 && !Mathf.Approximately(num2, num3);
		WeaponStyleLocomotionData locomotionData = GetLocomotionData(style);
		LocomotionMixerAnimations locomotionMixerAnimations = new LocomotionMixerAnimations();
		locomotionMixerAnimations.Add(locomotionData.Idle, 0f, Vector2.zero);
		if (UseCommonLocomotionAnimations)
		{
			LocomotionMixerAnimationsList locomotionMixerAnimationsList = ((style != 0 && UseDifferentAnimationsInCombat) ? InCombatAnimations : NonCombatAnimations);
			locomotionMixerAnimations.Add(locomotionMixerAnimationsList.Run, num2, Vector2.right);
			locomotionMixerAnimations.Add(locomotionMixerAnimationsList.RunBack, num2, Vector2.left);
			locomotionMixerAnimations.Add(locomotionMixerAnimationsList.RunLeft, num2, Vector2.up);
			locomotionMixerAnimations.Add(locomotionMixerAnimationsList.RunRight, num2, Vector2.down);
			if (flag)
			{
				locomotionMixerAnimations.Add(locomotionMixerAnimationsList.Walk, num, Vector2.right);
				locomotionMixerAnimations.Add(locomotionMixerAnimationsList.WalkBack, num, Vector2.left);
				locomotionMixerAnimations.Add(locomotionMixerAnimationsList.WalkLeft, num, Vector2.up);
				locomotionMixerAnimations.Add(locomotionMixerAnimationsList.WalkRight, num, Vector2.down);
			}
			if (flag2)
			{
				locomotionMixerAnimations.Add(locomotionMixerAnimationsList.Sprint, num3, Vector2.right);
			}
			locomotionMixerAnimations.ClipWithFootstepEvents = ((locomotionMixerAnimationsList.Run != null) ? locomotionMixerAnimationsList.Run : locomotionMixerAnimationsList.Walk);
		}
		else
		{
			locomotionMixerAnimations.Add(locomotionData.Run, num2, Vector2.right);
			locomotionMixerAnimations.Add(locomotionData.RunBack, num2, Vector2.left);
			locomotionMixerAnimations.Add(locomotionData.RunLeft, num2, Vector2.up);
			locomotionMixerAnimations.Add(locomotionData.RunRight, num2, Vector2.down);
			if (flag)
			{
				locomotionMixerAnimations.Add(locomotionData.Walk, num, Vector2.right);
				locomotionMixerAnimations.Add(locomotionData.WalkBack, num, Vector2.left);
				locomotionMixerAnimations.Add(locomotionData.WalkLeft, num, Vector2.up);
				locomotionMixerAnimations.Add(locomotionData.WalkRight, num, Vector2.down);
			}
			if (flag2)
			{
				locomotionMixerAnimations.Add(locomotionData.Sprint, num3, Vector2.right);
			}
			locomotionMixerAnimations.ClipWithFootstepEvents = ((locomotionData.Run != null) ? locomotionData.Run : locomotionData.Walk);
		}
		return locomotionMixerAnimations;
	}
}
