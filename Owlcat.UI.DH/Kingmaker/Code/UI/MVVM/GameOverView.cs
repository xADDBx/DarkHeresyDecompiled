using Kingmaker.Code.UI.MVVM.View;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Kingmaker.UI.Transitions;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class GameOverView : View<GameOverVM>, ITransitable
{
	[SerializeField]
	private ModalWindowView m_ModalWindowView;

	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	Transition ITransitable.Show()
	{
		return new UIAnimatorShowTransition(m_FadeAnimator).Run();
	}

	Transition ITransitable.Hide()
	{
		return new UIAnimatorHideTransition(m_FadeAnimator).Run();
	}

	protected override void OnBind()
	{
		base.gameObject.SetActive(value: true);
		ModalWindowsSounds.Instance.MessageBox.Show.Play();
		m_ModalWindowView.Bind(base.ViewModel.ModalWindowVM);
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: true, FullScreenUIType.EscapeMenu);
		});
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
		m_ModalWindowView.Unbind();
		ModalWindowsSounds.Instance.MessageBox.Hide.Play();
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: false, FullScreenUIType.EscapeMenu);
		});
	}
}
