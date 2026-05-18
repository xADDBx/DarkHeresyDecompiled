using System;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Cutscenes.Commands;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Gameplay.Controllers;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Dialog;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameModes;
using Kingmaker.Gameplay.Features.Morale;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.Settings.Graphics;
using Kingmaker.Sound.Base;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.Locator;
using UnityEngine;

namespace Kingmaker.Visual.Sound;

public class SoundState : IService, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IQuestHandler, IUnitLifeStateChanged, ISubscriber<IAbstractUnitEntity>, IAreaLoadingStagesHandler, IAreaHandler, IAwarenessHandler, ISubscriber<IMapObjectEntity>, ITimeOfDayChangedHandler, IAreaPartHandler, IPartyCombatHandler, IFullScreenUIHandler, IModalWindowUIHandler, IPowerBalanceHandler, ITurnBasedModeHandler, ITurnBasedModeResumeHandler, IPreparationTurnBeginHandler, IPreparationTurnEndHandler, IMoraleVictoryConfirmationRequest, IMoraleVictoryConfirmationHandler
{
	private enum DialogStates
	{
		Play,
		Pause,
		Stop
	}

	private const float MusicChangeDelay = 2f;

	private const float DefaultCameraZoom = 0f;

	private const float CameraZoomMultiplier = 100f;

	public const string GameAudioStateGroupName = "GameAudioState";

	private static ServiceProxy<SoundState> s_Proxy;

	private SoundStateType m_State;

	private static SoundState s_Instance;

	private bool m_CombatTriggered;

	private bool m_WasPaused;

	private CameraZoom m_CameraZoomComponent;

	private float m_CameraZoom;

	private float m_CurrentCameraZoom;

	[CanBeNull]
	private BlueprintArea m_ScheduledMusicArea;

	private bool m_ScheduledMainMenuMusic;

	private float m_ChangeMusicTime;

	private FullScreenUIType m_UIType;

	private ModalWindowUIType m_ModalWindowUIType;

	private DialogStates m_DialogState = DialogStates.Stop;

	private DialogStates m_BarksState = DialogStates.Stop;

	public static SoundState Instance
	{
		get
		{
			s_Proxy = ((s_Proxy?.Instance != null) ? s_Proxy : Services.GetProxy<SoundState>());
			return s_Proxy?.Instance;
		}
	}

	ServiceLifetimeType IService.Lifetime => ServiceLifetimeType.Game;

	public SoundStateType State => m_State;

	public MusicStateHandler MusicStateHandler { get; } = new MusicStateHandler();


	private SoundEventsManager SoundEvents => SoundEventsManager.Instance;

	private bool MusicChangeScheduled => m_ScheduledMusicArea != null;

	public bool OpenedCaseWasShowBefore { get; set; }

	public SoundState()
	{
		EventBus.Subscribe(this);
		MonoSingleton<ApplicationFocusObserver>.Instance.OnApplicationChangedFocus += OnApplicationFocusChanged;
	}

	public void Update()
	{
		SetState();
		UpdateScheduledAreaMusic();
		UpdateCameraZoom();
		SoundEvents.Update();
	}

	private void SetState()
	{
		GameModeType currentModeType = Game.Instance.CurrentModeType;
		if (!(currentModeType == GameModeType.None))
		{
			ResetState(CalculateState(currentModeType));
			if (Time.timeScale == 0f != m_WasPaused)
			{
				m_WasPaused = Time.timeScale == 0f;
				SoundEventsManager.PostEvent(m_WasPaused ? "PauseFX" : "ResumeFX", null);
			}
		}
	}

	private SoundStateType CalculateState(GameModeType mode)
	{
		if (CommandPlayVideo.Flag.Value)
		{
			return SoundStateType.Video;
		}
		if (LoadingProcess.Instance.IsLoadingInProcess || LoadingProcess.Instance.IsLoadingScreenActive)
		{
			return SoundStateType.LoadingScreen;
		}
		if (m_ModalWindowUIType != 0)
		{
			if (m_ModalWindowUIType != ModalWindowUIType.GameEndingTitles)
			{
				return SoundStateType.InGameMenu;
			}
			return SoundStateType.CreditsAfterEpilogues;
		}
		if (m_UIType == FullScreenUIType.DetectiveJournal)
		{
			return SoundStateType.DetectiveBoard;
		}
		if (m_UIType != 0 || mode == GameModeType.BugReport)
		{
			return SoundStateType.InGameMenu;
		}
		if (mode == GameModeType.Dialog)
		{
			DialogController dialogController = Game.Instance.Controllers.DialogController;
			SoundStateType soundStateType = ((dialogController != null && dialogController.Dialog?.Type == DialogType.Book) ? SoundStateType.BookQuest : SoundStateType.Dialog);
			if (soundStateType == SoundStateType.Dialog && m_State == SoundStateType.LoadingScreen)
			{
				return SoundStateType.Default;
			}
			return soundStateType;
		}
		if (mode == GameModeType.Cutscene)
		{
			return SoundStateType.CutScene;
		}
		if (mode == GameModeType.GlobalMap || mode == GameModeType.CutsceneGlobalMap)
		{
			return SoundStateType.GlobalMap;
		}
		if (mode == GameModeType.GameOver)
		{
			return SoundStateType.Death;
		}
		if (Game.Instance.Player.IsInCombat || mode == GameModeType.SpaceCombat)
		{
			return SoundStateType.Combat;
		}
		if (Game.Instance.IsPaused)
		{
			return SoundStateType.InGamePause;
		}
		return SoundStateType.Default;
	}

	private void UpdateCameraZoom()
	{
		if (m_CameraZoomComponent == null)
		{
			m_CameraZoomComponent = CameraRig.Instance?.CameraZoom;
		}
		m_CameraZoom = ((m_CameraZoomComponent != null) ? m_CameraZoomComponent.CurrentNormalizePosition : 0f);
		if (!(Math.Abs(m_CameraZoom - m_CurrentCameraZoom) < 0.005f))
		{
			m_CurrentCameraZoom = m_CameraZoom;
			AkUnitySoundEngine.SetRTPCValue("CameraZoom", m_CameraZoom * 100f);
		}
	}

	public void ResetState(SoundStateType state)
	{
		if (state != m_State)
		{
			AkUnitySoundEngine.SetState("GameAudioState", state.ToString());
			if (state == SoundStateType.Death)
			{
				MusicStateHandler.SetMusicSettingState(MusicStateHandler.MusicSettingState.Death);
			}
			if (state == SoundStateType.Default || state == SoundStateType.Dialog || (state == SoundStateType.LoadingScreen && LoadingProcess.Instance.CurrentProcessTag != LoadingProcessTag.Save))
			{
				MusicStateHandler.SetMusicSettingState(MusicStateHandler.MusicSettingState.Exploration);
			}
			if (state == SoundStateType.Combat)
			{
				MusicStateHandler.SetMusicSettingState(MusicStateHandler.MusicSettingState.Combat);
			}
			if (state == SoundStateType.GlobalMap)
			{
				AkUnitySoundEngine.SetRTPCValue("PartyBanterPositioning", 1f);
			}
			if (state == SoundStateType.Default)
			{
				AkUnitySoundEngine.SetRTPCValue("PartyBanterPositioning", 0f);
			}
			if ((m_State == SoundStateType.Dialog && m_UIType != FullScreenUIType.Chargen) || m_State == SoundStateType.CutScene || state == SoundStateType.LoadingScreen)
			{
				MusicStateHandler.ResetStoryMode();
			}
			UpdateDialogState(state, m_State);
			UpdateBarksState(state, m_State);
			m_State = state;
			if (state == SoundStateType.MainMenu)
			{
				MusicStateHandler.SetMusicState(MusicStateHandler.MusicState.MainMenu);
			}
		}
	}

	private void UpdateDialogState(SoundStateType newState, SoundStateType oldState)
	{
		if (newState != SoundStateType.Dialog && m_DialogState == DialogStates.Play)
		{
			m_DialogState = DialogStates.Pause;
			SoundEventsManager.PostEvent("PauseDialog", null);
		}
		if (newState == SoundStateType.Dialog && m_DialogState == DialogStates.Pause)
		{
			m_DialogState = DialogStates.Play;
			SoundEventsManager.PostEvent("ResumeDialog", null);
		}
	}

	private void UpdateBarksState(SoundStateType newState, SoundStateType oldState)
	{
		if (newState != SoundStateType.Default && newState != SoundStateType.Combat)
		{
			if (m_BarksState == DialogStates.Play)
			{
				m_BarksState = DialogStates.Pause;
				Game.Instance.Controllers.VoiceOverController.PauseBarks();
			}
		}
		else if (m_BarksState != 0)
		{
			m_BarksState = DialogStates.Play;
			Game.Instance.Controllers.VoiceOverController.ResumeBarks();
		}
	}

	public void ResetBeforeUnloading()
	{
		SoundEventsManager.PostEvent("ResetSoundBeforeLoading", null);
	}

	void IUnitCombatHandler.HandleUnitJoinCombat()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity.IsPlayerEnemy)
		{
			MusicStateHandler.OnEnemyJoinCombat(baseUnitEntity);
		}
	}

	void IUnitCombatHandler.HandleUnitLeaveCombat()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		MusicStateHandler.OnEnemyLeaveCombat(baseUnitEntity);
	}

	void IQuestHandler.HandleQuestStarted(Quest quest)
	{
	}

	void IQuestHandler.HandleQuestCompleted(Quest objective)
	{
		AkUnitySoundEngine.PostTrigger("QuestCompleted", null);
	}

	void IQuestHandler.HandleQuestFailed(Quest objective)
	{
		AkUnitySoundEngine.PostTrigger("QuestFailed", null);
	}

	void IQuestHandler.HandleQuestUpdated(Quest objective)
	{
	}

	void IUnitLifeStateChanged.HandleUnitLifeStateChanged(UnitLifeState prevLifeState)
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity != null && baseUnitEntity.LifeState.State == UnitLifeState.Dead && baseUnitEntity.Health.LastHandledDamage != null && baseUnitEntity.Health.LastHandledDamage.Initiator.IsPlayerFaction && !baseUnitEntity.Blueprint.VisualSettings.NoFinishingBlow && !string.IsNullOrEmpty(ConfigRoot.Instance.Sound.FinishingBlow))
		{
			SoundEventsManager.PostEvent(ConfigRoot.Instance.Sound.FinishingBlow, baseUnitEntity.View.gameObject);
		}
	}

	void IAwarenessHandler.OnEntityNoticed(BaseUnitEntity character)
	{
		MapObjectEntity entity = EventInvokerExtensions.GetEntity<MapObjectEntity>();
		SoundEventsManager.PostEvent("DiscoveryNotification", entity.View.gameObject);
	}

	private void OnApplicationFocusChanged(bool isFocused)
	{
		if (!ApplicationFocusEvents.SoundDisabled && !ApplicationFocusEvents.EventBusDisabled)
		{
			if (SettingsRoot.Sound.MuteAudioWhileTheGameIsOutFocus.GetValue())
			{
				SoundEventsManager.PostEvent(isFocused ? "ResumeAudio" : "PauseAudio", null);
			}
			if (isFocused && CutsceneController.Skipping)
			{
				SoundEvents.SetStoppingAllState(active: true);
			}
		}
	}

	public void OnDetectiveJournalChange(MusicStateHandler.DetectiveBoardMusicState state)
	{
		MusicStateHandler.OnDetectiveJournalChange(state);
	}

	public void OnMusicStateChange(MusicStateHandler.MusicState state)
	{
		MusicStateHandler.OnMusicStateChange(state);
	}

	public void OnMusicChargenStateChange(MusicStateHandler.MusicChargenState state)
	{
		MusicStateHandler.SetMusicChargenState(state);
	}

	public void OnMusicChargenPCVoiceChange(MusicStateHandler.MusicChargenPCVoice voice)
	{
		MusicStateHandler.SetMusicChargenPCVoice(voice);
	}

	void IAreaLoadingStagesHandler.OnAreaScenesLoaded()
	{
		ScheduleNewAreaMusic();
		OnTimeOfDayChanged();
	}

	public void ScheduleNewAreaMusic()
	{
		m_ScheduledMusicArea = Game.Instance.CurrentlyLoadedArea;
		PFLog.System.Log($"Scheduled music for area: {m_ScheduledMusicArea}");
		if (m_ScheduledMusicArea == null)
		{
			m_ScheduledMainMenuMusic = true;
		}
		m_ChangeMusicTime = Time.unscaledTime + 2f;
	}

	void IAreaLoadingStagesHandler.OnAreaLoadingComplete()
	{
		MusicStateHandler.UpdateCombatMusicState();
	}

	void IAreaHandler.OnAreaBeginUnloading()
	{
		MusicStateHandler.BeginAreaTransition();
	}

	void IAreaHandler.OnAreaDidLoad()
	{
		MusicStateHandler.EndAreaTransition();
	}

	void IAreaPartHandler.OnAreaPartChanged(BlueprintAreaPart previous)
	{
		if (!MusicChangeScheduled)
		{
			MusicStateHandler.HandleUpdateArea();
		}
	}

	public void UpdateScheduledAreaMusic()
	{
		if (m_ScheduledMusicArea != null || m_ScheduledMainMenuMusic)
		{
			if (m_ScheduledMusicArea != Game.Instance.CurrentlyLoadedArea)
			{
				m_ScheduledMusicArea = null;
				m_ScheduledMainMenuMusic = false;
				m_ChangeMusicTime = 0f;
			}
			else if (m_ChangeMusicTime <= Time.unscaledTime)
			{
				PFLog.System.Log($"Starting music for area: {m_ScheduledMusicArea}");
				MusicStateHandler.SetMusicSettingState(MusicStateHandler.MusicSettingState.Exploration);
				MusicStateHandler.HandleUpdateArea();
				m_ScheduledMusicArea = null;
				m_ScheduledMainMenuMusic = false;
				m_ChangeMusicTime = 0f;
			}
		}
	}

	public void OnLoadingScreenShown()
	{
	}

	void ITimeOfDayChangedHandler.OnTimeOfDayChanged()
	{
		OnTimeOfDayChanged();
	}

	private static void OnTimeOfDayChanged()
	{
		AkUnitySoundEngine.SetState("TimeOfDay", Game.Instance.TimeOfDay.ToString());
	}

	public static GameObject Get2DSoundObject()
	{
		GameObject gameObject = null;
		CameraRig instance = CameraRig.Instance;
		if (instance != null)
		{
			gameObject = instance.gameObject;
		}
		if (gameObject == null)
		{
			GameObject soundGameObject = Game.Instance.RootUIContext.SoundGameObject;
			if (soundGameObject != null)
			{
				gameObject = soundGameObject.gameObject;
			}
		}
		if (!(gameObject == null))
		{
			return gameObject;
		}
		return null;
	}

	public void StartCutsceneSkip()
	{
		SoundEvents.SetStoppingAllState(active: true);
	}

	public void StopCutsceneSkip()
	{
		SoundEvents.SetStoppingAllState(active: false);
	}

	public void StartDialog()
	{
		m_DialogState = DialogStates.Play;
	}

	public void StopDialog()
	{
		m_DialogState = DialogStates.Stop;
		SoundEventsManager.PostEvent("StopDialog", null);
	}

	void IPartyCombatHandler.HandlePartyCombatStateChanged(bool inCombat)
	{
		if (inCombat)
		{
			CombatSounds.Instance.Combat.CombatStart.Play();
			m_CombatTriggered = true;
			MusicStateHandler.HandlePartyCombatStateChange(isCombatStarted: true);
		}
		else if (m_CombatTriggered)
		{
			CombatSounds.Instance.Combat.CombatEnd.Play();
			m_CombatTriggered = false;
			MusicStateHandler.HandlePartyCombatStateChange(isCombatStarted: false);
		}
	}

	void IFullScreenUIHandler.HandleFullScreenUiChanged(bool state, FullScreenUIType fullScreenUIType)
	{
		m_UIType = (state ? fullScreenUIType : FullScreenUIType.Unknown);
		SetState();
	}

	void IModalWindowUIHandler.HandleModalWindowUiChanged(bool state, ModalWindowUIType modalWindowUIType)
	{
		m_ModalWindowUIType = (state ? modalWindowUIType : ModalWindowUIType.Unknown);
		SetState();
	}

	void IPowerBalanceHandler.HandlePowerBalanceValueUpdate(MoraleGroup combatGroup)
	{
	}

	void IPowerBalanceHandler.HandlePowerBalanceStateUpdate(MoraleGroup combatGroup, PowerBalanceState state)
	{
	}

	void IPowerBalanceHandler.HandlePowerBalanceRecalculated()
	{
		MusicStateHandler.UpdateCombatMusicState();
	}

	void ITurnBasedModeHandler.HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		MusicStateHandler.UpdateCombatMusicState();
	}

	void ITurnBasedModeResumeHandler.HandleTurnBasedModeResumed()
	{
		MusicStateHandler.UpdateCombatMusicState();
	}

	void IPreparationTurnBeginHandler.HandleBeginPreparationTurn(bool canDeploy)
	{
		MusicStateHandler.UpdateCombatMusicState();
	}

	void IPreparationTurnEndHandler.HandleEndPreparationTurn()
	{
		MusicStateHandler.UpdateCombatMusicState();
	}

	void IMoraleVictoryConfirmationRequest.HandleMoraleVictoryConfirmationRequest(IMoraleVictoryConfirmationRequest.Callback callback)
	{
		MusicStateHandler.UpdateCombatMusicState();
	}

	void IMoraleVictoryConfirmationHandler.HandleMoraleVictoryConfirmation(bool confirmed)
	{
		MusicStateHandler.UpdateCombatMusicState();
	}
}
