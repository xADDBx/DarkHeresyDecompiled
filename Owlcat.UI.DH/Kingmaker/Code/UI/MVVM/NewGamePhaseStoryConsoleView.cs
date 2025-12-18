using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM;

public class NewGamePhaseStoryConsoleView : NewGamePhaseStoryBaseView
{
	[SerializeField]
	private NewGamePhaseStoryScenarioSelectorConsoleView m_StorySelectorConsoleView;

	[SerializeField]
	private CustomUIVideoPlayerConsoleView m_CustomUIVideoPlayerConsoleView;

	[SerializeField]
	private ConsoleHint m_ScrollStoryHint;

	[SerializeField]
	private ConsoleHint m_PurchaseHint;

	[SerializeField]
	private ConsoleHint m_InstallHint;

	public override void Initialize()
	{
		base.Initialize();
		if (!IsInit)
		{
			m_CustomUIVideoPlayerConsoleView.Initialize();
			IsInit = true;
		}
	}

	protected override void OnBind()
	{
		m_CustomUIVideoPlayerConsoleView.Bind(base.ViewModel.CustomUIVideoPlayerVM);
		base.OnBind();
		m_StorySelectorConsoleView.Bind(base.ViewModel.SelectionGroup);
	}

	public void Scroll(InputActionEventData arg1, float x)
	{
		Scroll(x);
	}

	private void Scroll(float x)
	{
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.scrollDelta = new Vector2(0f, x * m_ScrollRect.scrollSensitivity);
		m_ScrollRect.OnSmoothlyScroll(pointerEventData);
	}

	public void CreateInputImpl(InputLayer inputLayer, ConsoleHintsWidget hintsWidget, ConsoleHint switchOnOffHint, ConsoleHint purchaseHint, ConsoleHint installHint, ConsoleHint deleteDlcHint, ConsoleHint playPauseVideoHint)
	{
		if (m_ScrollStoryHint != null)
		{
			m_ScrollStoryHint.BindCustomAction(3, inputLayer, base.ViewModel.IsEnabled).AddTo(this);
		}
		if (m_PurchaseHint != null)
		{
			m_PurchaseHint.BindCustomAction(10, inputLayer, base.ViewModel.IsEnabled.And(base.ViewModel.DlcIsBought.Not()).And(SwitchOnButtonActive.Not()).ToReadOnlyReactiveProperty(initialValue: false)).AddTo(this);
		}
		if (m_InstallHint != null)
		{
			m_InstallHint.BindCustomAction(11, inputLayer, base.ViewModel.IsEnabled.And(SwitchOnButtonActive.Not()).And(base.ViewModel.DlcIsBoughtAndNotInstalled).And(base.ViewModel.DownloadingInProgress.Not())
				.And(base.ViewModel.IsRealConsole)
				.ToReadOnlyReactiveProperty(initialValue: false)).AddTo(this);
		}
		switchOnOffHint.Bind(inputLayer.AddButton(delegate
		{
			base.ViewModel.SwitchDlcOn();
		}, 10, base.ViewModel.IsEnabled.And(SwitchOnButtonActive).ToReadOnlyReactiveProperty(initialValue: false))).AddTo(this);
		switchOnOffHint.SetLabel(UIStrings.Instance.SettingsUI.SettingsToggleOff);
		installHint.Bind(inputLayer.AddButton(delegate
		{
			base.ViewModel.InstallDlc();
		}, 11, base.ViewModel.IsEnabled.And(SwitchOnButtonActive.Not()).And(base.ViewModel.DlcIsBoughtAndNotInstalled).And(base.ViewModel.DownloadingInProgress.Not())
			.And(base.ViewModel.IsRealConsole)
			.ToReadOnlyReactiveProperty(initialValue: false))).AddTo(this);
		installHint.SetLabel(UIStrings.Instance.DlcManager.Install);
		deleteDlcHint.Bind(inputLayer.AddButton(delegate
		{
			base.ViewModel.DeleteDlc();
		}, 11, base.ViewModel.IsEnabled.And(SwitchOnButtonActive).And(base.ViewModel.DlcIsBoughtAndNotInstalled.Not()).And(base.ViewModel.DownloadingInProgress.Not())
			.And(base.ViewModel.IsRealConsole)
			.ToReadOnlyReactiveProperty(initialValue: false))).AddTo(this);
		deleteDlcHint.SetLabel(UIStrings.Instance.DlcManager.DeleteDlc);
		base.ViewModel.DlcIsOn.Subscribe(delegate(bool value)
		{
			switchOnOffHint.SetLabel(value ? UIStrings.Instance.SettingsUI.SettingsToggleOff : UIStrings.Instance.SettingsUI.SettingsToggleOn);
		}).AddTo(this);
		purchaseHint.Bind(inputLayer.AddButton(delegate
		{
			base.ViewModel.ShowInStore();
		}, 10, base.ViewModel.IsEnabled.And(base.ViewModel.DlcIsBought.Not()).And(SwitchOnButtonActive.Not()).ToReadOnlyReactiveProperty(initialValue: false))).AddTo(this);
		purchaseHint.SetLabel(UIStrings.Instance.DlcManager.Purchase);
		m_CustomUIVideoPlayerConsoleView.CreateInputImpl(inputLayer, hintsWidget, playPauseVideoHint, base.ViewModel.IsEnabled);
	}

	public List<IConsoleEntity> GetNavigationEntities()
	{
		return m_StorySelectorConsoleView.GetNavigationEntities();
	}

	protected override void ShowHideVideoImpl(bool state)
	{
		base.ShowHideVideoImpl(state);
		m_CustomUIVideoPlayerConsoleView.gameObject.SetActive(state);
	}
}
