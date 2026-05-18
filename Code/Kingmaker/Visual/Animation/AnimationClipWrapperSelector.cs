using System;
using Kingmaker.ResourceLinks;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Visual.Animation;

[Serializable]
public struct AnimationClipWrapperSelector
{
	public enum SelectorMode
	{
		Wrapper,
		Switcher
	}

	[SerializeField]
	private SelectorMode m_Mode;

	[SerializeField]
	[ShowIf("ShowWrapper")]
	private AnimationClipWrapperLink m_WrapperLink;

	[SerializeField]
	[ShowIf("ShowSwitcher")]
	private AnimationClipWrapperSwitcherLink m_SwitcherLink;

	private bool ShowWrapper => m_Mode == SelectorMode.Wrapper;

	private bool ShowSwitcher => m_Mode == SelectorMode.Switcher;

	public AnimationClipWrapper GetWrapper(IAnimationManager animationManager)
	{
		if (m_Mode == SelectorMode.Wrapper)
		{
			return m_WrapperLink?.Load();
		}
		if (m_Mode == SelectorMode.Switcher)
		{
			return m_SwitcherLink?.Load()?.GetWrapper(animationManager);
		}
		return null;
	}

	public void Load()
	{
		m_WrapperLink?.Load();
	}
}
