using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class InfoSectionView : View<InfoSectionVM>
{
	private bool m_IsInit;

	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	private InfoBodyView m_InfoBodyView;

	[SerializeField]
	private ScrollRectExtended m_ScrollRectExtended;

	[SerializeField]
	private bool m_SaveScrollPosition;

	private readonly ReactiveProperty<bool> m_IsActive = new ReactiveProperty<bool>(value: true);

	public bool IsScrollActive => m_ScrollRectExtended.verticalScrollbar.IsActive();

	public bool ScrollbarOnBottom => m_ScrollRectExtended.verticalNormalizedPosition < 0.01f;

	public bool ScrollbarOnTop => m_ScrollRectExtended.verticalNormalizedPosition > 0.99f;

	public ScrollRectExtended ScrollRectExtended => m_ScrollRectExtended;

	public bool HasScroll => m_ScrollRectExtended.content.sizeDelta.y >= m_ScrollRectExtended.viewport.sizeDelta.y;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			base.gameObject.SetActive(value: false);
			m_FadeAnimator.Initialize();
			m_IsInit = true;
		}
	}

	public void SetActive(bool state)
	{
		m_IsActive.Value = state;
	}

	protected override void OnBind()
	{
		base.gameObject.SetActive(value: true);
		base.ViewModel.InfoVM.DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(delegate
		{
			TooltipDataChanged();
		}).AddTo(this);
		m_IsActive.DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(delegate
		{
			TooltipDataChanged();
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		Hide();
	}

	private void Show()
	{
		m_FadeAnimator.AppearAnimation();
		if (!m_SaveScrollPosition)
		{
			ResetPosition();
		}
	}

	public void Hide()
	{
		m_FadeAnimator.DisappearAnimation(delegate
		{
			base.gameObject.SetActive(value: false);
		});
	}

	private void TooltipDataChanged()
	{
		if (base.ViewModel.InfoVM.CurrentValue == null || !m_IsActive.Value)
		{
			Hide();
		}
		else
		{
			Show();
		}
		m_InfoBodyView.Bind(base.ViewModel.InfoVM.CurrentValue);
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
	}

	public void Scroll(float x)
	{
		if (!(m_ScrollRectExtended == null))
		{
			PointerEventData data = new PointerEventData(EventSystem.current)
			{
				scrollDelta = new Vector2(0f, x * m_ScrollRectExtended.scrollSensitivity)
			};
			m_ScrollRectExtended.OnSmoothlyScroll(data);
		}
	}

	public bool HandleDown()
	{
		float num = 1f;
		if (ScrollRectExtended.verticalNormalizedPosition > 0.01f)
		{
			Scroll(0f - num);
			return true;
		}
		return false;
	}

	public void ResetPosition()
	{
		m_ScrollRectExtended.Or(null)?.ScrollToTop();
	}
}
