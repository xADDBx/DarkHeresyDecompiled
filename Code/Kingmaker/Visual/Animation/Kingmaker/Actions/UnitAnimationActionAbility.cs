using System.Collections.Generic;
using System.Linq;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Animation.WeaponStyles;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "UnitAnimationActionAbility", menuName = "Animation Manager/Actions/Unit Animation Ability")]
public class UnitAnimationActionAbility : UnitAnimationAction
{
	private class Data
	{
		public AnimationClipWrapper Clip;

		public bool IsStarted;
	}

	[SerializeField]
	[ValidateNotNull]
	private BlueprintWeaponStyleList.Reference m_WeaponStyleSettings;

	private List<AnimationClip> m_ClipsListCache;

	private List<AnimationClipWrapper> m_ClipWrappersListCache;

	private HashSet<AnimationClipWrapper> m_ClipWrappersHashSet;

	private BlueprintWeaponStyleList WeaponStyleSettings => m_WeaponStyleSettings.Get();

	public override UnitAnimationType Type => UnitAnimationType.Ability;

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
			m_ClipWrappersHashSet.AddRange(from c in WeaponStyleSettings.EnumerateAbilityClips()
				where c != null
				select c);
		}
		return m_ClipWrappersHashSet;
	}

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		WeaponAnimationStyle weaponAnimationStyle = (handle.Manager.IsInCombat ? handle.WeaponStyle : WeaponAnimationStyle.NonCombat);
		AnimationClipWrapper animationClipWrapper = GetAnimationClipWrapper(handle.AbilityStyle, weaponAnimationStyle, handle.IsTargetSelf);
		if (animationClipWrapper == null && handle.Manager.IsInCombat)
		{
			PFLog.Animations.Log($"No clip found for '{weaponAnimationStyle}' weapon style, try search in 'NonCombat' weapon style");
			animationClipWrapper = GetAnimationClipWrapper(handle.AbilityStyle, WeaponAnimationStyle.NonCombat, handle.IsTargetSelf);
		}
		if (animationClipWrapper == null)
		{
			PFLog.Animations.Error($"No clip! (action: Ability, style: {handle.AbilityStyle}/{weaponAnimationStyle}, animset: {handle.Manager.AnimationSet}");
			handle.IsSkipped = true;
			handle.Release();
		}
		else
		{
			handle.ActionData = new Data
			{
				Clip = animationClipWrapper
			};
			handle.IsPrecastFinished = true;
		}
	}

	public override void OnTransitionOutStarted(UnitAnimationActionHandle handle)
	{
		if (handle.SpecialCastBehaviour != SpecialBehaviourType.NoCast && handle.IsPrecastFinished)
		{
			base.OnTransitionOutStarted(handle);
		}
	}

	public override void OnUpdate(UnitAnimationActionHandle handle, float deltaTime)
	{
		if (handle.Manager.BlockAttackAnimation)
		{
			return;
		}
		Data data = (Data)handle.ActionData;
		if (data == null)
		{
			return;
		}
		handle.SpeedScale = Game.CombatAnimSpeedUp;
		if (data.Clip == null)
		{
			if (handle.GetTime() >= handle.CastingTime)
			{
				handle.ActEventsCounter++;
				handle.Release();
			}
		}
		else if (!data.IsStarted)
		{
			handle.StartClip(data.Clip, ClipDurationType.Oneshot);
			data.IsStarted = true;
		}
	}

	private AnimationClipWrapper GetAnimationClipWrapper(AbilityAnimationStyle abilityStyle, WeaponAnimationStyle weaponStyle, bool isTargetSelf)
	{
		if (!isTargetSelf)
		{
			return WeaponStyleSettings[weaponStyle]?.Ability[abilityStyle];
		}
		return WeaponStyleSettings[weaponStyle]?.Ability[abilityStyle, WeaponStyleAbilityData.VariationType.Self];
	}
}
