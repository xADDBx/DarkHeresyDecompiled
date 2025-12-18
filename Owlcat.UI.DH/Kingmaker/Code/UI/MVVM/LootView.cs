using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class LootView<TLootCollector, TInteractionSlot, TPlayerStash> : View<LootVM> where TLootCollector : LootCollectorView where TInteractionSlot : InteractionSlotPartView where TPlayerStash : PlayerStashView
{
	[SerializeField]
	private FadeAnimator m_Animator;

	[Header("Collector exit location")]
	[SerializeField]
	protected TLootCollector m_CollectorExitLocation;

	[Header("Collector on location")]
	[SerializeField]
	protected TLootCollector m_CollectorOnLocation;

	[Header("InteractionSlot")]
	[SerializeField]
	protected TInteractionSlot m_InteractionSlot;

	[Header("PlayerStash")]
	[SerializeField]
	protected TPlayerStash m_PlayerStash;

	protected readonly ReactiveCommand<Unit> m_OnPanelsChanged = new ReactiveCommand<Unit>();

	private bool m_RightPanelIsShown;

	public void Awake()
	{
		m_Animator.Initialize();
		m_CollectorExitLocation.Initialize();
		m_CollectorOnLocation.Initialize();
		m_InteractionSlot.Initialize();
		m_PlayerStash.Initialize();
	}

	protected override void OnBind()
	{
		Show();
		m_CollectorExitLocation.Bind(base.ViewModel.LootCollectorExitLocation);
		m_CollectorOnLocation.Bind(base.ViewModel.LootCollectorOnLocation);
		m_InteractionSlot.Bind(base.ViewModel.InteractionSlot);
		m_PlayerStash.Bind(base.ViewModel.PlayerStash);
		if (base.ViewModel.IsOneSlot)
		{
			ShowPanels(showRight: true, force: true);
		}
		else if (base.ViewModel.IsPlayerStash)
		{
			ShowPanels(showRight: true, force: true);
		}
		else
		{
			ShowPanels(showRight: false, force: true);
		}
		base.ViewModel.ExtendedView.Skip(1).Subscribe(delegate(bool extended)
		{
			ShowPanels(extended, extended);
		}).AddTo(this);
		if (base.ViewModel.Mode == LootWindowMode.ZoneExit)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.LootWindow.CollectAllBeforeLeave.Text, addToLog: false, WarningNotificationFormat.Attention);
			});
		}
		Game.Instance.RequestPauseUi(isPaused: true);
	}

	protected override void OnUnbind()
	{
		Game.Instance.RequestPauseUi(isPaused: false);
		Hide();
	}

	private void ShowPanels(bool showRight, bool force = false)
	{
		bool flag = false;
		if (showRight != m_RightPanelIsShown || force)
		{
			m_RightPanelIsShown = showRight;
			flag = true;
		}
		if (flag)
		{
			m_OnPanelsChanged.Execute();
		}
	}

	private void Show()
	{
		m_Animator.AppearAnimation();
		UISounds.Instance.Sounds.Loot.GetLootWindowOpenSound(base.ViewModel.Mode).Play();
	}

	private void Hide()
	{
		ContextMenuHelper.HideContextMenu();
		m_Animator.DisappearAnimation();
		UISounds.Instance.Sounds.Loot.GetLootWindowCloseSound(base.ViewModel.Mode).Play();
	}
}
