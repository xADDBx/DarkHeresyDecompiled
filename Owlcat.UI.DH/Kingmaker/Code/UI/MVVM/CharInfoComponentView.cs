using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.Events;

namespace Kingmaker.Code.UI.MVVM;

public abstract class CharInfoComponentView<TViewModel> : View<TViewModel>, ICharInfoComponentView where TViewModel : CharInfoComponentVM
{
	[SerializeField]
	protected FadeAnimator m_FadeAnimator;

	public bool IsBinded => base.ViewModel != null;

	public virtual void Initialize()
	{
		m_FadeAnimator.Or(null)?.Initialize();
		base.gameObject.SetActive(value: false);
	}

	protected override void OnBind()
	{
		Show();
		base.ViewModel.Unit?.Subscribe(delegate
		{
			RefreshView();
		}).AddTo(this);
		if (base.ViewModel.Unit == null)
		{
			RefreshView();
		}
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
		if ((bool)m_FadeAnimator)
		{
			m_FadeAnimator.AppearAnimation();
		}
	}

	public void ShowLeftCanvasSound()
	{
		UISounds.Instance.Sounds.Character.CharacterStatsShow.Play();
	}

	public void ShowRightCanvasSound()
	{
		UISounds.Instance.Sounds.Character.CharacterInfoShow.Play();
	}

	protected virtual void OnShow()
	{
	}

	public void Hide(UnityAction onHideCallback = null)
	{
		OnHide();
		if ((bool)m_FadeAnimator)
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
