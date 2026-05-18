using Kingmaker.Blueprints.Area;
using Kingmaker.Code.View.UI.MVVM.BugReport;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Sound;
using Kingmaker.Utility;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class MainMenuVM : ViewModel
{
	public readonly MainMenuButtonsVM MainMenuButtonsVm;

	public readonly MainMenuWelcomeWidgetVM MainMenuWelcomeWidgetVM;

	private MainMenuContext m_MainMenuContext;

	public MainMenuVM(MainMenuContext mainMenuContext)
	{
		m_MainMenuContext = mainMenuContext;
		MainMenuButtonsVm = new MainMenuButtonsVM(this).AddTo(this);
		MainMenuWelcomeWidgetVM = new MainMenuWelcomeWidgetVM(this).AddTo(this);
		MainMenuCrushDump.CheckCrushDumpMessage();
		MainMenuCrushDump.CheckCrushReportingUtils();
	}

	public void LoadLastGame()
	{
		m_MainMenuContext?.EnterGame(Game.Instance.LoadLastSave);
	}

	public void ShowNewGameSetup()
	{
		m_MainMenuContext?.ShowNewGameSetup();
	}

	public void ShowLicense()
	{
		m_MainMenuContext?.ShowTermsOfUse();
	}

	public void ShowFeedback()
	{
		m_MainMenuContext?.ShowFeedback();
	}

	public void SetNewGamePreset(BlueprintAreaPreset newGamePreset)
	{
		Game.NewGamePreset = newGamePreset;
		PFLog.System.Log($"Selected new game preset: {newGamePreset}");
	}

	protected override void OnDispose()
	{
		SoundBanksManager.UnloadVoiceBanks();
		SaveScreenshotManager.Instance.Cleanup();
		m_MainMenuContext = null;
	}

	private void OnCloseFirstLaunchSettingsVM()
	{
	}

	public void ShowNetLobby()
	{
		EventBus.RaiseEvent(delegate(INetLobbyRequest h)
		{
			h.HandleNetLobbyRequest(isMainMenu: true);
		});
	}

	public void ShowDlcManager()
	{
		EventBus.RaiseEvent(delegate(IDlcManagerUIHandler h)
		{
			h.HandleOpenDlcManager();
		});
	}

	public void OpenSettings()
	{
		EventBus.RaiseEvent(delegate(ISettingsUIHandler h)
		{
			h.HandleOpenSettings(isMainMenu: true);
		});
	}

	public void Exit()
	{
		SystemUtil.ApplicationQuit();
	}

	public void ShowCredits()
	{
	}
}
