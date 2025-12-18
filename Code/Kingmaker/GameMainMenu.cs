using Kingmaker.Code.UI.MVVM;
using Kingmaker.Utility;
using Kingmaker.Utility.CommandLineArgs;
using Kingmaker.Visual;
using Kingmaker.Visual.Sound;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kingmaker;

public class GameMainMenu : MonoBehaviour
{
	private bool m_GraphicsSettingsApplied;

	private bool MainMenuIsLoaded => SceneManager.GetSceneByName(GameScenes.MainMenu).isLoaded;

	private void Awake()
	{
		PFLog.System.Log($"Awake: {base.gameObject.name} @ frame {Time.frameCount}");
	}

	private void Start()
	{
		PFLog.System.Log($"Start: {base.gameObject.name} @ frame {Time.frameCount}");
		if (MainMenuIsLoaded)
		{
			PFLog.System.Log("MainMenu.Start()");
			if (CommandLineArguments.Parse().Contains("skipmainmenu"))
			{
				Game.Instance.SaveManager.UpdateSaveListIfNeeded();
				Game.Instance.RootUIContext.EnterGame(Game.Instance.LoadNewGame);
			}
			SceneManager.SetActiveScene(SceneManager.GetSceneByName(GameScenes.MainMenu));
			GameUIState.Instance.SetMainMenuActive(state: true);
			GameHeapSnapshot.MainMenuSnapshot();
		}
	}

	private void Update()
	{
		SoundState.Instance.UpdateScheduledAreaMusic();
		Game.Instance.Statistic.Tick(Game.Instance, isMainMenu: true);
		if (!m_GraphicsSettingsApplied && !CommandLineArguments.Parse().Contains("menu-default-graphics"))
		{
			PFLog.System.Log("MainMenu.Update(): Applying Graphics Settings");
			m_GraphicsSettingsApplied = true;
			if ((bool)RenderingManager.Instance)
			{
				RenderingManager.Instance.ApplySettings();
			}
			PFLog.System.Log("MainMenu.Update(): Applied Graphics Settings");
		}
	}

	private void OnDisable()
	{
		GameUIState.Instance.SetMainMenuActive(state: false);
	}
}
