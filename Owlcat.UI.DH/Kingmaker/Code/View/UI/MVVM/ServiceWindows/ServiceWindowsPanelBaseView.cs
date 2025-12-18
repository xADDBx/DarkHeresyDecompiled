using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.Components.Animations;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Transitions;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.ServiceWindows;

public abstract class ServiceWindowsPanelBaseView : View<ServiceWindowsPanelVM>, ITransitable
{
	[SerializeField]
	private MoveAnimator m_MoveAnimator;

	[SerializeField]
	private GraphicColorAnimator m_BackgroundFadeAnimator;

	[Space]
	[SerializeField]
	protected CharInfoNameAndPortraitBaseView CharInfoView;

	private UIAnimatorTransition m_ShowTransition;

	private UIAnimatorTransition m_HideTransition;

	private FullScreenUIType m_PreviousScreenType;

	private FullScreenUIType m_CurrentScreenType;

	private readonly IReadOnlyDictionary<FullScreenUIType, float> m_DisappearDelays = new Dictionary<FullScreenUIType, float>
	{
		{
			FullScreenUIType.Inventory,
			0.3f
		},
		{
			FullScreenUIType.CharacterScreen,
			0.3f
		},
		{
			FullScreenUIType.DetectiveJournal,
			0.3f
		},
		{
			FullScreenUIType.LocalMap,
			0.3f
		}
	};

	Transition ITransitable.Show()
	{
		base.gameObject.SetActive(value: true);
		if (m_ShowTransition == null)
		{
			m_ShowTransition = new UIAnimatorShowTransition(m_MoveAnimator);
		}
		m_BackgroundFadeAnimator.AppearAnimation();
		return m_ShowTransition.Run(CompleteShowTransition);
	}

	Transition ITransitable.Hide()
	{
		if (m_HideTransition == null)
		{
			m_HideTransition = new UIAnimatorHideTransition(m_MoveAnimator);
		}
		m_DisappearDelays.TryGetValue(m_PreviousScreenType, out var value);
		m_MoveAnimator.SetDisappearDelay(value);
		m_BackgroundFadeAnimator.DisappearAnimation();
		return m_HideTransition.Run(CompleteHideTransition);
	}

	protected virtual void OnShowTransitionCompleted()
	{
	}

	protected override void OnBind()
	{
		base.ViewModel.CurrentUIType.Subscribe(delegate(FullScreenUIType value)
		{
			EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
			{
				h.HandleFullScreenUiChanged(state: true, value);
			});
			Game.Instance.RequestPauseUi(value != FullScreenUIType.LocalMap);
			m_PreviousScreenType = m_CurrentScreenType;
			m_CurrentScreenType = value;
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		Game.Instance.RequestPauseUi(isPaused: false);
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: false, base.ViewModel.CurrentUIType.CurrentValue);
		});
	}

	private void CompleteShowTransition()
	{
		base.ViewModel.CharInfoAndPortraitVM.Subscribe(CharInfoView.Bind).AddTo(this);
		OnShowTransitionCompleted();
	}

	private void CompleteHideTransition()
	{
		base.gameObject.SetActive(value: false);
	}
}
