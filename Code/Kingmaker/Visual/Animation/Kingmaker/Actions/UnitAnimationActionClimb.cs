using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.Visual.Animation.WeaponStyles;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

public class UnitAnimationActionClimb : UnitAnimationAction
{
	private class Data
	{
		public AnimationClipWrapper LastClip;

		public WarhammerNodeLinkTraverser.State TraverseState;
	}

	private class ClimbSettings
	{
		public float HorizontalInDuration;

		public float InDuration;

		public float TraverseDuration;

		public float OutDuration;

		public float HorizontalOutClipDuration;

		public float HorizontalOutMoveDuration;

		public float LegsOffsetAtTop;

		public float LegsOffsetAtBottom;

		public float VerticalSpeed;

		public bool IsLargeUnit;

		public float InClipStartTime;

		public AnimationClipWrapper HorizontalInClip;

		public AnimationClipWrapper InClip;

		public AnimationClipWrapper LoopClip;

		public AnimationClipWrapper OutClip;

		public AnimationClipWrapper HorizontalOutClip;
	}

	private enum ClimbType
	{
		LowLedgeUp,
		LowLedgeDown,
		HighLedgeUp,
		HighLedgeDown,
		LadderUp,
		LadderDown
	}

	private const float DefaultSpeedScale = 1f;

	[SerializeField]
	private float m_ClimbUpSpeed;

	[SerializeField]
	private float m_ClimbDownSpeed;

	[Space]
	[Tooltip("It is a distance between unit's feet and the upper end of the ladder, when unit is at the top of the ladder, but not on the ground")]
	[SerializeField]
	private float m_LegsOffsetAtLadderTop;

	[Tooltip("It is a distance between unit's feet and the bottom end of the ladder, when unit is about to jump to the ground from the ladder")]
	[SerializeField]
	private float m_LegsOffsetAtLadderBottom;

	[SerializeField]
	private BlueprintWeaponStyleList.Reference m_WeaponStyleSettings;

	private HashSet<AnimationClipWrapper> m_ClipWrappersHashSet;

	[CanBeNull]
	private BlueprintWeaponStyleList WeaponStyleSettings => m_WeaponStyleSettings?.Get();

	public override UnitAnimationType Type => UnitAnimationType.Climb;

	public override IEnumerable<AnimationClipWrapper> ClipWrappers => CollectClipWrappers();

	private IEnumerable<AnimationClipWrapper> CollectClipWrappers()
	{
		if (m_ClipWrappersHashSet != null)
		{
			return m_ClipWrappersHashSet;
		}
		m_ClipWrappersHashSet = new HashSet<AnimationClipWrapper>();
		if (WeaponStyleSettings != null)
		{
			m_ClipWrappersHashSet.AddRange(WeaponStyleSettings.EnumerateClimbClips());
		}
		return m_ClipWrappersHashSet;
	}

	public override void OnTransitionOutStarted(UnitAnimationActionHandle handle)
	{
	}

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		WarhammerNodeLinkTraverser nodeLinkTraverser = handle.Unit.MovementAgent.NodeLinkTraverser;
		WeaponStyleClimbData climbClips = GetClimbClips(handle.WeaponStyle);
		ClimbSettings climbSettings = GetClimbSettings(nodeLinkTraverser.TraverseData, climbClips);
		if (climbSettings != null)
		{
			if (nodeLinkTraverser.IsUpTraverse)
			{
				nodeLinkTraverser.InUpHorizontalClipDuration = climbSettings.HorizontalInDuration;
				nodeLinkTraverser.InUpVerticalClipDuration = climbSettings.InDuration;
				nodeLinkTraverser.OutUpVerticalClipDuration = climbSettings.OutDuration;
				nodeLinkTraverser.OutUpHorizontalClipDuration = climbSettings.HorizontalOutClipDuration;
				nodeLinkTraverser.OutUpHorizontalMoveDuration = climbSettings.HorizontalOutMoveDuration;
				nodeLinkTraverser.InUpVerticalDistance = 0f;
				nodeLinkTraverser.OutUpVerticalDistance = climbSettings.LegsOffsetAtTop;
			}
			else
			{
				nodeLinkTraverser.InDownHorizontalClipDuration = climbSettings.HorizontalInDuration;
				nodeLinkTraverser.InDownVerticalClipDuration = climbSettings.InDuration;
				nodeLinkTraverser.OutDownVerticalClipDuration = climbSettings.OutDuration;
				nodeLinkTraverser.OutDownHorizontalClipDuration = climbSettings.HorizontalOutClipDuration;
				nodeLinkTraverser.InDownVerticalDistance = climbSettings.LegsOffsetAtTop;
				nodeLinkTraverser.OutDownVerticalDistance = climbSettings.LegsOffsetAtBottom;
			}
			nodeLinkTraverser.VerticalSpeed = climbSettings.VerticalSpeed;
			nodeLinkTraverser.IsLargeEntity = climbSettings.IsLargeUnit;
			handle.AnimationLayer = UnitAnimationLayerType.Locomotion;
			handle.HasCrossfadePriority = true;
			handle.ActionData = new Data();
		}
	}

	public override void OnUpdate(UnitAnimationActionHandle handle, float deltaTime)
	{
		WarhammerNodeLinkTraverser nodeLinkTraverser = handle.Unit.MovementAgent.NodeLinkTraverser;
		WarhammerNodeLinkTraverser.State lastState = nodeLinkTraverser.LastState;
		if (handle.ActionData is Data data && data.TraverseState == lastState)
		{
			AnimationBase activeAnimation = handle.ActiveAnimation;
			if (activeAnimation == null || activeAnimation.State == AnimationState.Finished)
			{
				nodeLinkTraverser.ForceNextState();
			}
		}
		else if (nodeLinkTraverser.LastState.IsTraverseState())
		{
			StartClip(handle, lastState);
		}
		else
		{
			handle.Release();
		}
	}

	private void StartClip(UnitAnimationActionHandle handle, WarhammerNodeLinkTraverser.State traverseState)
	{
		Data data = (Data)handle.ActionData;
		if (data == null)
		{
			PFLog.Animations.Error("Climb action has no ActionData, release animation handle");
			handle.Release();
			return;
		}
		data.TraverseState = traverseState;
		WarhammerNodeLinkTraverser nodeLinkTraverser = handle.Unit.MovementAgent.NodeLinkTraverser;
		WeaponStyleClimbData climbClips = GetClimbClips(handle.WeaponStyle);
		ClimbSettings climbSettings = GetClimbSettings(nodeLinkTraverser.TraverseData, climbClips);
		if (climbSettings == null)
		{
			return;
		}
		AnimationClipWrapper animationClipWrapper = traverseState switch
		{
			WarhammerNodeLinkTraverser.State.TraverseHorizontalIn => climbSettings.HorizontalInClip, 
			WarhammerNodeLinkTraverser.State.TraverseIn => climbSettings.InClip, 
			WarhammerNodeLinkTraverser.State.Traverse => climbSettings.LoopClip, 
			WarhammerNodeLinkTraverser.State.TraverseOut => climbSettings.OutClip, 
			WarhammerNodeLinkTraverser.State.TraverseHorizontalOut => climbSettings.HorizontalOutClip, 
			_ => null, 
		};
		if (!(animationClipWrapper == null))
		{
			ClipDurationType duration = ((traverseState != WarhammerNodeLinkTraverser.State.Traverse) ? ClipDurationType.Oneshot : ClipDurationType.Endless);
			handle.StartClip(animationClipWrapper, duration);
			if (data.LastClip == null)
			{
				handle.ActiveAnimation.SetTime(climbSettings.InClipStartTime);
			}
			data.LastClip = animationClipWrapper;
		}
	}

	private ClimbSettings GetClimbSettings(TraverseData data, WeaponStyleClimbData climbClips)
	{
		float traverseHeight = data.TraverseHeight;
		object obj = GetClimbType(traverseHeight, data.IsUpTraverse) switch
		{
			ClimbType.LowLedgeUp => GetLedgeClimbSettings(data, climbClips.LedgeLowUp, traverseHeight, isUp: true), 
			ClimbType.LowLedgeDown => GetLedgeClimbSettings(data, climbClips.LedgeLowDown, traverseHeight, isUp: false), 
			ClimbType.HighLedgeUp => GetLedgeClimbSettings(data, climbClips.LedgeHighUp, traverseHeight, isUp: true), 
			ClimbType.HighLedgeDown => GetLedgeClimbSettings(data, climbClips.LedgeHighDown, traverseHeight, isUp: false), 
			ClimbType.LadderUp => GetLadderClimbSettings(SelectClipAtTraverseStart(data, climbClips.LadderUpHorizontalIn, climbClips.LadderUpHorizontalInLeft), null, climbClips.LadderUp, SelectClipAtTraverseEnd(data, climbClips.LadderUpVerticalOut, climbClips.LadderUpVerticalOutLeft), SelectClipAtTraverseEnd(data, climbClips.LadderUpHorizontalOut, climbClips.LadderUpHorizontalOutLeft), CalculateTraverseDuration(traverseHeight, isUp: true), m_ClimbUpSpeed, data.SpeedBeforeTraverse, data.HasPathAfterTraverse), 
			ClimbType.LadderDown => GetLadderClimbSettings(SelectClipAtTraverseStart(data, climbClips.LadderDownHorizontalIn, climbClips.LadderDownHorizontalInLeft), SelectClipAtTraverseStart(data, climbClips.LadderDownVerticalIn, climbClips.LadderDownVerticalInLeft), climbClips.LadderDown, SelectClipAtTraverseEnd(data, climbClips.LadderDownVerticalOut, climbClips.LadderDownVerticalOutLeft), SelectClipAtTraverseEnd(data, climbClips.LadderDownHorizontalOut, climbClips.LadderDownHorizontalOutLeft), CalculateTraverseDuration(traverseHeight, isUp: false), m_ClimbDownSpeed, data.SpeedBeforeTraverse, data.HasPathAfterTraverse), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		((ClimbSettings)obj).IsLargeUnit = climbClips.IsLargeUnit;
		return (ClimbSettings)obj;
	}

	private static bool IsAnimationSynchronizationNeeded(WarhammerNodeLinkTraverser traverser)
	{
		if (traverser.IsUpTraverse && traverser.LastState == WarhammerNodeLinkTraverser.State.TraverseHorizontalOut)
		{
			return !traverser.HasPathAfterTraverse;
		}
		return false;
	}

	private void SynchronizeAnimationWithViewPosition(UnitAnimationActionHandle handle, TraverseData traverseData, MechanicEntity entity)
	{
		float ratio = Mathf.Sqrt((entity.GetView().transform.position.To2D() - traverseData.WaypointFrom2D).sqrMagnitude / (traverseData.WaypointTo2D - traverseData.WaypointFrom2D).sqrMagnitude);
		WeaponStyleClimbData climbClips = GetClimbClips(handle.WeaponStyle);
		ClimbSettings climbSettings = GetClimbSettings(traverseData, climbClips);
		(float time, float endTime) horizontalOutTimeByDistanceRatio = GetHorizontalOutTimeByDistanceRatio(climbSettings, ratio);
		float item = horizontalOutTimeByDistanceRatio.time;
		float item2 = horizontalOutTimeByDistanceRatio.endTime;
		float num = (item2 - handle.ActiveAnimation?.GetTime()) ?? item2;
		float num2 = item2 - item;
		handle.SpeedScale = num / num2;
	}

	private (float time, float endTime) GetHorizontalOutTimeByDistanceRatio(ClimbSettings climbSettings, float ratio)
	{
		if (climbSettings.HorizontalOutClip != null)
		{
			return (time: climbSettings.HorizontalOutClip.Length * ratio, endTime: climbSettings.HorizontalOutClip.Length);
		}
		float endTraverseTime = climbSettings.InClip.GetEndTraverseTime();
		float num = climbSettings.InClip.Length - 0.001f;
		return (time: Mathf.Lerp(endTraverseTime, num, ratio), endTime: num);
	}

	private static AnimationClipWrapper SelectClipAtTraverseStart(TraverseData data, AnimationClipWrapper rightClip, AnimationClipWrapper leftClip)
	{
		Vector3 waypointOffsetVector = GetWaypointOffsetVector(data, atTraverseStart: true);
		return SelectRightOrLeftClip(data, waypointOffsetVector, rightClip, leftClip);
	}

	private static AnimationClipWrapper SelectClipAtTraverseEnd(TraverseData data, AnimationClipWrapper rightClip, AnimationClipWrapper leftClip)
	{
		Vector3 waypointOffsetVector = GetWaypointOffsetVector(data, atTraverseStart: false);
		return SelectRightOrLeftClip(data, waypointOffsetVector, rightClip, leftClip);
	}

	private static Vector3 GetWaypointOffsetVector(TraverseData data, bool atTraverseStart)
	{
		if (!atTraverseStart)
		{
			return data.WaypointTo2D.To3D() - data.GraphNodeTo2D.To3D();
		}
		return data.GraphNodeFrom2D.To3D() - data.WaypointFrom2D.To3D();
	}

	private static AnimationClipWrapper SelectRightOrLeftClip(TraverseData data, Vector3 waypointOffsetVec, AnimationClipWrapper rightClip, AnimationClipWrapper leftClip)
	{
		if (waypointOffsetVec.sqrMagnitude < 0.0001f)
		{
			return rightClip;
		}
		if (leftClip == null)
		{
			return rightClip;
		}
		Vector3 rhs = data.GraphNodeTo2D.To3D() - data.GraphNodeFrom2D.To3D();
		if (!(Vector3.Cross(waypointOffsetVec, rhs).y >= 0f))
		{
			return leftClip;
		}
		return rightClip;
	}

	private ClimbType GetClimbType(float height, bool isUp)
	{
		if (height <= 2.3f)
		{
			if (!isUp)
			{
				return ClimbType.LowLedgeDown;
			}
			return ClimbType.LowLedgeUp;
		}
		if (height <= 3.2f)
		{
			if (!isUp)
			{
				return ClimbType.HighLedgeDown;
			}
			return ClimbType.HighLedgeUp;
		}
		if (!isUp)
		{
			return ClimbType.LadderDown;
		}
		return ClimbType.LadderUp;
	}

	private ClimbSettings GetLedgeClimbSettings(TraverseData data, AnimationClipWrapper climbLedgeClip, float height, bool isUp)
	{
		float startTraverseTime = climbLedgeClip.GetStartTraverseTime();
		float endTraverseTime = climbLedgeClip.GetEndTraverseTime();
		float traverseToMoveTime = climbLedgeClip.GetTraverseToMoveTime();
		float num = ((data.SpeedBeforeTraverse > 2f) ? climbLedgeClip.GetMoveToTraverseTime() : 0f);
		float num2 = endTraverseTime - startTraverseTime;
		float length = climbLedgeClip.Length;
		ClimbSettings climbSettings = new ClimbSettings
		{
			InDuration = startTraverseTime,
			TraverseDuration = num2,
			OutDuration = length - endTraverseTime,
			VerticalSpeed = height / num2,
			InClipStartTime = num
		};
		if (isUp)
		{
			climbSettings.InClip = climbLedgeClip;
			climbSettings.InDuration = startTraverseTime - num;
			climbSettings.OutDuration = 0f;
			climbSettings.HorizontalOutClipDuration = (data.HasPathAfterTraverse ? (traverseToMoveTime - endTraverseTime) : (length - endTraverseTime));
			climbSettings.HorizontalOutMoveDuration = traverseToMoveTime - endTraverseTime;
		}
		else
		{
			climbSettings.HorizontalInClip = climbLedgeClip;
			climbSettings.HorizontalInDuration = startTraverseTime - num;
			climbSettings.InDuration = 0f;
			climbSettings.OutDuration = traverseToMoveTime - endTraverseTime;
		}
		return climbSettings;
	}

	private float CalculateTraverseDuration(float height, bool isUp)
	{
		if (!isUp)
		{
			return (height - m_LegsOffsetAtLadderTop - m_LegsOffsetAtLadderBottom) / m_ClimbDownSpeed;
		}
		return (height - m_LegsOffsetAtLadderTop) / m_ClimbUpSpeed;
	}

	private ClimbSettings GetLadderClimbSettings(AnimationClipWrapper horizontalInClip, AnimationClipWrapper inClip, AnimationClipWrapper loopClip, AnimationClipWrapper outClip, AnimationClipWrapper horizontalOutClip, float traverseDuration, float verticalSpeed, float speedBeforeTraverse, bool hasPathAfterTraverse)
	{
		float inDuration = inClip.Or(null)?.Length ?? 0f;
		float outDuration = outClip.Or(null)?.Length ?? 0f;
		float num = ((horizontalInClip != null && speedBeforeTraverse > 2f) ? horizontalInClip.GetMoveToTraverseTime() : 0f);
		float horizontalInDuration = ((horizontalInClip != null) ? (horizontalInClip.Length - num) : 0f);
		float num2 = ((horizontalOutClip != null) ? horizontalOutClip.Length : 0f);
		float num3 = ((horizontalOutClip != null) ? horizontalOutClip.GetTraverseToMoveTime() : 0f);
		return new ClimbSettings
		{
			HorizontalInDuration = horizontalInDuration,
			InDuration = inDuration,
			TraverseDuration = traverseDuration,
			OutDuration = outDuration,
			HorizontalOutClipDuration = (hasPathAfterTraverse ? num3 : num2),
			HorizontalOutMoveDuration = num3,
			HorizontalInClip = horizontalInClip,
			InClip = inClip,
			LoopClip = loopClip,
			OutClip = outClip,
			HorizontalOutClip = horizontalOutClip,
			LegsOffsetAtTop = m_LegsOffsetAtLadderTop,
			LegsOffsetAtBottom = m_LegsOffsetAtLadderBottom,
			VerticalSpeed = verticalSpeed,
			InClipStartTime = num
		};
	}

	private WeaponStyleClimbData GetClimbClips(WeaponAnimationStyle weaponStyle)
	{
		WeaponStyleClimbData weaponStyleClimbData = WeaponStyleSettings?[weaponStyle]?.Climb;
		if (weaponStyleClimbData == null || weaponStyleClimbData.EnumerateClips().Empty())
		{
			return WeaponStyleSettings?[WeaponAnimationStyle.NonCombat]?.Climb;
		}
		return weaponStyleClimbData;
	}
}
