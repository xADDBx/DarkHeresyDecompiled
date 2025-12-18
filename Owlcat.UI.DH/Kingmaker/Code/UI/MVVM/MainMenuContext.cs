using System;
using System.Collections;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Persistence.Scenes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Settings;
using Kingmaker.Utility.UnityExtensions;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class MainMenuContext : ViewModel
{
	public static MainMenuContext Instance;

	private readonly ReactiveProperty<MainMenuVM> m_MainMenuVM;

	private readonly ReactiveProperty<NewGameVM> m_NewGame;

	private readonly ReactiveProperty<TermsOfUseVM> m_TermsOfUseVM;

	private readonly ReactiveProperty<FeedbackPopupVM> m_FeedbackPopupVM = new ReactiveProperty<FeedbackPopupVM>();

	private bool m_IsChargenMusicTheme;

	private ChargenUnit m_ChargenUnit;

	public readonly CharGenContextVM CharGenContextVM;

	private readonly ReactiveCommand<Unit> m_OpenCreditsCommand = new ReactiveCommand<Unit>();

	private readonly ReactiveCommand<Unit> m_PlayFirstLaunchFXCommand = new ReactiveCommand<Unit>();

	private bool m_EnterGameStarted;

	public Observable<Unit> OpenCreditsCommand => m_OpenCreditsCommand;

	public Observable<Unit> PlayFirstLaunchFXCommand => m_PlayFirstLaunchFXCommand;

	public MainMenuContext(ReactiveProperty<MainMenuVM> mainMenuVM, ReactiveProperty<NewGameVM> newGameVM, ReactiveProperty<TermsOfUseVM> termsOfUseVM)
	{
		m_MainMenuVM = mainMenuVM;
		m_NewGame = newGameVM;
		m_TermsOfUseVM = termsOfUseVM;
		GameUIState.Instance.IsInMainMenuObservable.Subscribe(OnMainMenuStateChanged).AddTo(this);
		Instance = this;
	}

	protected override void OnDispose()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		base.OnDispose();
	}

	private void OnMainMenuStateChanged(bool isInMainMenu)
	{
		if (isInMainMenu)
		{
			InitMainMenu();
		}
		else
		{
			ClearMainMenuState();
		}
	}

	private void InitMainMenu()
	{
		m_MainMenuVM.Value = new MainMenuVM(this).AddTo(this);
		new MainMenuWindowsQueue().Then(delegate(Action next)
		{
			TryShowTermsOfUse(next);
		}).Then(delegate(Action next)
		{
			TryShowStatisticRequest(next);
		}).Run();
	}

	private void ClearMainMenuState()
	{
		DisposeMainMenu();
		DisposeTermsOfUse();
	}

	private void DisposeMainMenu()
	{
		m_MainMenuVM.Value?.Dispose();
		m_MainMenuVM.Value = null;
	}

	private void TryShowTermsOfUse(Action nextAction)
	{
		if (!TermsOfUse.TermsOfUseAccepted)
		{
			ShowTermsOfUse(nextAction);
		}
		else
		{
			nextAction?.Invoke();
		}
	}

	public void ShowTermsOfUse(Action nextAction = null)
	{
		ReactiveProperty<TermsOfUseVM> termsOfUseVM = m_TermsOfUseVM;
		if (termsOfUseVM.Value == null)
		{
			TermsOfUseVM termsOfUseVM3 = (termsOfUseVM.Value = new TermsOfUseVM(delegate
			{
				DisposeTermsOfUse();
				nextAction?.Invoke();
			}));
		}
	}

	private void DisposeTermsOfUse()
	{
		m_TermsOfUseVM.Value?.Dispose();
		m_TermsOfUseVM.Value = null;
	}

	private void TryShowStatisticRequest(Action nextAction)
	{
		if (!SettingsRoot.Game.Main.AskedSendGameStatistic.GetValue())
		{
			ShowGameStatisticRequest(delegate
			{
				nextAction?.Invoke();
			});
		}
		else
		{
			nextAction?.Invoke();
		}
	}

	public void ShowGameStatisticRequest(Action nextAction)
	{
		EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler w)
		{
			w.HandleOpen(UIStrings.Instance.TermsOfUseTexts.GameStatisticEnabledDialogue, DialogMessageBoxType.Dialog, delegate(DialogMessageBoxButton button)
			{
				OnEnableGameStatistic(button, nextAction);
			}, OnLinkInvoke, UIStrings.Instance.TermsOfUseTexts.AcceptBtn, UIStrings.Instance.TermsOfUseTexts.DeclineBtn);
		});
	}

	public void OnEnableGameStatistic(DialogMessageBoxButton buttonType, Action nextAction)
	{
		SettingsRoot.Game.Main.SendGameStatistic.SetValueAndConfirm(buttonType == DialogMessageBoxButton.Yes);
		SettingsController.Instance.SaveAll();
		nextAction?.Invoke();
	}

	public void OnLinkInvoke(TMP_LinkInfo linkInfo)
	{
		switch (linkInfo.GetLinkID())
		{
		case "eula":
			ShowTermsOfUse();
			break;
		case "pp":
			Application.OpenURL("https://owlcatgames.com/privacy");
			break;
		case "upp":
			Application.OpenURL("https://unity3d.com/legal/privacy-policy");
			break;
		}
	}

	private void TryShowFeedbacke(Action nextAction)
	{
		if (!TermsOfUse.TermsOfUseAccepted)
		{
			ShowFeedback(nextAction);
		}
		else
		{
			nextAction?.Invoke();
		}
	}

	public void ShowFeedback(Action nextAction = null)
	{
		ReactiveProperty<FeedbackPopupVM> feedbackPopupVM = m_FeedbackPopupVM;
		if (feedbackPopupVM.Value == null)
		{
			FeedbackPopupVM feedbackPopupVM3 = (feedbackPopupVM.Value = new FeedbackPopupVM(delegate
			{
				DisposeFeedback();
				nextAction?.Invoke();
			}));
		}
	}

	private void DisposeFeedback()
	{
		m_TermsOfUseVM.Value?.Dispose();
		m_TermsOfUseVM.Value = null;
	}

	public void ShowNewGameSetup()
	{
		m_NewGame.Value = new NewGameVM(CloseAction, FinishAction);
		void CloseAction()
		{
			DisposeNewGame();
		}
		void FinishAction()
		{
			DisposeNewGame();
			StartNewGame();
		}
	}

	private void DisposeNewGame()
	{
		m_NewGame.Value?.Dispose();
		m_NewGame.Value = null;
	}

	public void EnterGame(Action action)
	{
		if (m_EnterGameStarted)
		{
			PFLog.UI.Error("Double game start detected!");
		}
		else
		{
			CoroutineRunner.Start(EnterGameCoroutine(action));
		}
	}

	private IEnumerator EnterGameCoroutine(Action action)
	{
		m_EnterGameStarted = true;
		RootUIContext.Instance.CommonVM.CloseTutorialOnLoad();
		RootUIContext.Instance.LoadingScreenRootVM.ShowLoadingScreen();
		yield return null;
		Runner.ShouldStartManually = true;
		yield return SceneLoader.LoadObligatoryScenesAsync();
		action();
		yield return null;
		Runner.StartManually();
		m_EnterGameStarted = false;
		Runner.ShouldStartManually = false;
	}

	public void StartNewGame()
	{
		EnterGame(Game.Instance.LoadNewGame);
	}

	public void LoadLastGame()
	{
		EnterGame(LoadLastSave);
	}

	private void LoadLastSave()
	{
		Game.Instance.SaveManager.UpdateSaveListIfNeeded();
		MainThreadDispatcher.StartCoroutine(UIUtilitySaves.WaitForSaveUpdated(delegate
		{
			SaveInfo latestSave = Game.Instance.SaveManager.GetLatestSave();
			if (latestSave != null)
			{
				Game.Instance.LoadGameFromMainMenu(latestSave);
			}
			else
			{
				Game.Instance.LoadNewGame();
			}
		}));
	}
}
