using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.StatefulRandom;
using Kingmaker.Visual.Animation.WeaponStyles;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "UnitAnimationDodge", menuName = "Animation Manager/Actions/Unit Dodge")]
public class UnitAnimationActionDefence : UnitAnimationAction
{
	[SerializeField]
	private BlueprintWeaponStyleList.Reference m_WeaponStyleSettings;

	private HashSet<AnimationClipWrapper> m_ClipWrappersHashSet;

	[CanBeNull]
	private BlueprintWeaponStyleList WeaponStyleSettings => m_WeaponStyleSettings?.Get();

	public override bool IsAdditive => true;

	public override bool IsAdditiveToItself => false;

	public override UnitAnimationType Type => UnitAnimationType.Defence;

	public override IEnumerable<AnimationClipWrapper> ClipWrappers => CollectClipWrappers();

	private HashSet<AnimationClipWrapper> CollectClipWrappers()
	{
		if (m_ClipWrappersHashSet != null)
		{
			return m_ClipWrappersHashSet;
		}
		m_ClipWrappersHashSet = new HashSet<AnimationClipWrapper>();
		if (WeaponStyleSettings != null)
		{
			m_ClipWrappersHashSet.AddRange(WeaponStyleSettings.EnumerateDefenceClips());
		}
		return m_ClipWrappersHashSet;
	}

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		AnimationClipWrapper variant = GetVariant(handle);
		if ((bool)variant)
		{
			handle.AnimationLayer = UnitAnimationLayerType.Reactions;
			handle.StartClip(variant, ClipDurationType.Oneshot);
		}
		else
		{
			handle.Release();
		}
	}

	public override void OnUpdate(UnitAnimationActionHandle handle, float deltaTime)
	{
	}

	private AnimationClipWrapper GetVariant(UnitAnimationActionHandle handle)
	{
		WeaponStyleDefenceData weaponStyleDefenceData = WeaponStyleSettings?[handle.WeaponStyle]?.Defence ?? WeaponStyleSettings?[WeaponAnimationStyle.MeleeMelee]?.Defence;
		if (weaponStyleDefenceData == null)
		{
			return null;
		}
		if (handle.Manager.InCover && weaponStyleDefenceData.DefenceInCoverClips.Count > 0)
		{
			return GetRandomClipFromList(weaponStyleDefenceData.DefenceInCoverClips, handle.Manager.StatefulRandom);
		}
		return GetRandomClipFromList(weaponStyleDefenceData.DefenceClips, handle.Manager.StatefulRandom);
	}

	private static AnimationClipWrapper GetRandomClipFromList(IReadOnlyList<AnimationClipWrapper> list, StatefulRandom random)
	{
		if (list == null || list.Count == 0)
		{
			return null;
		}
		return list[random.Range(0, list.Count)];
	}
}
