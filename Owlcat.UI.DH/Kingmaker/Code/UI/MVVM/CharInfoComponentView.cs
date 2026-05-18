using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.Events;

namespace Kingmaker.Code.UI.MVVM;

public abstract class CharInfoComponentView<TViewModel> : View<TViewModel>, ICharInfoComponentView where TViewModel : CharInfoComponentVM
{
	[ShowIf("m_HasAnimation")]
	[SerializeField]
	protected FadeAnimator m_FadeAnimator;

	[SerializeField]
	private bool m_HasAnimation = true;

	public bool IsBinded => base.ViewModel != null;

	public virtual void Initialize()
	{
		m_FadeAnimator.Or(null)?.Initialize();
		base.gameObject.SetActive(value: false);
	}

	protected override void OnBind()
	{
		Show();
		base.ViewModel.Unit.Subscribe(delegate
		{
			RefreshView();
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		Hide();
	}

	protected virtual void RefreshView()
	{
	}

	public void BindSection(CharInfoComponentVM vm)
	{
		Bind(vm as TViewModel);
	}

	public void UnbindSection()
	{
		Unbind();
	}

	private void Show()
	{
		OnShow();
		base.gameObject.SetActive(value: true);
		if ((bool)m_FadeAnimator && m_HasAnimation)
		{
			m_FadeAnimator.AppearAnimation();
		}
	}

	public void ShowLeftCanvasSound()
	{
		ServiceWindowsSounds.Instance.Character.StatsShow.Play();
	}

	public void ShowRightCanvasSound()
	{
		ServiceWindowsSounds.Instance.Character.InfoShow.Play();
	}

	protected virtual void OnShow()
	{
	}

	public void Hide(UnityAction onHideCallback = null)
	{
		OnHide();
		if ((bool)m_FadeAnimator && m_HasAnimation)
		{
			m_FadeAnimator.DisappearAnimation(delegate
			{
				base.gameObject.SetActive(value: false);
				onHideCallback?.Invoke();
			});
		}
	}

	protected virtual void OnHide(UnityAction onHideCallback = null)
	{
	}
}
