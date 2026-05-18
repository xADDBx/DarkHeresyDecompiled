using System.Collections.Generic;
using Kingmaker.ResourceLinks;
using UnityEngine;

namespace Kingmaker.Visual.Animation;

[CreateAssetMenu(fileName = "RandomWrapperSwitcher", menuName = "Animation/Wrapper Switcher/Random Wrapper Switcher")]
public class RandomWrapperSwitcher : AnimationClipWrapperSwitcher
{
	private static Dictionary<int, int> m_LastUsed = new Dictionary<int, int>();

	[SerializeField]
	private List<AnimationClipWrapperSwitcherLink> m_Switchers = new List<AnimationClipWrapperSwitcherLink>();

	[SerializeField]
	private bool m_TryToNotRepeat;

	public override AnimationClipWrapper GetWrapper(IAnimationManager animationManager)
	{
		if (m_Switchers == null || m_Switchers.Count == 0)
		{
			return null;
		}
		if (m_Switchers.Count == 1)
		{
			return GetWrapper(m_Switchers, 0, animationManager);
		}
		int num = animationManager.StatefulRandom.Range(0, m_Switchers.Count);
		GameObject gameObject = animationManager.GameObject;
		if (m_TryToNotRepeat && gameObject != null)
		{
			_ = m_Switchers[num];
			int instanceID = gameObject.GetInstanceID();
			if (m_LastUsed.TryGetValue(instanceID, out var value) && num == value)
			{
				num = (num + 1) % m_Switchers.Count;
			}
			_ = m_Switchers[num];
			m_LastUsed[instanceID] = num;
			return GetWrapper(m_Switchers, num, animationManager);
		}
		return GetWrapper(m_Switchers, num, animationManager);
	}

	public override IEnumerable<AnimationClipWrapper> EnumerateClips()
	{
		foreach (AnimationClipWrapperSwitcherLink switcher in m_Switchers)
		{
			IEnumerable<AnimationClipWrapper> enumerable = switcher.Load().EnumerateClips();
			foreach (AnimationClipWrapper item in enumerable)
			{
				yield return item;
			}
		}
	}

	private AnimationClipWrapper GetWrapper(List<AnimationClipWrapperSwitcherLink> switchers, int switcherIndex, IAnimationManager animationManager)
	{
		AnimationClipWrapperSwitcherLink animationClipWrapperSwitcherLink = switchers[switcherIndex];
		if (animationClipWrapperSwitcherLink == null)
		{
			PFLog.Animations.Error(this, $"Switchers[{switcherIndex}]: link is not set");
			return null;
		}
		AnimationClipWrapperSwitcher animationClipWrapperSwitcher = animationClipWrapperSwitcherLink.Load();
		if (animationClipWrapperSwitcher == null)
		{
			PFLog.Animations.Error(this, $"Switchers[{switcherIndex}]: switcher is null (missing asset?)");
			return null;
		}
		AnimationClipWrapper wrapper = animationClipWrapperSwitcher.GetWrapper(animationManager);
		if (wrapper == null)
		{
			PFLog.Animations.Error(this, $"Switchers[{switcherIndex}]: wrapper is null");
		}
		return wrapper;
	}
}
