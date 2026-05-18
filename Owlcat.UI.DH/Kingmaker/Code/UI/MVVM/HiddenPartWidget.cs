using System;
using DG.Tweening;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

[Serializable]
public class HiddenPartWidget
{
	[ShowIf("m_HasHiddenPart")]
	[SerializeField]
	private CanvasGroup m_HiddenBlock;

	[ShowIf("m_HasHiddenPart")]
	[SerializeField]
	private CanvasGroup m_DefaultBlock;

	[Header("Values")]
	[SerializeField]
	private bool m_HasHiddenPart;

	public void SetHiddenState(bool isHidden)
	{
		if (m_HasHiddenPart)
		{
			SwitchBlockVisibility(m_HiddenBlock, isHidden);
			SwitchBlockVisibility(m_DefaultBlock, !isHidden);
		}
	}

	public void SetHiddenStateImmediate(bool isHidden)
	{
		if (m_HasHiddenPart)
		{
			SwitchBlockVisibility(m_HiddenBlock, isHidden, immediate: true);
			SwitchBlockVisibility(m_DefaultBlock, !isHidden, immediate: true);
		}
	}

	private void SwitchBlockVisibility(CanvasGroup block, bool isVisible, bool immediate = false)
	{
		if (!(block == null))
		{
			if (immediate)
			{
				block.alpha = (isVisible ? 1f : 0f);
			}
			else
			{
				block.DOFade(isVisible ? 1f : 0f, 1f);
			}
		}
	}
}
