using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.DLC;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Settings;
using Kingmaker.Stores.DlcInterfaces;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.BuildModeUtils;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public abstract class EscMenuBaseView : View<EscMenuVM>
{
	[Header("Common Buttons")]
	[SerializeField]
	protected OwlcatMultiButton m_SaveButton;

	[SerializeField]
	protected OwlcatMultiButton m_LoadButton;

	[SerializeField]
	protected OwlcatMultiButton m_MultiplayerButton;

	[SerializeField]
	protected OwlcatMultiButton m_MultiplayerRolesButton;

	[SerializeField]
	protected OwlcatMultiButton m_FormationButton;

	[SerializeField]
	protected OwlcatMultiButton m_OptionsButton;

	[SerializeField]
	protected OwlcatMultiButton m_ModsButton;

	[SerializeField]
	protected OwlcatMultiButton m_BugReportButton;

	[SerializeField]
	protected OwlcatMultiButton m_MainMenuButton;

	[SerializeField]
	protected OwlcatMultiButton m_QuitButton;

	[Header("Common Labels")]
	[SerializeField]
	private TextMeshProUGUI m_SaveButtonLabel;

	[SerializeField]
	private TextMeshProUGUI m_LoadButtonLabel;

	[SerializeField]
	private TextMeshProUGUI m_MultiplayerButtonLabel;

	[SerializeField]
	private TextMeshProUGUI m_MultiplayerRolesButtonLabel;

	[SerializeField]
	private TextMeshProUGUI m_FormationButtonLabel;

	[SerializeField]
	private TextMeshProUGUI m_OptionsButtonLabel;

	[SerializeField]
	private TextMeshProUGUI m_ModsButtonLabel;

	[SerializeField]
	private TextMeshProUGUI m_BugReportButtonLabel;

	[SerializeField]
	private TextMeshProUGUI m_MainMenuButtonLabel;

	[SerializeField]
	private TextMeshProUGUI m_QuitButtonLabel;

	[Header("Another")]
	[SerializeField]
	private TextMeshProUGUI m_CanSwitchOnDlcsCount;

	[SerializeField]
	private TextMeshProUGUI m_CurrentLocationText;

	[SerializeField]
	private TextMeshProUGUI m_AreaNameText;

	protected GridConsoleNavigationBehaviour NavigationBehaviour;

	protected InputLayer InputLayer;

	public static readonly string InputLayerContextName = "EscMenu";

	public void Awake()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void OnBind()
	{
		Game.Instance.RequestPauseUi(isPaused: true);
		base.gameObject.SetActive(value: true);
		m_CurrentLocationText.text = UIStrings.Instance.EscapeMenu.CurrentAreaLabel.Text + ":";
		UISounds.Instance.Sounds.Systems.FullscreenWindowFadeShow.Play();
		base.ViewModel.AreaName.Subscribe(delegate(string value)
		{
			m_AreaNameText.SetText(value);
		}).AddTo(this);
		_ = BuildModeUtility.IsCoopEnabled;
		m_MultiplayerButton.gameObject.SetActive(value: false);
		m_MultiplayerRolesButton.gameObject.SetActive(value: false);
		m_MultiplayerRolesButton.SetInteractable(state: false);
		m_QuitButton.gameObject.SetActive(value: true);
		ObservableSubscribeExtensions.Subscribe(m_QuitButton.OnLeftClickAsObservable(), delegate
		{
			LeftClickAndHideFocusAction(delegate
			{
				base.ViewModel.OnQuit();
			});
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_QuitButton.OnConfirmClickAsObservable(), delegate
		{
			base.ViewModel.OnQuit();
		}).AddTo(this);
		bool isActive = PhotonManager.Lobby.IsActive;
		m_ModsButton.gameObject.SetActive(!isActive);
		IEnumerable<IBlueprintDlc> source = Game.Instance.Player.GetAvailableAdditionalContentDlcForCurrentCampaign().Where(delegate(IBlueprintDlc dlc)
		{
			BlueprintDlc blueprintDlc = dlc as BlueprintDlc;
			return (blueprintDlc == null || !blueprintDlc.CheckIsLateToSwitch()) && !(blueprintDlc?.GetDlcSwitchOnOffState() ?? false);
		});
		m_CanSwitchOnDlcsCount.transform.parent.gameObject.SetActive(!isActive && source.Any());
		if (!isActive && source.Any())
		{
			m_CanSwitchOnDlcsCount.text = source.Count().ToString();
		}
		if (!isActive)
		{
			ObservableSubscribeExtensions.Subscribe(m_ModsButton.OnLeftClickAsObservable(), delegate
			{
				base.ViewModel.OnMods();
			}).AddTo(this);
			ObservableSubscribeExtensions.Subscribe(m_ModsButton.OnConfirmClickAsObservable(), delegate
			{
				base.ViewModel.OnMods();
			}).AddTo(this);
		}
		SetButtonsTexts();
		ObservableSubscribeExtensions.Subscribe(m_SaveButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnSave();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_LoadButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnLoad();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_OptionsButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnSettings();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_BugReportButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnBugReport();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_MainMenuButton.OnLeftClickAsObservable(), delegate
		{
			LeftClickAndHideFocusAction(delegate
			{
				base.ViewModel.OnMainMenu();
			});
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_SaveButton.OnConfirmClickAsObservable(), delegate
		{
			base.ViewModel.OnSave();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_LoadButton.OnConfirmClickAsObservable(), delegate
		{
			base.ViewModel.OnLoad();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_OptionsButton.OnConfirmClickAsObservable(), delegate
		{
			base.ViewModel.OnSettings();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_BugReportButton.OnConfirmClickAsObservable(), delegate
		{
			base.ViewModel.OnBugReport();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_MainMenuButton.OnConfirmClickAsObservable(), delegate
		{
			base.ViewModel.OnMainMenu();
		}).AddTo(this);
		m_SaveButton.SetInteractable(base.ViewModel.IsSavingAllowed);
		m_OptionsButton.SetInteractable(base.ViewModel.IsOptionsAllowed);
		if ((bool)SettingsRoot.Difficulty.OnlyOneSave)
		{
			m_SaveButton.SetHint(UIStrings.Instance.SaveLoadTexts.SaveIsNotPossibleInIronMan).AddTo(this);
		}
		BuildNavigation();
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.UpdateButtonsInteractable, delegate
		{
			UpdateInteractableButtons();
		}).AddTo(this);
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: true, FullScreenUIType.EscapeMenu);
		});
	}

	protected override void OnUnbind()
	{
		if (!base.ViewModel.InternalWindowOpened)
		{
			Game.Instance.RequestPauseUi(isPaused: false);
		}
		base.ViewModel.InternalWindowOpened = false;
		base.gameObject.SetActive(value: false);
		UISounds.Instance.Sounds.Systems.FullscreenWindowFadeHide.Play();
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: false, FullScreenUIType.EscapeMenu);
		});
	}

	private void LeftClickAndHideFocusAction(Action action)
	{
		base.ViewModel.TriggerUpdateButtonsFocus();
		action();
	}

	private void UpdateInteractableButtons()
	{
		if (BuildModeUtility.IsCoopEnabled)
		{
			m_MultiplayerRolesButton.SetInteractable(!Game.Instance.IsSpaceCombat && base.ViewModel.IsSavingAllowed);
		}
		m_SaveButton.SetInteractable(base.ViewModel.IsSavingAllowed);
		m_OptionsButton.SetInteractable(base.ViewModel.IsOptionsAllowed);
		UpdateInteractableButtonsImpl();
	}

	protected virtual void UpdateInteractableButtonsImpl()
	{
	}

	private void BuildNavigation()
	{
		NavigationBehaviour = new GridConsoleNavigationBehaviour();
		NavigationBehaviour.AddTo(this);
		List<OwlcatMultiButton> list = new List<OwlcatMultiButton> { m_SaveButton, m_LoadButton, m_OptionsButton, m_BugReportButton, m_MainMenuButton, m_QuitButton };
		if (!PhotonManager.Lobby.IsActive)
		{
			list.Add(m_ModsButton);
		}
		if (BuildModeUtility.IsCoopEnabled)
		{
			list.Add(m_MultiplayerButton);
			list.Add(m_MultiplayerRolesButton);
		}
		list.Sort((OwlcatMultiButton x, OwlcatMultiButton y) => x.transform.GetSiblingIndex().CompareTo(y.transform.GetSiblingIndex()));
		NavigationBehaviour.SetEntitiesVertical(new List<IConsoleEntity>(list));
		BuildNavigationImpl(NavigationBehaviour);
		InputLayer = NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = InputLayerContextName
		});
		CreateInputImpl(InputLayer);
		GamePad.Instance.PushLayer(InputLayer).AddTo(this);
	}

	protected virtual void BuildNavigationImpl(GridConsoleNavigationBehaviour navigationBehaviour)
	{
	}

	protected virtual void CreateInputImpl(InputLayer inputLayer)
	{
		EscHotkeyManager.Instance.Subscribe(base.ViewModel.OnClose).AddTo(this);
	}

	protected virtual void SetButtonsTexts()
	{
		m_SaveButtonLabel.text = UIStrings.Instance.EscapeMenu.EscMenuSaveGame;
		m_LoadButtonLabel.text = UIStrings.Instance.EscapeMenu.EscMenuLoadGame;
		m_MultiplayerButtonLabel.text = UIStrings.Instance.EscapeMenu.EscMenuMultiplayer;
		m_MultiplayerRolesButtonLabel.text = UIStrings.Instance.EscapeMenu.EscMenuRoles;
		m_QuitButtonLabel.text = UIStrings.Instance.EscapeMenu.EscMenuExit;
		m_OptionsButtonLabel.text = UIStrings.Instance.EscapeMenu.EscMenuOptions;
		m_BugReportButtonLabel.text = UIStrings.Instance.EscapeMenu.EscMenuBugReport;
		m_MainMenuButtonLabel.text = UIStrings.Instance.EscapeMenu.EscMenuMainMenu;
		bool flag = false;
		m_ModsButtonLabel.text = (flag ? UIStrings.Instance.DlcManager.DlcManagerLabel : UIStrings.Instance.DlcManager.ModsLabel);
	}
}
