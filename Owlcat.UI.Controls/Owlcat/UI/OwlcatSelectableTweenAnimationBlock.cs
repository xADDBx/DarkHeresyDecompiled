using System;
using Owlcat.UI.Tweenable;
using UnityEngine;

namespace Owlcat.UI;

[Serializable]
public struct OwlcatSelectableTweenAnimationBlock : IEquatable<OwlcatSelectableTweenAnimationBlock>
{
	[SerializeField]
	private TweenBehavior m_NormalTween;

	[SerializeField]
	private TweenBehavior m_HighlightedTween;

	[SerializeField]
	private TweenBehavior m_PressedTween;

	[SerializeField]
	private TweenBehavior m_FocusedTween;

	[SerializeField]
	private TweenBehavior m_DisabledTween;

	public TweenBehavior NormalTween
	{
		get
		{
			return m_NormalTween;
		}
		set
		{
			m_NormalTween = value;
		}
	}

	public TweenBehavior HighlightedTween
	{
		get
		{
			return m_HighlightedTween;
		}
		set
		{
			m_HighlightedTween = value;
		}
	}

	public TweenBehavior PressedTween
	{
		get
		{
			return m_PressedTween;
		}
		set
		{
			m_PressedTween = value;
		}
	}

	public TweenBehavior FocusedTween
	{
		get
		{
			return m_FocusedTween;
		}
		set
		{
			m_FocusedTween = value;
		}
	}

	public TweenBehavior DisabledTween
	{
		get
		{
			return m_DisabledTween;
		}
		set
		{
			m_DisabledTween = value;
		}
	}

	public bool Equals(OwlcatSelectableTweenAnimationBlock other)
	{
		if (NormalTween == other.NormalTween && HighlightedTween == other.HighlightedTween && PressedTween == other.PressedTween && FocusedTween == other.FocusedTween)
		{
			return DisabledTween == other.DisabledTween;
		}
		return false;
	}
}
