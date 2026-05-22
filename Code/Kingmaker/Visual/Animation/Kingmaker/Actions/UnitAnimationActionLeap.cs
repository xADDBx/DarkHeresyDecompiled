using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Pathfinding;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Animation.WeaponStyles;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

public class UnitAnimationActionLeap : UnitAnimationAction
{
	[SerializeField]
	private BlueprintWeaponStyleList.Reference m_WeaponStyleSettings;

	private HashSet<AnimationClipWrapper> m_ClipWrappersHashSet;

	[CanBeNull]
	private BlueprintWeaponStyleList WeaponStyleSettings => m_WeaponStyleSettings?.Get();

	public override UnitAnimationType Type => UnitAnimationType.Leap;

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
			m_ClipWrappersHashSet.AddRange(from c in WeaponStyleSettings.EnumerateLeapClips()
				where c != null
				select c);
		}
		foreach (WeaponStyleLeapData item in WeaponStyleSettings.WeaponStyles.Select((BlueprintWeaponStyleList.WeaponStyleEntry x) => x.AnimationSet.Leap))
		{
			m_ClipWrappersHashSet.Add(item.LeapOneCell);
			m_ClipWrappersHashSet.Add(item.LeapTwoCell);
		}
		return m_ClipWrappersHashSet;
	}

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		WarhammerNodeLinkTraverser nodeLinkTraverser = handle.Unit.MovementAgent.NodeLinkTraverser;
		AnimationClipWrapper clipWrapper = GetClipWrapper(handle.WeaponStyle, nodeLinkTraverser.TraverseDistance);
		if (clipWrapper == null)
		{
			nodeLinkTraverser.LeapTraverseTime = 1f;
			handle.Release();
			return;
		}
		float startTraverseTime = clipWrapper.GetStartTraverseTime();
		float endTraverseTime = clipWrapper.GetEndTraverseTime();
		float num = ((nodeLinkTraverser.SpeedBeforeTraverse > 2f) ? clipWrapper.GetMoveToTraverseTime() : 0f);
		float num2 = (nodeLinkTraverser.HasPathAfterTraverse ? clipWrapper.GetTraverseToMoveTime() : clipWrapper.Length);
		nodeLinkTraverser.LeapTraverseTime = endTraverseTime - startTraverseTime;
		nodeLinkTraverser.LeapInTime = startTraverseTime - num;
		nodeLinkTraverser.LeapOutTime = num2 - endTraverseTime;
		handle.AnimationLayer = UnitAnimationLayerType.Locomotion;
		handle.StartClip(clipWrapper, ClipDurationType.Oneshot);
		handle.ActiveAnimation.SetTime(num);
	}

	public override void OnUpdate(UnitAnimationActionHandle handle, float deltaTime)
	{
		if (!handle.Manager.IsLeaping)
		{
			handle.Release();
		}
	}

	private AnimationClipWrapper GetClipWrapper(WeaponAnimationStyle weaponStyle, float distance)
	{
		WeaponStyleLeapData weaponStyleLeapData = ((WeaponStyleSettings == null) ? null : (WeaponStyleSettings[weaponStyle]?.Leap ?? WeaponStyleSettings[WeaponAnimationStyle.NonCombat]?.Leap));
		if (weaponStyleLeapData != null)
		{
			if (!(distance < 3f))
			{
				return weaponStyleLeapData.LeapTwoCell;
			}
			return weaponStyleLeapData.LeapOneCell;
		}
		return null;
	}
}
