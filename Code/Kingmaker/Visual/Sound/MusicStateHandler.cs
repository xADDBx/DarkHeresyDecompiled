using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.DLC;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Gameplay.Features.Encounter;
using Kingmaker.Sound;
using Kingmaker.Sound.Base;
using Kingmaker.Stores;
using Kingmaker.UnitLogic.Visual.Blueprints;
using Kingmaker.Utility.FlagCountable;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Visual.Sound;

public class MusicStateHandler
{
	public enum MusicSettingState
	{
		Combat,
		Exploration,
		Death
	}

	public enum MusicState
	{
		Chargen,
		MainMenu,
		Credits,
		BossFight,
		Story,
		Setting,
		CoopLobby,
		DetectiveBoard
	}

	public enum DetectiveBoardMusicState
	{
		None,
		Default,
		Report
	}

	public static readonly string MainMusicEventStart = "Music_Play";

	public static readonly string MainMusicEventStop = "Music_Stop";

	private readonly GameObject m_MusicPlayerObject;

	private UnitVisualSettings.MusicCombatState m_CombatState;

	private List<string> m_OverridedStates = new List<string>();

	private bool m_StoryModeActive;

	private bool m_ProlongTillNextCombat;

	private bool m_EventStarted;

	private bool m_IsStoryMusicState;

	private bool m_OverrideSettingState;

	private AkStateReference m_OverrideMusicSetting;

	private CountableFlag m_ActiveBossFight = new CountableFlag();

	private const string m_MusicCombatStateEventName = "MusicCombatState";

	private LogChannel m_Logger = LogChannelFactory.GetOrCreate("Audio");

	public MusicStateHandler()
	{
		m_MusicPlayerObject = new GameObject("[Music Player]", typeof(AudioObject));
		m_MusicPlayerObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
		if (Application.isPlaying)
		{
			Object.DontDestroyOnLoad(m_MusicPlayerObject);
		}
	}

	public void SetDefaultState(bool setDefaultMusicState = true)
	{
		SoundEventsManager.PostEvent(MainMusicEventStart, m_MusicPlayerObject);
		m_EventStarted = true;
		m_ActiveBossFight.ReleaseAll();
		if (setDefaultMusicState)
		{
			SetMusicState(MusicState.MainMenu);
		}
		SetMusicSettingState(MusicSettingState.Exploration);
	}

	public void HandleUpdateArea()
	{
		if (!IsOverrideAvailable("MusicSettingType", isMainMenuState: false))
		{
			return;
		}
		BlueprintArea currentlyLoadedArea = Game.Instance.CurrentlyLoadedArea;
		BlueprintAreaPart blueprintAreaPart = SimpleBlueprintExtendAsObject.Or(Game.Instance.CurrentlyLoadedAreaPart, currentlyLoadedArea);
		if (currentlyLoadedArea != null)
		{
			if (m_OverrideSettingState)
			{
				SetState("MusicSettingType", m_OverrideMusicSetting.Value);
			}
			else if (blueprintAreaPart != null && blueprintAreaPart.MusicSetting != null)
			{
				SetState("MusicSettingType", blueprintAreaPart.MusicSetting.Value);
			}
			else if (currentlyLoadedArea.MusicSetting != null)
			{
				SetState("MusicSettingType", currentlyLoadedArea.MusicSetting.Value);
			}
			SetState("MusicState", (m_ActiveBossFight ? MusicState.BossFight : MusicState.Setting).ToString());
			SetState("MusicSettingState", ((!Game.Instance.Player.IsInCombat) ? MusicSettingState.Exploration : MusicSettingState.Combat).ToString());
			SetState("MusicStoryType", "None");
			m_StoryModeActive = false;
			m_ProlongTillNextCombat = false;
			m_OverridedStates.Clear();
		}
		else
		{
			SetMusicState(MusicState.MainMenu);
			SetMusicSettingState(MusicSettingState.Exploration);
			SetState("MusicStoryType", "None");
		}
	}

	public void SetMusicCombatState(UnitVisualSettings.MusicCombatState state)
	{
		if (IsOverrideAvailable("MusicCombatState", isMainMenuState: false) && m_CombatState != state)
		{
			m_Logger.Log($"Combat music was changed {m_CombatState} => {state}");
			m_CombatState = state;
			SetState("MusicCombatState", m_CombatState.ToString());
		}
	}

	public void OnEnemyJoinCombat(MechanicEntity unit)
	{
		if (unit is UnitEntity { MusicBossFightType: not null, MusicBossFightType: not null } unitEntity)
		{
			m_ActiveBossFight.Retain();
			if (IsOverrideAvailable(unitEntity.MusicBossFightType.Group, isMainMenuState: false))
			{
				SetMusicState(MusicState.BossFight);
				SetState(unitEntity.MusicBossFightType.Group, unitEntity.MusicBossFightType.Value);
			}
		}
	}

	public void OnEnemyLeaveCombat(MechanicEntity unit)
	{
		if (unit is UnitEntity { MusicBossFightType: not null, MusicBossFightType: not null, IsDead: not false })
		{
			m_ActiveBossFight.Release();
		}
	}

	public void HandlePartyCombatStateChange(bool isCombatStarted)
	{
		if (m_ProlongTillNextCombat && m_OverridedStates != null)
		{
			m_ProlongTillNextCombat = false;
			m_OverridedStates.Clear();
			HandleUpdateArea();
		}
		if (!isCombatStarted)
		{
			m_ActiveBossFight.ReleaseAll();
			SetState("MusicState", MusicState.Setting.ToString());
		}
	}

	public void OnChargenChange(bool chargen)
	{
		SetMusicState((!chargen) ? MusicState.MainMenu : MusicState.Chargen);
	}

	public void OnMusicStateChange(MusicState state)
	{
		if (state != MusicState.DetectiveBoard || (!Game.Instance.Player.IsInCombat && !m_IsStoryMusicState))
		{
			SetMusicState(state);
		}
	}

	public void OnDetectiveJournalChange(DetectiveBoardMusicState state)
	{
		SetState("MusicDetectiveBoard", state.ToString());
	}

	public void SetMusicSettingState(MusicSettingState state)
	{
		if (IsOverrideAvailable("MusicSettingState", isMainMenuState: false))
		{
			SetState("MusicSettingState", state.ToString());
		}
	}

	public void SetMusicState(MusicState state)
	{
		if (!m_EventStarted)
		{
			SetDefaultState();
		}
		string text = string.Empty;
		if (state == MusicState.MainMenu)
		{
			BlueprintDlc blueprintDlc = StoreManager.GetPurchasableDLCs().OfType<BlueprintDlc>().LastOrDefault();
			text = ((!string.IsNullOrWhiteSpace(blueprintDlc?.MusicSetting?.Value)) ? blueprintDlc.MusicSetting.Value : string.Empty);
		}
		if (IsOverrideAvailable("MusicState", state == MusicState.MainMenu || state == MusicState.Credits))
		{
			SetState("MusicState", (!string.IsNullOrWhiteSpace(text)) ? text : state.ToString());
		}
	}

	private void SetState(string group, string state)
	{
		AkUnitySoundEngine.SetState(group, state);
		if (group == "MusicState")
		{
			m_IsStoryMusicState = state == MusicState.Story.ToString();
		}
	}

	public void SetMusicStoryType(AkStateReference state, bool prolongTillNextCombat)
	{
		SetState(state.Group, state.Value);
		if (state.Group == "MusicState" && state.Value != "Story")
		{
			SetState("MusicStoryType", "None");
		}
		m_StoryModeActive = true;
		m_ProlongTillNextCombat = prolongTillNextCombat;
		if (m_OverridedStates == null)
		{
			m_OverridedStates = new List<string>();
		}
		if (state.Group == "MusicCombatState")
		{
			m_CombatState = state.Value switch
			{
				"Losing" => UnitVisualSettings.MusicCombatState.Losing, 
				"Regular" => UnitVisualSettings.MusicCombatState.Regular, 
				"Winning" => UnitVisualSettings.MusicCombatState.Winning, 
				"Won" => UnitVisualSettings.MusicCombatState.Won, 
				"Deployment" => UnitVisualSettings.MusicCombatState.Deployment, 
				_ => m_CombatState, 
			};
		}
		m_OverridedStates.Add(state.Group);
	}

	public void ResetMusicStateOverride(AkStateReference state)
	{
		if (m_OverridedStates.Contains(state.Group))
		{
			SetState(state.Group, "None");
			m_OverridedStates.Remove(state.Group);
			if (state.Group == "MusicCombatState")
			{
				UpdateCombatMusicState();
			}
		}
	}

	private bool IsOverrideAvailable(string stateGroup, bool isMainMenuState)
	{
		if (isMainMenuState)
		{
			return true;
		}
		if (m_StoryModeActive || m_ProlongTillNextCombat)
		{
			return !m_OverridedStates.Contains(stateGroup);
		}
		return true;
	}

	public void ResetStoryMode()
	{
		m_StoryModeActive = false;
		if (!m_ProlongTillNextCombat && m_OverridedStates != null)
		{
			m_OverridedStates.Clear();
			SetMusicState(MusicState.Setting);
			HandleUpdateArea();
		}
	}

	public void StartMusicStopEvent()
	{
		SoundEventsManager.PostEvent(MainMusicEventStop, m_MusicPlayerObject);
		m_EventStarted = false;
	}

	public void OverrideAreaSetting(AkStateReference overrideMusicSetting)
	{
		if (overrideMusicSetting != null)
		{
			m_OverrideMusicSetting = overrideMusicSetting;
			m_OverrideSettingState = true;
			SetState("MusicSettingType", overrideMusicSetting.Value);
		}
	}

	public void DisableOverrideAreaSetting()
	{
		m_OverrideMusicSetting = null;
		m_OverrideSettingState = false;
		BlueprintArea currentlyLoadedArea = Game.Instance.CurrentlyLoadedArea;
		BlueprintAreaPart blueprintAreaPart = SimpleBlueprintExtendAsObject.Or(Game.Instance.CurrentlyLoadedAreaPart, currentlyLoadedArea);
		if (currentlyLoadedArea != null)
		{
			if (blueprintAreaPart != null && blueprintAreaPart.MusicSetting != null)
			{
				SetState("MusicSettingType", blueprintAreaPart.MusicSetting.Value);
			}
			else if (currentlyLoadedArea.MusicSetting != null)
			{
				SetState("MusicSettingType", currentlyLoadedArea.MusicSetting.Value);
			}
		}
	}

	public void UpdateCombatMusicState()
	{
		UnitVisualSettings.MusicCombatState musicCombatState = (TurnController.IsInTurnBasedCombat() ? GetCombatMusicState() : UnitVisualSettings.MusicCombatState.None);
		SetMusicCombatState(musicCombatState);
	}

	public UnitVisualSettings.MusicCombatState GetCombatMusicState()
	{
		if (Game.Instance.Controllers.TurnController.IsPreparationTurn)
		{
			return UnitVisualSettings.MusicCombatState.Deployment;
		}
		ActiveEncounter current = ActiveEncounter.Current;
		if (current != null && current.IsWaitingForMoraleVictoryConfirmation)
		{
			return UnitVisualSettings.MusicCombatState.Won;
		}
		return Game.Instance.Controllers.MoraleController.GetMusicCombatStateByPowerBalance();
	}
}
