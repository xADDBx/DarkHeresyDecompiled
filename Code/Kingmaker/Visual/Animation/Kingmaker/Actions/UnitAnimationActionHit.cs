using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.StatefulRandom;
using Kingmaker.Visual.Animation.Events;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "UnitAnimationActionHit", menuName = "Animation Manager/Actions/Unit Hit")]
public class UnitAnimationActionHit : UnitAnimationAction
{
	private class ActionData
	{
		public float RecoverFromHitTime;

		public bool IsRecovered;
	}

	public List<AnimationClipWrapper> HitClips;

	[Header("Hits from directions")]
	public bool UseHitsFromDirections;

	[ShowIf("UseHitsFromDirections")]
	public AnimationClipWrapper Front;

	[ShowIf("UseHitsFromDirections")]
	public AnimationClipWrapper Back;

	[Space]
	[ShowIf("UseHitsFromDirections")]
	public AnimationClipWrapper Left;

	[ShowIf("UseHitsFromDirections")]
	public AnimationClipWrapper Right;

	[Space]
	[ShowIf("UseHitsFromDirections")]
	public AnimationClipWrapper FrontLeft;

	[ShowIf("UseHitsFromDirections")]
	public AnimationClipWrapper FrontRight;

	[Space]
	[ShowIf("UseHitsFromDirections")]
	public AnimationClipWrapper BackLeft;

	[ShowIf("UseHitsFromDirections")]
	public AnimationClipWrapper BackRight;

	private HashSet<AnimationClipWrapper> m_ClipWrappersHashSet;

	public override UnitAnimationType Type => UnitAnimationType.Hit;

	public override bool IsAdditive => false;

	public override IEnumerable<AnimationClipWrapper> ClipWrappers => CollectClipWrappers();

	private IEnumerable<AnimationClipWrapper> CollectClipWrappers()
	{
		if (m_ClipWrappersHashSet != null)
		{
			return m_ClipWrappersHashSet;
		}
		m_ClipWrappersHashSet = new HashSet<AnimationClipWrapper>();
		m_ClipWrappersHashSet.AddRange(EnumerateClips());
		return m_ClipWrappersHashSet;
	}

	private IEnumerable<AnimationClipWrapper> EnumerateClips()
	{
		foreach (AnimationClipWrapper item in HitClips.Where((AnimationClipWrapper clip) => clip != null))
		{
			yield return item;
		}
		if (UseHitsFromDirections)
		{
			if (Front != null)
			{
				yield return Front;
			}
			if (Back != null)
			{
				yield return Back;
			}
			if (Left != null)
			{
				yield return Left;
			}
			if (Right != null)
			{
				yield return Right;
			}
			if (FrontLeft != null)
			{
				yield return FrontLeft;
			}
			if (FrontRight != null)
			{
				yield return FrontRight;
			}
			if (BackLeft != null)
			{
				yield return BackLeft;
			}
			if (BackRight != null)
			{
				yield return BackRight;
			}
		}
	}

	public override void OnFinish(UnitAnimationActionHandle handle)
	{
		if (handle.ActionData is ActionData { IsRecovered: false } actionData)
		{
			handle.Manager.HitAnimationIsActive.Release();
			actionData.IsRecovered = true;
		}
		base.OnFinish(handle);
	}

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		if (handle.Manager.IsDead)
		{
			return;
		}
		AnimationClipWrapper animationClip = GetAnimationClip(handle.Spell, handle.Unit.Data.Forward, handle.HitDirection, handle.Manager.StatefulRandom);
		if (animationClip == null)
		{
			PFLog.Animations.Error($"No clip! (action: Hit, weapon style: {handle.WeaponStyle}, animset: {handle.Manager.AnimationSet}");
			handle.Release();
			return;
		}
		handle.AnimationLayer = AnimationLayerType.Reactions;
		handle.StartClip(animationClip, ClipDurationType.Oneshot);
		if (handle.Manager.HitAnimationIsActive.Value)
		{
			handle.ActiveAnimation?.SetTime(GetHitRestartTime(animationClip));
		}
		handle.Manager.HitAnimationIsActive.Retain();
		handle.ActionData = new ActionData
		{
			RecoverFromHitTime = GetRecoverFromHitTime(animationClip)
		};
	}

	public override void OnUpdate(UnitAnimationActionHandle handle, float deltaTime)
	{
		ActionData actionData = (ActionData)handle.ActionData;
		if (!actionData.IsRecovered && !(actionData.RecoverFromHitTime > handle.GetTime()))
		{
			handle.Manager.HitAnimationIsActive.Release();
			actionData.IsRecovered = true;
		}
	}

	private AnimationClipWrapper GetAnimationClip([CanBeNull] BlueprintAbility hitSource, Vector3 unitForward, Vector3 hitDirection, StatefulRandom random)
	{
		if (!IsHitFromDirection(hitSource, hitDirection))
		{
			return GetRandomClipFromList(HitClips, random);
		}
		return TryGetClipForHitFromDirection(hitDirection, unitForward) ?? GetRandomClipFromList(HitClips, random);
	}

	private static bool IsHitFromDirection([CanBeNull] BlueprintAbility hitSource, Vector3 hitDirection)
	{
		if (hitDirection.sqrMagnitude > 0.01f)
		{
			if (hitSource != null)
			{
				return !hitSource.IsAoE;
			}
			return false;
		}
		return false;
	}

	private AnimationClipWrapper TryGetClipForHitFromDirection(Vector3 hitDirection, Vector3 unitForward)
	{
		float num = Vector3.SignedAngle(unitForward, hitDirection, Vector3.up);
		if (num < -23f)
		{
			if (!(num < -113f))
			{
				if (num < -67f)
				{
					return Left;
				}
				return FrontLeft;
			}
			if (!(num < -157f))
			{
				return BackLeft;
			}
		}
		else
		{
			if (!(num > 67f))
			{
				if (num > 23f)
				{
					return FrontRight;
				}
				return Front;
			}
			if (!(num > 157f))
			{
				if (num > 113f)
				{
					return BackRight;
				}
				return Right;
			}
		}
		return Back;
	}

	private static AnimationClipWrapper GetRandomClipFromList(IReadOnlyList<AnimationClipWrapper> list, StatefulRandom random)
	{
		if (list == null || list.Count == 0)
		{
			return null;
		}
		return list[random.Range(0, list.Count)];
	}

	private static float GetHitRestartTime(AnimationClipWrapper clip)
	{
		if (!clip.Events.TryFind((AnimationClipEvent e) => e is AnimationClipEventHitReact animationClipEventHitReact && animationClipEventHitReact.Type == HitReactAnimationEventType.Hit, out var result))
		{
			return 0f;
		}
		return result.Time;
	}

	private static float GetRecoverFromHitTime(AnimationClipWrapper clip)
	{
		if (!clip.Events.TryFind((AnimationClipEvent e) => e is AnimationClipEventHitReact animationClipEventHitReact && animationClipEventHitReact.Type == HitReactAnimationEventType.Recovered, out var result))
		{
			return clip.Length;
		}
		return result.Time;
	}
}
