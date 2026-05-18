using System;
using System.Collections;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Scenes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Settings;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.Visual.Sound;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class MainMenuContext : ViewModel
{
	private readonly ReactiveProperty<MainMenuVM> m_MainMenuVM;

	private readonly ReactiveProperty<NewGameVM> m_NewGame;

	private readonly ReactiveProperty<TermsOfUseVM> m_TermsOfUseVM;

	private readonly ReactiveProperty<FeedbackPopupVM> m_FeedbackPopupVM = new ReactiveProperty<FeedbackPopupVM>();

	private readonly Action m_LoadingHandler;

	private bool m_IsChargenMusicTheme;

	private ChargenUnit m_ChargenUnit;

	public static MainMenuContext Instance;

	private bool m_EnterGameStarted;

	public MainMenuContext(ReactiveProperty<MainMenuVM> mainMenuVM, ReactiveProperty<NewGameVM> newGameVM, ReactiveProperty<TermsOfUseVM> termsOfUseVM, Action loadingHandler)
	{
		m_MainMenuVM = mainMenuVM;
		m_NewGame = newGameVM;
		m_TermsOfUseVM = termsOfUseVM;
		m_LoadingHandler = loadingHandler;
		GameUIState.Instance.IsInMainMenuObservable.Subscribe(OnMainMenuStateChanged).AddTo(this);
		WarmupChargenUnit();
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

	private void TryShowFeedback(Action nextAction)
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
		Game.Instance.StartNewGameProcess();
		void CloseAction()
		{
			Game.Instance.CancelNewGameProcess();
			DisposeNewGame();
		}
		void FinishAction()
		{
			if (m_ChargenUnit == null)
			{
				BlueprintUnit defaultPlayerCharacter = ConfigRoot.Instance.NewGameSettings.DefaultPlayerCharacter;
				m_ChargenUnit = new ChargenUnit(defaultPlayerCharacter);
			}
			else if (m_ChargenUnit.Used)
			{
				m_ChargenUnit.RecreateUnit();
			}
			m_ChargenUnit.Used = true;
			CharGenConfig.Create(m_ChargenUnit.Unit, CharGenMode.NewGame).SetOnComplete(delegate(BaseUnitEntity newUnit)
			{
				Game.NewGameUnit = newUnit;
				Game.Instance.Player.SetMainCharacter(newUnit);
				DisposeNewGame();
			}).SetEnterNewGameAction(delegate
			{
				EnterGame(Game.Instance.LoadNewGame);
			})
				.SetSoundActions(new CharGenSoundActions
				{
					OnOpen = delegate
					{
						UpdateSoundState(MusicStateHandler.MusicState.Chargen);
					},
					OnClose = delegate
					{
						UpdateSoundState(MusicStateHandler.MusicState.MainMenu);
					},
					OnComplete = delegate
					{
						FullScreenSounds.Instance.Chargen.ChargenCompleteClick.Play();
						UpdateSoundState(MusicStateHandler.MusicState.Setting);
					}
				})
				.OpenUI();
		}
	}

	private void UpdateSoundState(MusicStateHandler.MusicState state)
	{
		SoundState.Instance.OnMusicStateChange(state);
	}

	private void DisposeNewGame()
	{
		m_NewGame.Value?.Dispose();
		m_NewGame.Value = null;
	}

	private void WarmupChargenUnit()
	{
		if (m_ChargenUnit == null)
		{
			BlueprintUnit defaultPlayerCharacter = ConfigRoot.Instance.NewGameSettings.DefaultPlayerCharacter;
			m_ChargenUnit = new ChargenUnit(defaultPlayerCharacter);
		}
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
		m_LoadingHandler();
		yield return null;
		Runner.ShouldStartManually = true;
		yield return SceneLoader.LoadObligatoryScenesAsync();
		action();
		yield return null;
		Runner.StartManually();
		m_EnterGameStarted = false;
		Runner.ShouldStartManually = false;
	}
}
