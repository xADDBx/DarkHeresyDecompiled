using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class DialogPortraitView : View<PortraitVM>
{
	[SerializeField]
	private RectTransform m_RectTransform;

	[SerializeField]
	private Vector2 m_FullSize;

	[SerializeField]
	private Vector2 m_HalfSize;

	[SerializeField]
	private Image m_Portrait;

	[SerializeField]
	private FadeAnimator m_Animator;

	private bool m_IsShown;

	private Sprite PortraitSmall => base.ViewModel.PortraitSmall;

	private Sprite PortraitHalf => base.ViewModel.PortraitHalf;

	private Sprite PortraitFull => base.ViewModel.PortraitFull;

	private void Awake()
	{
		m_Animator?.Initialize();
		m_Animator.gameObject.SetActive(value: false);
	}

	protected override void OnBind()
	{
		if (PortraitFull != null)
		{
			m_Portrait.sprite = PortraitFull;
			m_RectTransform.sizeDelta = m_FullSize;
			Show();
		}
		else if (PortraitHalf != null)
		{
			m_Portrait.sprite = PortraitHalf;
			m_RectTransform.sizeDelta = m_HalfSize;
			Show();
		}
		else
		{
			Hide();
		}
	}

	protected override void OnUnbind()
	{
		Hide();
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
