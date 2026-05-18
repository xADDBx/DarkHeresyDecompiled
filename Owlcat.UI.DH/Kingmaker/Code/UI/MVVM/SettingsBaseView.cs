using Kingmaker.Code.UI.MVVM.Common;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Kingmaker.UI.Transitions;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class SettingsBaseView : View<SettingsVM>, ITransitable
{
	[Header("Views")]
	[SerializeField]
	protected VirtualListVertical m_VirtualList;

	[SerializeField]
	protected InfoSectionView m_InfoView;

	[SerializeField]
	protected SettingsMenuSelectorBaseView m_MenuSelector;

	[SerializeField]
	protected FlexibleLensSelectorView m_SelectorView;

	[Header("Animator")]
	[SerializeField]
	protected FadeAnimator m_Animator;

	[Header("Screen")]
	[SerializeField]
	private UIServiceWindowPostProcessView m_PostProcessView;

	private bool m_IsShowed;

	public virtual void Awake()
	{
		base.gameObject.SetActive(value: false);
		m_Animator.Initialize();
		m_MenuSelector.Initialize();
		m_PostProcessView.Initialize();
	}

	protected override void OnBind()
	{
		m_MenuSelector.Bind(base.ViewModel.SelectionGroup);
		m_SelectorView.Bind(base.ViewModel.Selector);
		m_VirtualList.Subscribe(base.ViewModel.SettingEntities).AddTo(this);
		m_InfoView.Bind(base.ViewModel.InfoVM);
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.OnSwitchSettings, delegate
		{
			OnSelectedMenuEntity(base.ViewModel.SelectedMenuEntity.CurrentValue);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.LanguageChanged, delegate
		{
			UpdateLocalization();
		}).AddTo(this);
		Game.Instance.RequestPauseUi(isPaused: true);
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: true, FullScreenUIType.Settings);
		});
	}

	protected override void OnUnbind()
	{
		Game.Instance.RequestPauseUi(isPaused: false);
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: false, FullScreenUIType.Settings);
		});
	}

	Transition ITransitable.Show()
	{
		if (m_IsShowed)
		{
			return Transition.None;
		}
		m_IsShowed = true;
		base.gameObject.SetActive(value: true);
		UISounds.Instance.Sounds.ServiceWindowsSounds.PlayOpenSound(ServiceWindowsType.LocalMap);
		return new UIAnimatorShowTransition(m_Animator).Run(CompleteShowTransition);
	}

	Transition ITransitable.Hide()
	{
		if (!m_IsShowed)
		{
			return Transition.None;
		}
		UISounds.Instance.Sounds.ServiceWindowsSounds.PlayCloseSound(ServiceWindowsType.LocalMap);
		m_PostProcessView.Hide(immediate: false);
		return new UIAnimatorHideTransition(m_Animator).Run(CompleteHideTransition);
	}

	private void CompleteShowTransition()
	{
		m_PostProcessView.ShowFrom(UIPostEffectState.Off);
	}

	private void CompleteHideTransition()
	{
		base.gameObject.SetActive(value: false);
		m_IsShowed = false;
	}

	protected abstract void OnSelectedMenuEntity(SettingsMenuEntityVM entity);

	protected abstract void UpdateLocalization();
}
