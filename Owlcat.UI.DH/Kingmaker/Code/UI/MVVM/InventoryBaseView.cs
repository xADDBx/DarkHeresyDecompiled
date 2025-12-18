using Kingmaker.Code.View.UI.Components.Camera;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common.Animations;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.UI.MVVM;

public abstract class InventoryBaseView<T> : View<InventoryVM>, IInventoryNotificationHandler, ISubscriber where T : InventoryEquipSlotView
{
	[Header("Views")]
	[SerializeField]
	protected InventoryDollView<T> m_DollView;

	[SerializeField]
	protected InventoryStashView m_StashView;

	[SerializeField]
	protected PartyPCWindowsView m_PartyView;

	[SerializeField]
	protected InventoryWarning m_WarningPrefab;

	[Header("Screen")]
	[SerializeField]
	[FormerlySerializedAs("UIPostProcessMember")]
	private UIPostProcessMember m_UIPostProcessMember;

	[SerializeField]
	private FadeAnimator m_ScreenContentFadeAnimator;

	[SerializeField]
	private GameObject m_ShatteredGlass;

	private InventoryWarning m_CurrentWarning;

	private bool m_IsInit;

	private bool m_SuppressHideAnimation;

	private Coroutine m_DisposeCoroutine;

	public void Initialize()
	{
		m_PartyView.Initialize();
		m_DollView.Initialize();
		m_StashView.Initialize();
		m_ScreenContentFadeAnimator.Initialize();
		base.gameObject.SetActive(value: false);
	}

	protected override void OnBind()
	{
		ShowWindow();
		m_UIPostProcessMember.Bind();
		m_PartyView.Bind(base.ViewModel.PartyVM);
		m_DollView.Bind(base.ViewModel.DollVM);
		m_StashView.Bind(base.ViewModel.StashVM);
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.SuppressHideAnimation, delegate
		{
			m_SuppressHideAnimation = true;
		}).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void OnUnbind()
	{
		HideWindow();
		m_DollView.ClearViewIfNeeded();
		m_SuppressHideAnimation = false;
		if ((bool)m_CurrentWarning)
		{
			WidgetFactory.DisposeWidget(m_CurrentWarning);
		}
	}

	private void ShowWindow()
	{
		base.gameObject.SetActive(value: true);
		m_ShatteredGlass.SetActive(value: true);
		UIPostProcessingAnimator.Instance.Or(null)?.PlayState(UIPostEffectState.Default);
		m_ScreenContentFadeAnimator.AppearAnimation();
	}

	private void HideWindow()
	{
		ContextMenuHelper.HideContextMenu();
		EventBus.RaiseEvent(delegate(ICounterWindowUIHandler h)
		{
			h.HandleCloseCounterWindow();
		});
		m_ShatteredGlass.SetActive(value: false);
		if (m_SuppressHideAnimation)
		{
			base.gameObject.SetActive(value: false);
			m_UIPostProcessMember.Dispose();
			return;
		}
		m_ScreenContentFadeAnimator.DisappearAnimation(delegate
		{
			base.gameObject.SetActive(value: false);
			ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
			{
				m_UIPostProcessMember?.Dispose();
			});
		});
		UIPostProcessingAnimator.Instance.Or(null)?.PlayState(UIPostEffectState.Off);
	}

	public void HandleWarning(string text)
	{
		float value = 0f;
		if ((bool)m_CurrentWarning && m_CurrentWarning.ViewModel != null)
		{
			value = m_CurrentWarning.DisappearTime;
			WidgetFactory.DisposeWidget(m_CurrentWarning);
		}
		ObservableSubscribeExtensions.Subscribe(Observable.Timer(value.Seconds(), UnityTimeProvider.UpdateIgnoreTimeScale), delegate
		{
			m_CurrentWarning = WidgetFactory.GetWidget(m_WarningPrefab);
			m_CurrentWarning.transform.SetParent(base.transform, worldPositionStays: false);
			m_CurrentWarning.Bind(text);
		}).AddTo(this);
	}
}
