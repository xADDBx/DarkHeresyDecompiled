using System;
using UnityEngine;

namespace Owlcat.UI;

[Serializable]
public struct OwlcatSelectableUnityAnimationBlock : IEquatable<OwlcatSelectableUnityAnimationBlock>
{
	[SerializeField]
	private Animator m_UnityAnimator;

	[SerializeField]
	private string m_NormalTrigger;

	[SerializeField]
	private string m_HighlightedTrigger;

	[SerializeField]
	private string m_PressedTrigger;

	[SerializeField]
	private string m_FocusedTrigger;

	[SerializeField]
	private string m_DisabledTrigger;

	public Animator Animator
	{
		get
		{
			return m_UnityAnimator;
		}
		set
		{
			m_UnityAnimator = value;
		}
	}

	public string NormalTrigger
	{
		get
		{
			return m_NormalTrigger;
		}
		set
		{
			m_NormalTrigger = value;
		}
	}

	public string HighlightedTrigger
	{
		get
		{
			return m_HighlightedTrigger;
		}
		set
		{
			m_HighlightedTrigger = value;
		}
	}

	public string FocusedTrigger
	{
		get
		{
			return m_FocusedTrigger;
		}
		set
		{
			m_FocusedTrigger = value;
		}
	}

	public string PressedTrigger
	{
		get
		{
			return m_PressedTrigger;
		}
		set
		{
			m_PressedTrigger = value;
		}
	}

	public string DisabledTrigger
	{
		get
		{
			return m_DisabledTrigger;
		}
		set
		{
			m_DisabledTrigger = value;
		}
	}

	public static OwlcatSelectableUnityAnimationBlock DefaultUnityAnimationBlock
	{
		get
		{
			OwlcatSelectableUnityAnimationBlock result = default(OwlcatSelectableUnityAnimationBlock);
			result.NormalTrigger = "Normal";
			result.HighlightedTrigger = "Highlighted";
			result.FocusedTrigger = "Focused";
			result.PressedTrigger = "Pressed";
			result.DisabledTrigger = "Disabled";
			return result;
		}
	}

	public bool Equals(OwlcatSelectableUnityAnimationBlock other)
	{
		if (NormalTrigger == other.NormalTrigger && HighlightedTrigger == other.HighlightedTrigger && PressedTrigger == other.PressedTrigger && FocusedTrigger == other.FocusedTrigger)
		{
			return DisabledTrigger == other.DisabledTrigger;
		}
		return false;
	}

	public void DoAnimation(string triggerName)
	{
		if (!(m_UnityAnimator == null) && m_UnityAnimator.isActiveAndEnabled && m_UnityAnimator.hasBoundPlayables && !string.IsNullOrEmpty(triggerName))
		{
			m_UnityAnimator.ResetTrigger(NormalTrigger);
			m_UnityAnimator.ResetTrigger(HighlightedTrigger);
			m_UnityAnimator.ResetTrigger(FocusedTrigger);
			m_UnityAnimator.ResetTrigger(PressedTrigger);
			m_UnityAnimator.ResetTrigger(DisabledTrigger);
			m_UnityAnimator.SetTrigger(triggerName);
		}
	}
}
