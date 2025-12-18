using System;
using JetBrains.Annotations;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenPortraitView : View<PortraitVM>
{
	[SerializeField]
	private CharBPortraitChanger m_Portrait;

	[SerializeField]
	private FadeAnimator m_Animator;

	[SerializeField]
	private PortraitVM.PortraitSize m_Size = PortraitVM.PortraitSize.Full;

	[SerializeField]
	private OwlcatMultiButton m_HoverButton;

	private Action<bool> m_HoverAction;

	[UsedImplicitly]
	private bool m_IsInit;

	[UsedImplicitly]
	private bool m_IsShown;

	private Sprite PortraitSmall => base.ViewModel.PortraitSmall;

	private Sprite PortraitHalf => base.ViewModel.PortraitHalf;

	private Sprite PortraitFull => base.ViewModel.PortraitFull;

	public void Initialize(Action<bool> hoverAction = null)
	{
		if (!m_IsInit)
		{
			m_HoverAction = hoverAction;
			m_Animator.Initialize();
			m_IsInit = true;
		}
	}

	protected override void OnBind()
	{
		Initialize();
		if (CheckPortrait())
		{
			Show();
			SetupView();
		}
		else
		{
			Hide();
		}
		if (m_HoverButton != null)
		{
			m_HoverButton.OnHoverAsObservable().Subscribe(delegate(bool value)
			{
				m_HoverAction?.Invoke(value);
			}).AddTo(this);
		}
	}

	protected override void OnUnbind()
	{
		Hide();
	}

	private bool CheckPortrait()
	{
		return m_Size switch
		{
			PortraitVM.PortraitSize.Small => PortraitSmall != null, 
			PortraitVM.PortraitSize.Middle => PortraitHalf != null, 
			PortraitVM.PortraitSize.Full => PortraitFull != null, 
			_ => false, 
		};
	}

	private void SetupView()
	{
		if (m_Portrait != null && m_Portrait.Or(null)?.transform != null)
		{
			m_Portrait.transform.localScale = new Vector3((!base.ViewModel.PortraitData.FlipFullLengthPortraitInDialog) ? 1 : (-1), 1f, 1f);
		}
		switch (m_Size)
		{
		case PortraitVM.PortraitSize.Small:
			m_Portrait.SetNewPortrait(PortraitSmall, playAnimation: true, playSound: true);
			break;
		case PortraitVM.PortraitSize.Middle:
			m_Portrait.SetNewPortrait(PortraitHalf, playAnimation: true, playSound: true);
			break;
		case PortraitVM.PortraitSize.Full:
			m_Portrait.SetNewPortrait(PortraitFull, playAnimation: true, playSound: true);
			break;
		}
	}

	public void SetVisibility(bool visible)
	{
		if (visible)
		{
			Show();
		}
		else
		{
			Hide();
		}
	}

	public void Show()
	{
		if (!m_IsShown)
		{
			if (m_Animator != null)
			{
				m_Animator.AppearAnimation();
			}
			else
			{
				base.gameObject.SetActive(value: true);
			}
			m_IsShown = true;
		}
	}

	public void Hide()
	{
		if (m_Animator != null)
		{
			m_Animator.DisappearAnimation();
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
		m_IsShown = false;
	}
}
