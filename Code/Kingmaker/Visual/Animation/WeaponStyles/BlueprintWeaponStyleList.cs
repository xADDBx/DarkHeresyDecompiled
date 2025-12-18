using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Visual.Animation.WeaponStyles;

[TypeId("24424c5916fa4680940886c0b32f7658")]
public class BlueprintWeaponStyleList : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintWeaponStyleList>
	{
	}

	[Serializable]
	public class WeaponStyleEntry
	{
		public WeaponAnimationStyle Style;

		[SerializeField]
		private BlueprintWeaponStyleAnimationSet.Reference m_AnimationSet;

		public BlueprintWeaponStyleAnimationSet AnimationSet => m_AnimationSet?.Get();

		public void Preload()
		{
			m_AnimationSet.GetBlueprint();
		}
	}

	public List<WeaponStyleEntry> WeaponStyles;

	[CanBeNull]
	public BlueprintWeaponStyleAnimationSet this[WeaponAnimationStyle style]
	{
		get
		{
			for (int i = 0; i < WeaponStyles.Count; i++)
			{
				if (WeaponStyles[i].Style == style)
				{
					return WeaponStyles[i].AnimationSet;
				}
			}
			return null;
		}
	}

	private IEnumerable<AnimationClipWrapper> EnumerateClips(Func<WeaponStyleEntry, IWeaponStyleAnimationClipsProvider> clipsProviderGetter)
	{
		return from c in WeaponStyles.Where((WeaponStyleEntry x) => clipsProviderGetter(x) != null).SelectMany((WeaponStyleEntry x) => clipsProviderGetter(x).EnumerateClips())
			where c != null
			select c;
	}

	public IEnumerable<AnimationClipWrapper> EnumerateLocomotionClips()
	{
		return EnumerateClips((WeaponStyleEntry x) => x.AnimationSet?.Locomotion);
	}

	public IEnumerable<AnimationClipWrapper> EnumerateCoverClips()
	{
		return EnumerateClips((WeaponStyleEntry x) => x.AnimationSet?.Cover);
	}

	public IEnumerable<AnimationClipWrapper> EnumerateLeapClips()
	{
		return EnumerateClips((WeaponStyleEntry x) => x.AnimationSet?.Leap);
	}

	public IEnumerable<AnimationClipWrapper> EnumerateClimbClips()
	{
		return EnumerateClips((WeaponStyleEntry x) => x.AnimationSet?.Climb);
	}

	public IEnumerable<AnimationClipWrapper> EnumerateAbilityClips()
	{
		return EnumerateClips((WeaponStyleEntry x) => x.AnimationSet?.Ability);
	}

	public IEnumerable<AnimationClipWrapper> EnumerateAttackClips()
	{
		return EnumerateClips((WeaponStyleEntry x) => x.AnimationSet?.Attack);
	}

	public IEnumerable<AnimationClipWrapper> EnumerateEquipClips()
	{
		return EnumerateClips((WeaponStyleEntry x) => x.AnimationSet?.Attack);
	}

	public IEnumerable<AnimationClipWrapper> EnumerateCustomLoopActionClips()
	{
		return EnumerateClips((WeaponStyleEntry x) => x.AnimationSet?.CustomLoopAction);
	}

	public IEnumerable<AnimationClipWrapper> EnumerateDefenceClips()
	{
		return EnumerateClips((WeaponStyleEntry x) => x.AnimationSet?.Defence);
	}

	public IEnumerable<AnimationClipWrapper> EnumerateDisabledClips()
	{
		return EnumerateClips((WeaponStyleEntry x) => x.AnimationSet?.Disabled);
	}

	public IEnumerable<AnimationClipWrapper> EnumerateProneClips()
	{
		return EnumerateClips((WeaponStyleEntry x) => x.AnimationSet?.Prone);
	}

	public IEnumerable<AnimationClipWrapper> EnumerateDollRoomClips()
	{
		return EnumerateClips((WeaponStyleEntry x) => x.AnimationSet?.DollRoom);
	}
}
