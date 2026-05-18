using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Code.GameCore.Editor.Blueprints.BlueprintUnitEditorChecker;
using Code.GameCore.Mics;
using Core.Cheats;
using JetBrains.Annotations;
using Kingmaker.AreaLogic;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.AreaLogic.SummonPool;
using Kingmaker.AreaLogic.TimeOfDay;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Root;
using Kingmaker.Cheats;
using Kingmaker.Code.Framework.GameLog;
using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.Code.Gameplay.Entities;
using Kingmaker.Code.Gameplay.Features.Items.Utility;
using Kingmaker.Code.Middleware.Metrics;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.Facades;
using Kingmaker.Code.View.Bridge.Interfaces;
using Kingmaker.Code.View.Bridge.Services;
using Kingmaker.Code.View.Bridge.Utils;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Controllers.Units;
using Kingmaker.DialogSystem.State;
using Kingmaker.DLC;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Persistence.Scenes;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.Framework.GlobalEffectSystem;
using Kingmaker.GameCommands;
using Kingmaker.GameInfo;
using Kingmaker.GameModes;
using Kingmaker.Gameplay.Entities;
using Kingmaker.Gameplay.Features.Reputation;
using Kingmaker.Gameplay.Utility;
using Kingmaker.IngameConsole;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.Localization;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Networking;
using Kingmaker.Networking.Settings;
using Kingmaker.Networking.Tests;
using Kingmaker.Plugins.CoopDesyncAnalyzer.Attributes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.ResourceLinks;
using Kingmaker.Settings;
using Kingmaker.Stores.DlcInterfaces;
using Kingmaker.TextTools;
using Kingmaker.TextTools.Core;
using Kingmaker.Tutorial;
using Kingmaker.UI;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Pointer;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.Visual;
using Kingmaker.Visual.CharacterSystem;
using Kingmaker.Visual.FX;
using Kingmaker.Visual.Particles;
using Kingmaker.Visual.Particles.GameObjectsPooling;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.Locator;
using Owlcat.Runtime.Visual.DxtCompressor;
using Owlcat.Runtime.Visual.Effects.ParticleSumEmitter;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Kingmaker;

public class Game : IGameDoStartMode, IGameDoStopMode, IGameDoSwitchCutsceneLock
{
	public enum ControllerModeType
	{
		Mouse,
		Gamepad
	}

	private readonly struct StopModeInvoker : GameCommandQueue.IInvokable
	{
		private readonly Game m_Game;

		private readonly GameModeType m_Type;

		public StopModeInvoker(Game game, GameModeType type)
		{
			m_Game = game;
			m_Type = type;
		}

		public void Invoke()
		{
			if (m_Game.m_GameModeTicking || (bool)ContextData<SwitchingModes>.Current || 0 < m_Game.GameCommandQueue.Count)
			{
				m_Game.GameCommandQueue.AddCommand(new StopGameModeCommand(m_Type));
			}
			else
			{
				m_Game.DoStopMode(m_Type);
			}
		}
	}

	private class GameModeSwitchOnLoadHandler : IGameModeHandler, ISubscriber
	{
		private readonly Game m_Game;

		public GameModeSwitchOnLoadHandler(Game game)
		{
			m_Game = game;
		}

		public void OnGameModeStart(GameModeType gameMode)
		{
			m_Game.OnAreaLoadGameModeSet();
		}

		public void OnGameModeStop(GameModeType gameMode)
		{
		}
	}

	private readonly struct StartModeInvoker : GameCommandQueue.IInvokable
	{
		private readonly Game m_Game;

		private readonly GameModeType m_Type;

		public StartModeInvoker(Game game, GameModeType type)
		{
			m_Game = game;
			m_Type = type;
		}

		public void Invoke()
		{
			if (m_Game.m_GameModeTicking || (bool)ContextData<SwitchingModes>.Current || 0 < m_Game.GameCommandQueue.Count)
			{
				m_Game.GameCommandQueue.AddCommand(new StartGameModeCommand(m_Type));
			}
			else
			{
				m_Game.DoStartMode(m_Type);
			}
		}
	}

	private class LoadingAreaMarker : IEnumerator
	{
		public readonly BlueprintArea Area;

		public object Current => null;

		public LoadingAreaMarker(BlueprintArea area)
		{
			Area = area;
		}

		public bool MoveNext()
		{
			return false;
		}

		public void Reset()
		{
		}
	}

	public class SwitchingModes : ContextFlag<SwitchingModes>
	{
	}

	public readonly ControllersAccess Controllers = new ControllersAccess();

	public readonly EntityPools EntityPools = new EntityPools();

	private ControllerModeType m_ControllerMode;

	private bool _fromNewGameProcess;

	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("Game");

	private static Game s_Instance;

	[CanBeNull]
	private static SaveInfo s_ImportSave;

	[CanBeNull]
	private static BaseUnitEntity s_NewGameUnit;

	private CompositeDisposable m_BugReportDisposable;

	private EntityRef<UnitEntity> m_DefaultUnit;

	private Runner m_Runner;

	private bool m_GameModeTicking;

	private bool m_WillBePaused;

	private bool m_AreaWasSwitched;

	private bool m_MatchTimeOfDayScheduled;

	private bool m_AlreadyInitialized;

	private bool m_WasLoadingForTheAssert;

	private bool m_PauseRequestLastValue;

	private float m_LoadingProgress;

	private float m_LoadingScenesProgress;

	private readonly Stack<GameMode> m_GameModes = new Stack<GameMode>();

	private int[] m_ModesCount = new int[0];

	private GameModeSwitchOnLoadHandler m_GameModeSwitchOnLoadHandler;

	private readonly QuickSlotsReplenishLogic m_QuickSlotReplenish = new QuickSlotsReplenishLogic();

	public readonly GameCommandQueue GameCommandQueue = new GameCommandQueue();

	public readonly CursorController CursorController = new CursorController();

	public readonly UISettingsManager UISettingsManager = new UISettingsManager();

	public readonly GpuCrowdController GpuCrowdController = new GpuCrowdController();

	public readonly StaticInfoCollector StaticInfoCollector = new StaticInfoCollector();

	public readonly RealTimeController RealTimeController = new RealTimeController();

	public readonly SaveManager SaveManager = new SaveManager();

	public readonly UnitGroupsHolder UnitGroups = new UnitGroupsHolder();

	public readonly GridNodeToEntityCache GridNodeToEntityCache = new GridNodeToEntityCache();

	public readonly PersistentState State;

	public readonly TradeLogic TradeLogic = new TradeLogic();

	[CanBeNull]
	private static BlueprintAreaPreset s_NewGamePreset;

	private ServiceProxy<SummonPoolsManager> m_SummonPoolsProxy;

	public static bool DontChangeController { get; set; }

	public bool IsControllerMouse => ControllerMode == ControllerModeType.Mouse;

	public bool IsControllerGamepad => ControllerMode == ControllerModeType.Gamepad;

	public ControllerModeType ControllerMode
	{
		get
		{
			return m_ControllerMode;
		}
		set
		{
			if (m_ControllerMode == value)
			{
				return;
			}
			m_ControllerMode = ControllerModeType.Mouse;
			m_BugReportDisposable?.Dispose();
			m_BugReportDisposable = null;
			Input.simulateMouseWithTouches = m_ControllerMode != ControllerModeType.Gamepad;
			if (m_ControllerMode == ControllerModeType.Gamepad)
			{
				m_BugReportDisposable = BugReportControls.AddBugReportControls();
			}
			if (BuildModeUtility.IsDevelopment)
			{
				return;
			}
			try
			{
				Cursor.visible = m_ControllerMode == ControllerModeType.Mouse;
			}
			catch
			{
			}
		}
	}

	public static ControllerModeType? ControllerOverride
	{
		get
		{
			string text = BuildModeUtility.Data?.ForceControllerMode;
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}
			if (text.ToLower() == "gamepad")
			{
				return ControllerModeType.Gamepad;
			}
			if (text.ToLower() == "mouse")
			{
				return ControllerModeType.Mouse;
			}
			PFLog.Default.Error("Strange value in ForceControllerMode in startup.json - defaulting to mouse");
			return ControllerModeType.Mouse;
		}
	}

	public GameModeType CurrentModeType
	{
		get
		{
			if (m_GameModes.Count > 0)
			{
				return m_GameModes.Peek().Type;
			}
			return GameModeType.None;
		}
	}

	[CanBeNull]
	public GameMode CurrentGameMode
	{
		get
		{
			if (m_GameModes.Count > 0)
			{
				return m_GameModes.Peek();
			}
			return null;
		}
	}

	public bool IsSpaceCombat => CurrentModeType == GameModeType.SpaceCombat;

	private int[] ModesCount
	{
		get
		{
			if (m_ModesCount.Length < GameModeType.Count)
			{
				int[] array = new int[GameModeType.Count];
				Array.Copy(m_ModesCount, array, m_ModesCount.Length);
				m_ModesCount = array;
			}
			return m_ModesCount;
		}
	}

	public bool IsPaused
	{
		get
		{
			return IsModeActive(GameModeType.Pause);
		}
		set
		{
			if (value != IsPaused && !LoadingProcess.Instance.IsAnyLoadingScreenActive)
			{
				GameCommandQueue.SetPauseManualState(value);
			}
		}
	}

	public static bool IsTestRun { get; set; }

	[NotNull]
	public Player Player => Player.Ref.Entity;

	[NotNull]
	public DetectiveSystem DetectiveSystem => DetectiveSystem.Ref;

	[NotNull]
	public QuestBook QuestBook => QuestBook.Ref;

	[NotNull]
	public EtudesSystem EtudesSystem => EtudesSystem.Ref;

	[NotNull]
	public TutorialSystem TutorialSystem => TutorialSystem.Ref;

	[NotNull]
	public PartySharedInventory PartySharedInventory => PartySharedInventory.Ref;

	[NotNull]
	public DialogState DialogState => DialogState.Ref;

	[NotNull]
	public ReputationState Reputation => ReputationState.Ref;

	[NotNull]
	public AsksState AsksState => AsksState.Ref;

	[NotNull]
	public VendorsManager VendorsManager => VendorsManager.Ref;

	public GameStatistic Statistic => State.Statistic;

	public SceneLoader SceneLoader => SceneLoader.Instance;

	public bool AlreadyInitialized => m_AlreadyInitialized;

	public KeyboardAccess Keyboard => KeyboardAccess.Instance;

	public static bool IsInMainMenu => !Runner.IsActive;

	[CanBeNull]
	public static BlueprintAreaPreset NewGamePreset
	{
		get
		{
			return s_NewGamePreset;
		}
		set
		{
			PFLog.System.Log($"NewGamePreset setter | old={s_NewGamePreset} | new={value}");
			s_NewGamePreset = value;
			ImportSave = null;
		}
	}

	[CanBeNull]
	public static SaveInfo ImportSave
	{
		get
		{
			return s_ImportSave;
		}
		set
		{
			PFLog.System.Log($"ImportSave setter | old={s_ImportSave} | new={value}");
			s_ImportSave = value;
		}
	}

	[CanBeNull]
	public static BaseUnitEntity NewGameUnit
	{
		get
		{
			return s_NewGameUnit;
		}
		set
		{
			PFLog.System.Log($"NewGameUnit setter | old={s_NewGameUnit} | new={value}");
			s_NewGameUnit = value;
		}
	}

	public static Game Instance
	{
		get
		{
			if (s_Instance == null && (Application.isPlaying || ContextData<BlueprintUnitCheckerInEditorContextData>.Current != null))
			{
				EnsureGameLifetimeServices();
				s_Instance = new Game();
			}
			return s_Instance;
		}
	}

	public static bool HasInstance => s_Instance != null;

	public string SaveId { get; private set; }

	public AreaPersistentState LoadedAreaState => State.LoadedAreaState;

	public Area LoadedArea => State?.LoadedAreaState?.Area;

	public TimeOfDay TimeOfDay { get; private set; }

	public BlueprintArea CurrentlyLoadedArea => SceneLoader.CurrentlyLoadedArea;

	public BlueprintAreaPart CurrentlyLoadedAreaPart => SceneLoader.CurrentlyLoadedAreaPart;

	public RootGroup DynamicRoot => SceneLoader.DynamicRoot;

	public RootGroup CrossSceneRoot => SceneLoader.CrossSceneRoot;

	public CoopData CoopData => State.CoopData;

	public IRootUIContext RootUIContext { get; }

	public IBugReportContext BugReportContext => RootUIContext;

	public bool IsUnloading { get; set; }

	public SummonPoolsManager SummonPools
	{
		get
		{
			m_SummonPoolsProxy = ((m_SummonPoolsProxy?.Instance != null) ? m_SummonPoolsProxy : Services.GetProxy<SummonPoolsManager>());
			if (m_SummonPoolsProxy?.Instance == null)
			{
				Services.RegisterServiceInstance(new SummonPoolsManager());
				m_SummonPoolsProxy = Services.GetProxy<SummonPoolsManager>();
			}
			return m_SummonPoolsProxy?.Instance;
		}
	}

	public UnitEntity DefaultUnit
	{
		[SkipAnalysis]
		get
		{
			if (m_DefaultUnit.Entity == null)
			{
				m_DefaultUnit = Entity.Initialize(new UnitEntity("<default-unit>", isInGame: false, ConfigRoot.Instance.SystemMechanics.DefaultUnit));
			}
			return m_DefaultUnit;
		}
	}

	public bool InvertPauseButtonPressed { get; private set; }

	public bool PauseOnLoadPending { get; set; }

	public float UILoadingProgress => m_LoadingProgress * 0.2f + m_LoadingScenesProgress * 0.8f;

	public bool IsLoadingProgressPaused { get; private set; }

	public static float CombatAnimSpeedUp
	{
		get
		{
			if (!Instance.Player.IsInCombat)
			{
				return 1f;
			}
			if (!Instance.Controllers.TurnController.IsPlayerTurn)
			{
				return Instance.Player.UISettings.AnimSpeedUpNPC;
			}
			return Instance.Player.UISettings.AnimSpeedUpPlayer;
		}
	}

	[CanBeNull]
	public T GetController<T>() where T : class, IController
	{
		return GetController<T>(includeInactive: true);
	}

	public Camera GetCamera()
	{
		if (!Application.isPlaying)
		{
			return Camera.main;
		}
		CameraRig instance = CameraRig.Instance;
		if ((bool)instance && instance.Camera.enabled && instance.gameObject.activeInHierarchy)
		{
			return instance.Camera;
		}
		return CameraStackManager.Instance.GetMain();
	}

	private void InitializeControllerMode()
	{
		if (!DontChangeController)
		{
			ControllerMode = (UtilsConsole.HasAnyGamepad ? ControllerModeType.Gamepad : ControllerModeType.Mouse);
		}
		if (ControllerOverride.HasValue)
		{
			ControllerMode = ControllerOverride.Value;
		}
	}

	public static void DoAutoDetectControllerMode()
	{
		if (Application.isConsolePlatform && !Application.isEditor)
		{
			if (EventSystem.current != null)
			{
				UnityEngine.Object.Destroy(EventSystem.current.gameObject);
				EventSystem.current = null;
			}
			Instance.ControllerMode = ((!ApplicationHelper.IsRunningOnSwitch2 || !SettingsRoot.Game.Switch.SwitchJoyConAsMouse) ? ControllerModeType.Gamepad : ControllerModeType.Mouse);
		}
		else if (ControllerOverride.HasValue)
		{
			Instance.ControllerMode = ControllerOverride.Value;
		}
		else if (ApplicationHelper.IsRunOnSteamDeck)
		{
			Instance.ControllerMode = ControllerModeType.Gamepad;
		}
		else if (UtilsConsole.HasAnyGamepad)
		{
			Instance.ControllerMode = ControllerModeType.Gamepad;
		}
		else
		{
			Instance.ControllerMode = ControllerModeType.Mouse;
			DontChangeController = true;
		}
	}

	[CanBeNull]
	private T GetController<T>(bool includeInactive) where T : class, IController
	{
		if (m_GameModes.Count == 0 && !includeInactive)
		{
			return null;
		}
		GameMode result;
		T val = (m_GameModes.TryPeek(out result) ? result.GetControllerOptional<T>() : null);
		if (val != null || !includeInactive)
		{
			return val;
		}
		return GamesModeFactoryFacade.Instance.GetController<T>();
	}

	private void StopAllModes()
	{
		int num = 0;
		while (!m_GameModes.Empty())
		{
			DoStopMode(CurrentModeType);
			if (num++ > 100)
			{
				PFLog.System.Error("Could not stop all game modes properly");
				m_GameModes.Clear();
				break;
			}
		}
	}

	private void DoStopMode(GameModeType type)
	{
		if (type != CurrentModeType)
		{
			PFLog.System.Warning("Cannot stop game mode {0}, mode {1} is active", type, CurrentModeType);
			return;
		}
		using (ContextData<SwitchingModes>.Request())
		{
			using (ProfileScope.New($"Stop Mode ({type})"))
			{
				GameMode gameMode = m_GameModes.Pop();
				GameMode gameMode2 = (m_GameModes.Empty() ? null : m_GameModes.Peek());
				ModesCount[(int)type]--;
				gameMode.OnDisable();
				gameMode.OnStop(gameMode2);
				gameMode2?.OnEnable();
				PFLog.System.Log("Stopped mode {0} (next mode on stack {1})", gameMode.Type, CurrentModeType);
				HandleGameModeChanged(gameMode.Type, gameMode2?.Type ?? GameModeType.None);
			}
		}
	}

	private void DoStartMode(GameModeType type)
	{
		using (ContextData<SwitchingModes>.Request())
		{
			if (type == GameModeType.Pause && m_WillBePaused)
			{
				m_WillBePaused = false;
			}
			if (type == GameModeType.Pause)
			{
				GameModeType currentModeType = CurrentModeType;
				if (currentModeType != GameModeType.Default && currentModeType != GameModeType.Cutscene && currentModeType != GameModeType.GlobalMap)
				{
					PFLog.System.Log("Preventing starting {0} over {1}", type, currentModeType);
					return;
				}
			}
			if (type == GameModeType.Default || type == GameModeType.Dialog || type == GameModeType.Cutscene)
			{
				while (CurrentModeType == GameModeType.Pause)
				{
					PFLog.System.Log("Stopping Pause before starting {0}", type);
					DoStopMode(GameModeType.Pause);
				}
			}
			using (ProfileScope.New($"Start Mode ({type})"))
			{
				GameMode gameMode = (m_GameModes.Empty() ? null : m_GameModes.Peek());
				GameMode gameMode2 = GamesModeFactoryFacade.Instance.Create(type);
				ModesCount[(int)type]++;
				gameMode?.OnDisable();
				m_GameModes.Push(gameMode2);
				gameMode2.OnStart(gameMode);
				gameMode2.OnEnable();
				PFLog.System.Log("Started mode {0} (previous mode {1})", CurrentModeType, gameMode?.Type);
				HandleGameModeChanged(gameMode?.Type ?? GameModeType.None, gameMode2.Type);
			}
		}
	}

	private void HandleGameModeChanged(GameModeType oldMode, GameModeType newMode)
	{
		EventBus.RaiseEvent(delegate(IGameModeHandler h)
		{
			h.OnGameModeStop(oldMode);
		});
		if (oldMode == GameModeType.Pause || newMode == GameModeType.Pause)
		{
			EventBus.RaiseEvent(delegate(IPauseHandler h)
			{
				h.OnPauseToggled();
			});
		}
		EventBus.RaiseEvent(delegate(IGameModeHandler h)
		{
			h.OnGameModeStart(newMode);
		});
	}

	public void StartMode(GameModeType type)
	{
		if (type == CurrentModeType)
		{
			PFLog.System.Error("Game mode with type {0} already active", type);
		}
		else if (Player.GameOverReason.HasValue && type != GameModeType.GameOver)
		{
			PFLog.System.Error("Cannot enter game mode {0}: game is over", type);
		}
		else
		{
			PFLog.System.Log("Start mode request {0}", type);
			GameCommandQueue.LockAndRun(new StartModeInvoker(this, type));
		}
	}

	public void StopMode(GameModeType type)
	{
		PFLog.System.Log("Stop mode request {0}", type);
		GameCommandQueue.LockAndRun(new StopModeInvoker(this, type));
	}

	void IGameDoStartMode.DoStartMode(GameModeType type)
	{
		DoStartMode(type);
	}

	void IGameDoStopMode.DoStopMode(GameModeType type)
	{
		DoStopMode(type);
	}

	void IGameDoSwitchCutsceneLock.DoSwitchCutsceneLock(bool @lock)
	{
		DoSwitchCutsceneLock(@lock);
	}

	private void DoSwitchCutsceneLock(bool @lock)
	{
		if (CurrentModeType == GameModeType.None || (IsModeActive(GameModeType.CutsceneGlobalMap) || IsModeActive(GameModeType.Cutscene)) == @lock)
		{
			return;
		}
		GameModeType type = (IsModeActive(GameModeType.GlobalMap) ? GameModeType.CutsceneGlobalMap : GameModeType.Cutscene);
		if (@lock)
		{
			DoStartMode(type);
		}
		else
		{
			DoStopMode(type);
		}
		foreach (BaseUnitEntity partyAndPet in Player.PartyAndPets)
		{
			partyAndPet.Commands.InterruptAll((AbstractUnitCommand c) => !c.FromCutscene);
		}
		NetService.Instance.CancelCurrentCommands();
	}

	public bool IsModeActive(GameModeType gameModeType)
	{
		return ModesCount[(int)gameModeType] > 0;
	}

	public void RequestPauseUi(bool isPaused)
	{
		if (!IsInMainMenu)
		{
			m_PauseRequestLastValue = isPaused;
		}
	}

	private void TryDoPauseRequest()
	{
		if (!IsInMainMenu)
		{
			Controllers.PauseController.RequestPauseUi(m_PauseRequestLastValue);
		}
	}

	public void SetIsPauseForce(bool value)
	{
		if (value != IsPaused && !m_WillBePaused && !(LoadingProcess.Instance.IsAnyLoadingScreenActive && value))
		{
			if (value)
			{
				m_WillBePaused = true;
				Controllers.PointerController.SkipDeactivation = true;
				StartMode(GameModeType.Pause);
			}
			else
			{
				Controllers.PointerController.SkipDeactivation = true;
				StopMode(GameModeType.Pause);
			}
		}
	}

	private void InitializeKeyboardBindings()
	{
		Keyboard.RegisterBuiltinBindings();
		Screenshot.Initialize(Keyboard);
		Keyboard.Bind("QuickSave", delegate
		{
			MakeQuickSave();
		});
		Keyboard.Bind("QuickLoad", QuickLoadGame);
		Keyboard.Bind("Stop", delegate
		{
			SelectionManagerFacade.SelectionManager.Stop();
		});
		Keyboard.Bind("Hold", delegate
		{
			SelectionManagerFacade.SelectionManager.Hold();
		});
		Keyboard.Bind("Pause", Pause);
		Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.SkipBark.name, GameCommandQueue.SkipBark);
		Keyboard.Bind("UnpauseOn", UnpauseBindOn);
		Keyboard.Bind("UnpauseOff", UnpauseBindOff);
	}

	private void Pause()
	{
		if (IsPaused)
		{
			PauseBind();
		}
		else if (!UtilityGame.IsGlobalMap())
		{
			PauseBind();
		}
	}

	private void TryEndTurnBind()
	{
		if (!IsPaused && !UtilityGame.IsGlobalMap())
		{
			EndTurnBind();
		}
	}

	public void UnpauseBindOff()
	{
		InvertPauseButtonPressed = false;
	}

	public void UnpauseBindOn()
	{
		InvertPauseButtonPressed = true;
	}

	public void PauseBind()
	{
		IsPaused = !Instance.Controllers.TurnController.TurnBasedModeActive && !UtilityGame.IsGlobalMap() && !IsPaused;
	}

	[Obsolete]
	public void EndTurnBind()
	{
		TurnController turnController = CurrentGameMode?.GetControllerOptional<TurnController>();
		if (turnController != null && turnController.TurnBasedModeActive && (!turnController.IsSpaceCombat || UtilityNet.IsControlMainCharacter()) && !turnController.IsPreparationTurn && turnController.IsPlayerTurn && turnController.CurrentUnit.IsMyNetRole() && turnController.CurrentUnit.GetCommandsOptional()?.Current == null && !turnController.IsPreciseAttack)
		{
			turnController.TryEndPlayerTurnManually();
		}
	}

	private static void LoadArea([NotNull] BlueprintArea area, [CanBeNull] BlueprintAreaEnterPoint enterPoint, AutoSaveMode autoSaveMode, [CanBeNull] SaveInfo saveInfo = null, [CanBeNull] Action callback = null)
	{
		if (area == null)
		{
			throw new ArgumentException("area is null", "area");
		}
		BlueprintAreaPart areaPart;
		if (saveInfo != null)
		{
			if (saveInfo.Area == null)
			{
				throw new ArgumentException(string.Format("{0} {1} has null {2}", "saveInfo", saveInfo, "Area"), "saveInfo");
			}
			if (enterPoint != null)
			{
				throw new ArgumentException(string.Format("{0} {1} should be null when using {2}", "enterPoint", enterPoint, "saveInfo"), "enterPoint");
			}
			areaPart = saveInfo.AreaPart ?? throw new ArgumentException(string.Format("{0} {1} has null {2}", "saveInfo", saveInfo, "AreaPart"), "saveInfo");
			PFLog.System.Log("Load Area {0} [save name: {1}]", area, saveInfo);
		}
		else
		{
			if (enterPoint == null)
			{
				throw new ArgumentException("enterPoint is null", "enterPoint");
			}
			if (enterPoint.Area != area)
			{
				throw new ArgumentException(string.Format("{0} {1} area does not match {2} {3}", "enterPoint", enterPoint, "area", area), "enterPoint");
			}
			areaPart = enterPoint.AreaPart ?? throw new Exception(string.Format("{0} {1} has null {2}. Most likely this {3} is outside of mechanics bounds of any {4}.", "BlueprintAreaEnterPoint", enterPoint, "AreaPart", "BlueprintAreaEnterPoint", "AreaPart"));
			PFLog.System.Log("Load Area {0} [enter point: {1}]", area, enterPoint);
		}
		if (autoSaveMode != 0 && !enterPoint)
		{
			throw new ArgumentException($"Autosave {autoSaveMode} can be done only when enter point is specified");
		}
		if (LoadingProcess.Instance.FindProcess((IEnumerator v) => v is LoadingAreaMarker) is LoadingAreaMarker loadingAreaMarker)
		{
			throw new InvalidOperationException($"Cant start loading area {area} when loading queue already contains {loadingAreaMarker.Area} to load");
		}
		Instance.ResetLoadingProgress();
		Instance.IsLoadingProgressPaused = true;
		LoadingProcess.Instance.StartLoadingProcess("ConsolePreloadAreaCoroutine", SceneLoader.ConsolePreloadAreaCoroutine(area), delegate
		{
			Instance.IsLoadingProgressPaused = false;
		});
		Instance.RootUIContext.SetLoadingArea(area);
		BlueprintArea currentlyLoadedArea = Instance.SceneLoader.CurrentlyLoadedArea;
		HashSet<string> hotSceneNames = area.GetHotSceneNames();
		Metrics.Location.From(currentlyLoadedArea?.AssetGuid).To(area.AssetGuid).Send();
		if (currentlyLoadedArea != null)
		{
			if (saveInfo == null)
			{
				LoadingProcess.Instance.StartLoadingProcess("WaitTickCommandsEnd", LoadingProcessCommandsLogic.WaitTickCommandsEnd(), delegate
				{
					Instance.m_LoadingProgress = 0.05f;
				});
			}
			if (autoSaveMode == AutoSaveMode.BeforeExit)
			{
				if (saveInfo == null && Instance.CurrentModeType == GameModeType.Dialog && !Instance.Controllers.DialogController.DialogStopScheduled && !Instance.GameCommandQueue.ContainsCommand((StopGameModeCommand x) => x.GameMode == GameModeType.Dialog))
				{
					PFLog.System.Log($"Stop mode {Instance.CurrentModeType} before autosave");
					Instance.StopMode(Instance.CurrentModeType);
				}
				LoadingProcess.Instance.StartLoadingProcess("SaveRoutine (BeforeExit)", Instance.SaveManager.SaveRoutine(Instance.SaveManager.GetNextAutoslot()), delegate
				{
					Instance.m_LoadingProgress = 0.1f;
				});
			}
			if (Instance.CurrentModeType == GameModeType.Pause)
			{
				Instance.StopMode(Instance.CurrentModeType);
			}
			LoadingProcess.Instance.StartLoadingProcess("UnloadAreaCoroutine", Instance.SceneLoader.UnloadAreaCoroutine(saveInfo != null, currentlyLoadedArea == area, hotSceneNames), delegate
			{
				Instance.m_LoadingProgress = 0.2f;
			});
		}
		if (currentlyLoadedArea != null && saveInfo != null)
		{
			LoadingProcess.Instance.StartLoadingProcess("ResetGame", ResetGame(area));
		}
		LoadAreaStage2(area, enterPoint, autoSaveMode, saveInfo, callback, areaPart);
	}

	public static void ReloadAreaMechanic(bool clearFx, bool needNavMeshRescam, [CanBeNull] Action callback = null)
	{
		EventBus.RaiseEvent(delegate(IReloadMechanicsHandler h)
		{
			h.OnBeforeMechanicsReload();
		});
		LoadingProcess.Instance.StartLoadingProcess("ReloadAreaMechanicsCoroutine", SceneLoader.Instance.ReloadAreaMechanicsCoroutine(needNavMeshRescam), delegate
		{
			if (clearFx)
			{
				PFLog.System.Log("FxHelper.DestroyAll();");
				FxHelper.DestroyAllBlood();
			}
			EventBus.RaiseEvent(delegate(IReloadMechanicsHandler h)
			{
				h.OnMechanicsReloaded();
			});
			Instance.Player.ApplyUpgrades();
			ExecuteSafe(callback);
		}, LoadingProcessTag.ReloadMechanics);
		LoadingProcess.Instance.StartLoadingProcess("AwaitingNetwork", AwaitingNetwork(), null, LoadingProcessTag.ReloadMechanics);
	}

	public void LoadArea([NotNull] BlueprintAreaEnterPoint areaEnterPoint, AutoSaveMode autoSaveMode, Action callback = null)
	{
		if (areaEnterPoint == null)
		{
			throw new ArgumentException("areaEnterPoint is null", "areaEnterPoint");
		}
		Instance.Controllers.DialogController.Tick();
		if (CurrentlyLoadedArea == areaEnterPoint.Area)
		{
			Teleport(areaEnterPoint, includeFollowers: true, callback);
		}
		else
		{
			LoadArea(areaEnterPoint.Area, areaEnterPoint, autoSaveMode, null, callback);
		}
	}

	public bool IsAreaLoaded(BlueprintArea area)
	{
		return SceneLoader.IsAreaLoaded(area);
	}

	private static void LoadAreaStage2([NotNull] BlueprintArea area, [CanBeNull] BlueprintAreaEnterPoint enterPoint, AutoSaveMode autoSaveMode, [CanBeNull] SaveInfo saveInfo, [CanBeNull] Action callback, [NotNull] BlueprintAreaPart areaPart)
	{
		if (SceneManager.GetSceneByName(GameScenes.MainMenu).isLoaded)
		{
			LoadingProcess.Instance.StartLoadingProcess("UnloadMainMenuRoutine", UnloadMainMenuRoutine(), delegate
			{
				Instance.m_LoadingProgress = 0.3f;
			});
		}
		LoadingProcess.Instance.StartLoadingProcess("UnloadEntitiesCoroutine", SceneLoader.Instance.UnloadEntitiesCoroutine(saveInfo != null), delegate
		{
			Instance.m_LoadingProgress = 0.5f;
		});
		LoadingProcess.Instance.StartLoadingProcess("UnloadUnusedAssetsCoroutine", UnloadUnusedAssetsCoroutine(), delegate
		{
			Instance.m_LoadingProgress = 0.6f;
		});
		if (autoSaveMode == AutoSaveMode.WhenAreaIsUnloaded)
		{
			Instance.Player.NextEnterPoint = enterPoint;
			StartLoadingProcessDetached("SaveRoutine (WhenAreaIsUnloaded)", delegate
			{
				Instance.Player.NextEnterPoint = enterPoint;
				return Instance.SaveManager.SaveRoutine(Instance.SaveManager.GetNextAutoslot());
			}, delegate
			{
				Instance.m_LoadingProgress = 0.1f;
			});
		}
		if (saveInfo != null)
		{
			StartLoadingProcessDetached("LoadRoutine", () => Instance.SaveManager.LoadRoutine(saveInfo), delegate
			{
				Instance.m_LoadingProgress = 0.4f;
			});
		}
		LoadingProcess.Instance.StartLoadingProcess("UnloadEntitiesCoroutine 2", SceneLoader.Instance.UnloadEntitiesCoroutine(saveInfo != null), delegate
		{
			Instance.m_LoadingProgress = 0.5f;
		});
		LoadingProcess.Instance.StartLoadingProcess("UnloadUnusedAssetsCoroutine 2", UnloadUnusedAssetsCoroutine(), delegate
		{
			Instance.m_LoadingProgress = 0.6f;
		});
		LoadingProcess.Instance.StartLoadingProcess("LoadAreaCoroutine", SceneLoader.Instance.LoadAreaCoroutine(area, areaPart, enterPoint, isSmokeTest: false, new Progress<float>(Instance.OnLoadingScenesProgress)), delegate
		{
			Instance.OnLoadingScenesProgress(1f);
			Instance.OnAreaLoaded();
		});
		StartLoadingProcessDetached("LoadAdditiveAreas", () => Instance.LoadAdditiveAreas(area));
		LoadingProcess.Instance.StartLoadingProcess("PreloadUnitResources", PreloadUnitResources(), delegate
		{
			Instance.m_LoadingProgress = 0.7f;
		});
		LoadingProcess.Instance.StartLoadingProcess("ApplyUpgrades", delegate
		{
			Instance.Player.ApplyUpgrades();
		});
		StartLoadingProcessDetached("MatchTimeOfDayCoroutine", () => Instance.MatchTimeOfDayCoroutine(), delegate
		{
			Instance.m_LoadingProgress = 0.8f;
		});
		if (autoSaveMode == AutoSaveMode.AfterEntry)
		{
			StartLoadingProcessDetached("SaveRoutine (AfterEntry)", () => Instance.SaveManager.SaveRoutine(Instance.SaveManager.GetNextAutoslot()), delegate
			{
				Instance.m_LoadingProgress = 0.9f;
			});
		}
		LoadingProcess.Instance.StartLoadingProcess("AwaitTextureCompression", AwaitTextureCompression(), delegate
		{
			Instance.m_LoadingProgress = 0.95f;
		});
		StartLoadingProcessDetached("AreaLoadingComplete", () => Instance.AreaLoadingComplete(), delegate
		{
			Instance.Controllers.SceneControllables.Rescan();
			if (saveInfo != null)
			{
				EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
				{
					h.HandleWarning(WarningNotificationType.GameLoaded);
				});
			}
			Instance.m_LoadingProgress = 1f;
			Instance.m_LoadingScenesProgress = 1f;
		});
		LoadingProcess.Instance.StartLoadingProcess("AwaitingUserInput", AwaitingUserInput());
		LoadingProcess.Instance.StartLoadingProcess("AwaitingNetwork", AwaitingNetwork());
		LoadingProcess.Instance.StartLoadingProcess("LoadingAreaMarker", new LoadingAreaMarker(area));
		if (callback != null)
		{
			LoadingProcess.Instance.StartLoadingProcess("ExecuteSafe(callback)", delegate
			{
				ExecuteSafe(callback);
			});
		}
		EventBus.RaiseEvent(delegate(IAreaTransitionHandler h)
		{
			h.HandleAreaTransition();
		});
	}

	public void LoadAdditiveArea(BlueprintArea area)
	{
		LoadingProcess.Instance.StartLoadingProcess("LoadAdditiveArea", SceneLoader.LoadAdditiveArea(area), null, LoadingProcessTag.Save);
	}

	public void UnloadAdditiveArea(BlueprintArea area)
	{
		LoadingProcess.Instance.StartLoadingProcess("UnloadAdditiveArea", SceneLoader.UnloadAdditiveArea(area), null, LoadingProcessTag.Save);
	}

	public void ActivateAdditiveArea(BlueprintAreaEnterPoint enterPoint, bool showLoadingScreen, AutoSaveMode autoSaveMode = AutoSaveMode.None)
	{
		LoadingProcessTag processTag = ((!showLoadingScreen) ? LoadingProcessTag.TeleportParty : LoadingProcessTag.None);
		if (CurrentModeType == GameModeType.Dialog && !Instance.Controllers.DialogController.DialogStopScheduled && GameCommandQueue.ContainsCommand((StopGameModeCommand x) => x.GameMode == GameModeType.Dialog))
		{
			GameCommandQueue.Tick();
		}
		if (autoSaveMode == AutoSaveMode.BeforeExit)
		{
			LoadingProcess.Instance.StartLoadingProcess("SaveRoutine (BeforeExit)", SaveManager.SaveRoutine(SaveManager.GetNextAutoslot()), delegate
			{
				m_LoadingProgress = 0.5f;
			}, processTag);
		}
		if (CurrentModeType == GameModeType.Pause)
		{
			StopMode(CurrentModeType);
		}
		LoadingProcess.Instance.StartLoadingProcess("ActivateAdditiveArea", SceneLoader.ActivateAdditiveArea(enterPoint, isSmokeTest: false, showLoadingScreen), OnAdditiveAreaSwitched, processTag);
		MatchTimeOfDay();
		if (autoSaveMode == AutoSaveMode.AfterEntry)
		{
			LoadingProcess.Instance.StartLoadingProcess("SaveRoutine (AfterEntry)", SaveManager.SaveRoutine(SaveManager.GetNextAutoslot()), delegate
			{
				m_LoadingProgress = 0.9f;
			}, processTag);
		}
		m_LoadingProgress = 1f;
		LoadingProcess.Instance.StartLoadingProcess("AwaitingNetwork", AwaitingNetwork(), null, processTag);
	}

	public void LoadArbiter(BlueprintArea area)
	{
		LoadArea(area, null, AutoSaveMode.None);
	}

	public void MatchTimeOfDay()
	{
		if (!m_MatchTimeOfDayScheduled)
		{
			m_MatchTimeOfDayScheduled = true;
			LoadingProcess.Instance.StartLoadingProcess("MatchTimeOfDayCoroutine", MatchTimeOfDayCoroutine(), null, LoadingProcessTag.ReloadLight);
		}
	}

	public void MatchTimeOfDayForced()
	{
		TimeOfDay = Player.GameTime.TimeOfDay();
	}

	public IEnumerator<object> MatchTimeOfDayCoroutine()
	{
		if (!Application.isPlaying)
		{
			yield break;
		}
		m_MatchTimeOfDayScheduled = false;
		TimeOfDay oldTime = TimeOfDay;
		TimeOfDay = Player.GameTime.TimeOfDay();
		Task p = SceneLoader.Instance.MatchLightAndAudioScenesCoroutine();
		while (!p.IsCompleted)
		{
			yield return null;
		}
		p.Wait();
		if (TimeOfDay != oldTime)
		{
			EventBus.RaiseEvent(delegate(ITimeOfDayChangedHandler h)
			{
				h.OnTimeOfDayChanged();
			});
		}
	}

	public static IEnumerator<object> UnloadMainMenuRoutine()
	{
		if (Application.isPlaying && SceneManager.GetSceneByName(GameScenes.MainMenu).isLoaded)
		{
			AsyncOperation unload = SceneManager.UnloadSceneAsync(GameScenes.MainMenu);
			while (!unload.isDone)
			{
				yield return null;
			}
		}
	}

	public static IEnumerator<object> UnloadUnusedAssetsCoroutine()
	{
		if (Application.isPlaying)
		{
			ResourcesLibrary.CleanupLoadedCache();
			CustomPortraitsManager.Instance.Cleanup();
			GC.Collect();
			AsyncOperation unload = Resources.UnloadUnusedAssets();
			while (!unload.isDone)
			{
				yield return null;
			}
		}
	}

	private IEnumerator<object> LoadAdditiveAreas(BlueprintArea area)
	{
		IEnumerable<BlueprintArea> enumerable = GetAdditiveAreas(area).EmptyIfNull();
		foreach (BlueprintArea item in enumerable)
		{
			IEnumerator load = SceneLoader.LoadAdditiveArea(item);
			while (load.MoveNext())
			{
				yield return null;
			}
		}
	}

	public void ReloadArea()
	{
		if ((bool)CurrentlyLoadedArea)
		{
			LoadArea(CurrentlyLoadedArea, null, AutoSaveMode.None);
		}
	}

	private static IEnumerator AwaitTextureCompression()
	{
		using (CodeTimerTraceScope.New("Texture compression"))
		{
			CharacterAtlasService atlasService = Services.GetInstance<CharacterAtlasService>();
			DxtCompressorService2 dxtService = Services.GetInstance<DxtCompressorService2>();
			double startTime = Time.realtimeSinceStartupAsDouble;
			bool allDone = false;
			while (!allDone)
			{
				allDone = atlasService.RequestsCount == 0 && dxtService.RequestsCount == 0;
				if (allDone)
				{
					if (Time.realtimeSinceStartupAsDouble - startTime > 60.0)
					{
						break;
					}
					foreach (BaseUnitEntity item in Instance.Player.Party)
					{
						if (item.IsInGame)
						{
							Character character = item.View?.CharacterAvatar;
							if (character != null && character.IsTextureCompressionInProgress)
							{
								allDone = false;
								break;
							}
						}
					}
				}
				atlasService.Update();
				yield return null;
			}
			foreach (BaseUnitEntity item2 in Instance.Player.Party)
			{
				Character character2 = item2.View?.CharacterAvatar;
				if (item2.IsInGame && character2 != null && character2.IsTextureCompressionInProgress)
				{
					PFLog.Default.Error($"Timeout while waiting for texture to be compressed ({item2})");
				}
			}
		}
	}

	private static IEnumerator PreloadUnitResources()
	{
		using (CodeTimer.New("Preloading Unit Resources"))
		{
			try
			{
				ResourcesLibrary.StartPreloadingMode();
				using (CodeTimer.New("Preloading Unit Resources: Schedule"))
				{
					ResourcesPreload.PreloadUnitResources();
					ConfigRoot.Instance.HitSystemRoot.PreloadCommonHits();
				}
				while (ResourcesLibrary.TickPreloading())
				{
					yield return null;
				}
			}
			finally
			{
				ResourcesLibrary.StopPreloadingMode();
			}
		}
		using (CodeTimer.New("Prewarm pooled FX"))
		{
			ConfigRoot.Instance.HitSystemRoot.WarmupCommonHits();
		}
	}

	[SkipAnalysis]
	public void StartNewGameProcess()
	{
		_fromNewGameProcess = true;
		Player.GameId = Guid.NewGuid().ToString("N");
	}

	[SkipAnalysis]
	public void CancelNewGameProcess()
	{
		Player.GameId = null;
		_fromNewGameProcess = false;
	}

	[SkipAnalysis]
	public void LoadNewGame()
	{
		if (NewGamePreset == null)
		{
			throw new Exception("Cannot start new game. No preset specified");
		}
		LoadNewGame(NewGamePreset, ImportSave);
	}

	public void LoadNewGame(BlueprintAreaPreset preset, SaveInfo importFrom = null)
	{
		State.LoadedAreaState = new AreaPersistentState(new BlueprintArea
		{
			AssetGuid = "new-game-area",
			name = "new-game-area"
		});
		BaseUnitEntity baseUnitEntity;
		if (NewGameUnit != null)
		{
			baseUnitEntity = NewGameUnit;
			NewGameUnit = null;
			Player.CrossSceneState.AddEntityData(baseUnitEntity);
		}
		else
		{
			BlueprintUnit unit = preset.PlayerCharacter ?? ConfigRoot.Instance.NewGameSettings.DefaultPlayerCharacter;
			baseUnitEntity = AddUnitToPersistentSate(unit);
		}
		Player.SetMainCharacter(baseUnitEntity);
		if (!_fromNewGameProcess)
		{
			Player.GameId = Guid.NewGuid().ToString("N");
		}
		_fromNewGameProcess = false;
		Player.StartDate = DateTime.UtcNow;
		SaveId = Guid.Empty.ToString("N");
		Metrics.NewGame.Difficulty(MetricsUtils.GameDifficultyToString(SettingsRoot.Difficulty.GameDifficulty.GetValue())).Preset(preset.AssetGuid).Send();
		HashSet<BlueprintUnitReference> hashSet = new HashSet<BlueprintUnitReference>();
		foreach (BlueprintUnitReference companion in preset.Companions)
		{
			if ((bool)companion.Get() && !hashSet.Contains(companion))
			{
				BaseUnitEntity baseUnitEntity2 = AddUnitToPersistentSate(companion.Get());
				if ((bool)baseUnitEntity2.Faction != (bool)ConfigRoot.Instance.SystemMechanics.PlayerFaction)
				{
					baseUnitEntity2.Faction.Set(ConfigRoot.Instance.SystemMechanics.PlayerFaction);
				}
				if (!baseUnitEntity2.IsPet)
				{
					Player.AddCompanion(baseUnitEntity2);
				}
				hashSet.Add(companion);
			}
		}
		ProcessNotInPartyCompanions(preset.ExCompanions, hashSet, delegate(UnitPartCompanion companionPart)
		{
			companionPart.SetExState(CompanionExState.InReserve);
			companionPart.SetState(CompanionState.ExCompanion);
		});
		ProcessNotInPartyCompanions(preset.ExCompanionsKicked, hashSet, delegate(UnitPartCompanion companionPart)
		{
			companionPart.SetExState(CompanionExState.Kicked);
			companionPart.SetState(CompanionState.ExCompanion);
		});
		ProcessNotInPartyCompanions(preset.ExCompanionsDead, hashSet, delegate(UnitPartCompanion companionPart)
		{
			companionPart.SetExState(CompanionExState.Dead);
			companionPart.SetState(CompanionState.ExCompanion);
		});
		ProcessNotInPartyCompanions(preset.CompanionsRemote, hashSet, delegate(UnitPartCompanion companionPart)
		{
			companionPart.SetState(CompanionState.Remote);
		});
		Instance.Controllers.EntitySpawner.Tick();
		try
		{
			preset.SetupState();
		}
		catch (Exception ex)
		{
			if (preset.IsNewGamePreset)
			{
				throw;
			}
			Logger.Exception(ex);
		}
		Player.MinDifficultyController.UpdateMinDifficulty(force: true);
		Player.InvalidateCharacterLists();
		AreaDataStash.ClearDirectory();
		AreaDataStash.PrepareFirstLaunch();
		LoadArea(preset.EnterPoint, preset.MakeAutosave ? AutoSaveMode.AfterEntry : AutoSaveMode.None);
		LoadingProcess.Instance.StartLoadingProcess("Start game actions", Enumerable.Empty<object>().GetEnumerator(), delegate
		{
			PFLog.Default.Log("Running start-game actions");
			ConfigRoot.Instance.NewGameSettings.StartGameActions.Run();
			preset.StartGameActions.Run();
			if (importFrom != null && importFrom.Campaign.DlcReward == null && preset.Campaign != null)
			{
				PFLog.Default.Log($"Importing from main campaign save {importFrom}");
				BlueprintCampaign campaign = preset.Campaign;
				if (campaign.IsImportRequired && campaign.ImportFromMainCampaign.Condition.Check())
				{
					campaign.ImportFromMainCampaign.DoImport(importFrom);
				}
			}
			BlueprintCampaign campaign2 = preset.Campaign;
			if (campaign2 != null && campaign2.DlcReward != null)
			{
				Player.UsedDlcRewards.Add(preset.Campaign.DlcReward);
			}
			if (preset.Campaign != null)
			{
				foreach (BlueprintDlc item in preset.Campaign.AdditionalContentDlc)
				{
					if (item.IsActive)
					{
						foreach (IBlueprintDlcReward reward in item.Rewards)
						{
							if (reward is BlueprintDlcRewardCampaignAdditionalContent blueprintDlcRewardCampaignAdditionalContent && blueprintDlcRewardCampaignAdditionalContent.Campaign == preset.Campaign)
							{
								Player.UsedDlcRewards.Add(blueprintDlcRewardCampaignAdditionalContent);
							}
						}
					}
				}
			}
		});
		LoadingProcess.Instance.StartLoadingProcess("StartInSaveSettings", Enumerable.Empty<object>().GetEnumerator(), delegate
		{
			SettingsController.Instance.StartInSaveSettings();
			Metrics.Player.Send();
		});
	}

	private void ProcessNotInPartyCompanions(IEnumerable<BlueprintUnitReference> companions, ISet<BlueprintUnitReference> processedCompanions, Action<UnitPartCompanion> setState)
	{
		foreach (BlueprintUnitReference companion in companions)
		{
			if ((bool)companion.Get() && !processedCompanions.Contains(companion))
			{
				BaseUnitEntity baseUnitEntity = AddUnitToPersistentSate(companion.Get());
				baseUnitEntity.IsInGame = false;
				UnitPartCompanion orCreate = baseUnitEntity.GetOrCreate<UnitPartCompanion>();
				setState(orCreate);
				processedCompanions.Add(companion);
			}
		}
	}

	[SkipAnalysis]
	public void ResetToMainMenu(Exception exception = null)
	{
		SaveId = Guid.Empty.ToString("N");
		LoadingProcess.Instance.StopAll();
		LoadingProcess.Instance.StartLoadingProcess("LoadMainMenuCoroutine", SceneLoader.LoadMainMenuCoroutine(exception), delegate
		{
			SettingsController.Instance.StopInSaveSettings();
			Time.timeScale = 1f;
			m_LoadingProgress = 1f;
		});
	}

	private static IEnumerator<object> AwaitingUserInput()
	{
		if (!IsTestRun)
		{
			EventBus.RaiseEvent(delegate(IStartAwaitingUserInput h)
			{
				h.OnStartAwaitingUserInput();
			});
			while ((bool)LoadingProcess.Instance.IsAwaitingUserInput)
			{
				yield return null;
			}
		}
	}

	public static IEnumerator<object> AwaitingNetwork()
	{
		while (!NetworkingManager.LockOn(NetLockPointId.LoadingProcess))
		{
			yield return null;
		}
	}

	public static IEnumerator<object> AwaitingCommands()
	{
		while (Wait())
		{
			yield return null;
		}
		static bool Wait()
		{
			if (!NetService.Instance.Initialized)
			{
				return false;
			}
			if (!Instance.RealTimeController.TickCompleted)
			{
				return false;
			}
			int currentNetworkTick = Instance.RealTimeController.CurrentNetworkTick;
			if (Instance.RealTimeController.NextTickCommandsReady(currentNetworkTick))
			{
				return false;
			}
			return true;
		}
	}

	public void QuickLoadGame()
	{
		SaveManager.UpdateSaveListIfNeeded();
		SaveInfo newestQuickslot = SaveManager.GetNewestQuickslot();
		if (newestQuickslot == null)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(WarningNotificationType.NoQuickSaves);
			});
			PFLog.Default.Warning($"Unable to find any Quick Saves for current GameId={Instance.Player.GameId}. IronMan={SettingsRoot.Difficulty.OnlyOneSave.GetValue()}. List of found saves below.");
			StringBuilder stringBuilder = new StringBuilder();
			foreach (SaveInfo item in SaveManager)
			{
				stringBuilder.Append($"\"{item.FileName}\";{item.Type};{item.GameId}\n");
			}
			PFLog.Default.Log(stringBuilder.ToString());
		}
		else
		{
			LoadGame(newestQuickslot);
		}
	}

	public void LoadLastSave()
	{
		SaveManager.UpdateSaveListIfNeeded();
		SaveInfo latestSave = SaveManager.GetLatestSave();
		if (latestSave != null)
		{
			LoadGameFromMainMenu(latestSave);
		}
		else
		{
			LoadNewGame();
		}
	}

	[SkipAnalysis]
	public void LoadGame([NotNull] SaveInfo saveInfo, [CanBeNull] Action callback = null)
	{
		try
		{
			Services.GetInstance<LogThreadService>()?.Cleanup();
		}
		catch (Exception ex)
		{
			PFLog.Default.Log(ex);
		}
		if (PhotonManager.Lobby.IsActive)
		{
			PhotonManager.NetGame.StartGame((SaveInfoKey)saveInfo, callback);
		}
		else
		{
			LoadGameLocal(saveInfo, callback);
		}
	}

	[SkipAnalysis]
	public void LoadGameLocal([NotNull] SaveInfo saveInfo, [CanBeNull] Action callback = null)
	{
		if (IsInMainMenu)
		{
			Instance.RootUIContext.SaveLoadContextLoad(saveInfo, callback);
		}
		else
		{
			LoadGameForce(saveInfo, callback);
		}
	}

	private void LoadGameForce([NotNull] SaveInfo saveInfo, [CanBeNull] Action callback = null)
	{
		SaveId = saveInfo.SaveId;
		SoundState.Instance.ResetBeforeUnloading();
		LoadArea(saveInfo.Area, null, AutoSaveMode.None, saveInfo, callback);
	}

	public void LoadGameForSmokeTest(SaveInfo save)
	{
		Instance.RootUIContext.SetLoadingArea(save.Area);
		if (CurrentlyLoadedArea != null)
		{
			LoadingProcess.Instance.StartLoadingProcess("UnloadAreaCoroutine", SceneLoader.UnloadAreaCoroutine(forDispose: true));
			LoadingProcess.Instance.StartLoadingProcess("UnloadEntitiesCoroutine", SceneLoader.UnloadEntitiesCoroutine(unloadCrossScene: true));
		}
		LoadingProcess.Instance.StartLoadingProcess("LoadRoutine", SaveManager.LoadRoutine(save, isSmokeTest: true));
		LoadingProcess.Instance.StartLoadingProcess("LoadAreaCoroutine", SceneLoader.LoadAreaCoroutine(save.Area, save.AreaPart, null, isSmokeTest: true), OnAreaLoaded);
	}

	[SkipAnalysis]
	public void LoadGameFromMainMenu(SaveInfo saveInfo, [CanBeNull] Action callback = null)
	{
		LoadGameForce(saveInfo, callback);
	}

	public void MakeQuickSave(Action callback = null)
	{
		SaveManager.UpdateSaveListIfNeeded();
		RequestSaveGame(SaveManager.GetNextQuickslot(), null, callback);
	}

	public void RequestSaveGame([CanBeNull] SaveInfo saveInfo, [CanBeNull] string saveName = null, Action callback = null)
	{
		Instance.GameCommandQueue.SaveGame(saveInfo, saveName, callback);
	}

	public void SaveGame(SaveInfo saveInfo, Action callback = null)
	{
		if (saveInfo.Type != SaveInfo.SaveType.ForImport)
		{
			if (!SaveManager.IsSaveAllowed(saveInfo.Type))
			{
				if (saveInfo.Type != SaveInfo.SaveType.Auto)
				{
					EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
					{
						h.HandleWarning(WarningNotificationType.SavingImpossible);
					});
				}
				return;
			}
			if (saveInfo.Type != SaveInfo.SaveType.Bugreport && (bool)SettingsRoot.Difficulty.OnlyOneSave)
			{
				if (!SaveManager.IsIronmanSave(saveInfo))
				{
					EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
					{
						h.HandleWarning(WarningNotificationType.SavingImpossibleIronmanWillSavedAutomatically);
					});
					return;
				}
				if (saveInfo.Type != SaveInfo.SaveType.Auto && saveInfo.Type != SaveInfo.SaveType.IronMan)
				{
					EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
					{
						h.HandleWarning(WarningNotificationType.SavingImpossibleIronmanWillSavedAutomatically);
					});
					return;
				}
				saveInfo.Type = SaveInfo.SaveType.IronMan;
			}
		}
		LoadingProcess.Instance.StartLoadingProcess("SaveRoutine (manual)", SaveManager.SaveRoutine(saveInfo, forceAuto: false, showNotification: true), callback, LoadingProcessTag.Save);
	}

	private void OnLoadingScenesProgress(float value)
	{
		m_LoadingScenesProgress = value;
	}

	public void ResetLoadingProgress()
	{
		m_LoadingProgress = 0f;
		m_LoadingScenesProgress = 0f;
	}

	private static IEnumerator<object> DetachFromInstanceImpl(Func<IEnumerator<object>> enumeratorFn)
	{
		IEnumerator<object> enumerator = enumeratorFn();
		while (enumerator.MoveNext())
		{
			yield return enumerator.Current;
		}
	}

	private static void StartLoadingProcessDetached(string profilingName, Func<IEnumerator<object>> enumeratorFn, Action callback = null)
	{
		LoadingProcess.Instance.StartLoadingProcess(profilingName, DetachFromInstanceImpl(enumeratorFn), callback);
	}

	private PersistentState GetPersistentState()
	{
		IPersistentState persistentState = InterfaceServiceLocator.TryGetService<IPersistentState>();
		if (persistentState != null)
		{
			return (PersistentState)persistentState;
		}
		PersistentState persistentState2 = new PersistentState();
		InterfaceServiceLocator.RegisterService(persistentState2, typeof(IPersistentState));
		return persistentState2;
	}

	private void EnsureSingletonEntity<T>(EntityRef<T> @ref, SceneEntitiesState state) where T : Entity, new()
	{
		if (@ref.Entity == null)
		{
			T data = Entity.Initialize(new T());
			state.AddEntityData(data);
		}
	}

	[SkipAnalysis]
	private void InitializeState()
	{
		EnsureSingletonEntity(Player.Ref, State.PlayerState);
		EnsureSingletonEntity(DetectiveSystem.Ref, State.PlayerState);
		EnsureSingletonEntity(QuestBook.Ref, State.PlayerState);
		EnsureSingletonEntity(EtudesSystem.Ref, State.PlayerState);
		EnsureSingletonEntity(TutorialSystem.Ref, State.PlayerState);
		EnsureSingletonEntity(DialogState.Ref, State.PlayerState);
		EnsureSingletonEntity(ReputationState.Ref, State.PlayerState);
		EnsureSingletonEntity(VendorsManager.Ref, State.PlayerState);
		EnsureSingletonEntity(AsksState.Ref, State.PlayerState);
		EnsureSingletonEntity(PartySharedInventory.Ref, State.CrossSceneState);
	}

	[SkipAnalysis]
	private Game()
	{
		if (Application.isPlaying)
		{
			RootUIContext = RootUIContextService.CreateRootUIContext();
		}
		State = GetPersistentState();
		InitializeState();
	}

	public void Initialize()
	{
		if (m_AlreadyInitialized)
		{
			return;
		}
		PFLog.System.Log("Initializing Game. Version: " + GameVersion.GetVersion());
		using (CodeTimer.New("Game ctor"))
		{
			InitializeControllerMode();
			CursorController.Activate();
			TextTemplateEngineProxy.Instance.Initialize(TextTemplateEngine.Instance);
			LocalizationManager.Instance.Init(SettingsRoot.Game.Main.Localization, SettingsController.Instance, !SettingsRoot.Game.Main.LocalizationWasTouched.GetValue());
			InitializeKeyboardBindings();
			using (CodeTimer.New("Initialize Cheats"))
			{
				if (BuildModeUtility.IsDevelopment)
				{
					CheatsCommon.RegisterCheats(Keyboard);
					CheatsRelease.RegisterCheats(Keyboard);
				}
				CheatsManagerHolder.System.Database.SetExternals(SmartConsole.CommandNames);
			}
			UICamera.Claim();
			Player.InitializeHack();
			SaveManager.UpdateSaveListAsync();
			SceneEntitiesState.OnAdded += CrossSceneStateHandler;
			SceneEntitiesState.OnRemoved += CrossSceneStateHandler;
			GameHistoryLog.Initialize();
			m_AlreadyInitialized = true;
		}
	}

	public void SpeedUp(bool state)
	{
		TurnController turnController = Controllers.TurnController;
		if (!IsPaused && !UtilityGame.IsGlobalMap() && turnController.TurnBasedModeActive && (!turnController.IsSpaceCombat || UtilityNet.IsControlMainCharacter()) && !turnController.IsPreparationTurn)
		{
			if (state)
			{
				GameCommandQueue.DoSpeedUp();
			}
			else
			{
				GameCommandQueue.StopSpeedUp();
			}
		}
	}

	[SkipAnalysis]
	public static void EnsureGameLifetimeServices()
	{
		if (Services.GetInstance<SoundState>() == null)
		{
			Services.RegisterServiceInstance(new SoundState());
		}
		if (Services.GetInstance<DxtCompressorService2>() == null)
		{
			Services.RegisterServiceInstance(new DxtCompressorService2());
		}
		if (Services.GetInstance<CharacterAtlasService>() == null)
		{
			Services.RegisterServiceInstance(new CharacterAtlasService());
		}
		if (Services.GetInstance<FXPrewarmService>() == null)
		{
			Services.RegisterServiceInstance(new FXPrewarmService());
		}
		if (Services.GetInstance<LogThreadService>() == null)
		{
			Services.RegisterServiceInstance(new LogThreadService());
		}
		if (Services.GetInstance<ReportingUtils>() == null)
		{
			Services.RegisterServiceInstance(new ReportingUtils());
		}
		if (Services.GetInstance<MouseHoverBlueprintSystem>() == null)
		{
			Services.RegisterServiceInstance(new MouseHoverBlueprintSystem());
		}
		if (Services.GetInstance<EscHotkeyManager>() == null)
		{
			Services.RegisterServiceInstance(new EscHotkeyManager());
		}
		if (Services.GetInstance<UISounds>() == null)
		{
			Services.RegisterServiceInstance(new UISounds());
		}
		UnitAsksService.Ensure();
	}

	public void Tick()
	{
		StateUnchangedOutsideTickCheck.BeforeTick();
		bool isLoadingInProcess = LoadingProcess.Instance.IsLoadingInProcess;
		LoadingProcess.Instance.Tick();
		NetworkingManager.ReceivePackets();
		int num = 9;
		do
		{
			TickInternal(isLoadingInProcess);
			ContextData.Check();
			num--;
			if (num == 0)
			{
				if (!BuildModeUtility.Data.LimitDeltaTimeForProfiling)
				{
					PFLog.Replay.Log("Game.Tick: max tick count per frame has been exceeded!");
				}
				break;
			}
		}
		while (RealTimeController.OneMoreTick);
		StateUnchangedOutsideTickCheck.AfterTick();
		Services.GetInstance<CharacterAtlasService>()?.Update();
		Services.GetInstance<FXPrewarmService>()?.Update();
	}

	private void TickInternal(bool wasLoadingInProcess)
	{
		if (!IsLoadingInProcess())
		{
			NetService.Instance.Init();
			using (ProfileScope.New("Start Tick Real Time"))
			{
				RealTimeController.Tick();
			}
			TryDoPauseRequest();
			GameCommandQueue.Tick();
			if (m_WasLoadingForTheAssert && !RealTimeController.IsSimulationTick)
			{
				throw new Exception("Logic error in loading process. Expecting System tick as first tick after loading");
			}
			if (m_GameModes.Count <= 0)
			{
				throw new Exception("Logic error: we have zero game modes");
			}
			m_WasLoadingForTheAssert = false;
		}
		if (IsLoadingInProcess())
		{
			using (ProfileScope.New("Stats Tick (Loading)"))
			{
				Statistic.Tick(this);
			}
			SoundState.Instance.UpdateScheduledAreaMusic();
			RealTimeController.Suspend();
			Controllers.TimeController.Suspend();
			m_WasLoadingForTheAssert = true;
			return;
		}
		using (ProfileScope.New("Update SoundState"))
		{
			SoundState.Instance.Update();
		}
		if (LoadingProcess.Instance.IsLoadingScreenActive && m_GameModes.Count <= 0)
		{
			return;
		}
		try
		{
			m_GameModeTicking = true;
			using (ProfileScope.New("Cleanup Awake Units"))
			{
				EntityPools.AllAwakeUnits.RemoveAll((AbstractUnitEntity i) => !i.IsInState);
			}
			using (ProfileScope.New("Tick Game Mode"))
			{
				m_GameModes.Peek().Tick();
			}
			using (ProfileScope.New("Stats Tick (Game)"))
			{
				Statistic.Tick(this);
			}
			using (ProfileScope.New("Tick UnitAsksController"))
			{
				UnitAsksService.Instance.Tick();
			}
		}
		finally
		{
			m_GameModeTicking = false;
		}
		if (PauseOnLoadPending)
		{
			PauseOnLoadPending = false;
			IsPaused = true;
		}
		using (ProfileScope.New("Finish Tick Real Time"))
		{
			RealTimeController.FinishTick();
		}
		bool IsLoadingInProcess()
		{
			if (!wasLoadingInProcess)
			{
				return LoadingProcess.Instance.IsLoadingInProcess;
			}
			return true;
		}
	}

	public void HandleQuit()
	{
		SmartConsole.ClearRegistrations();
		CheatsManagerHolder.System.Database.SetExternals(SmartConsole.CommandNames);
		PlayerPrefs.Save();
		Statistic.Quit();
		StaticInfoCollector.Quit();
		SaveManager.WaitCommit();
		AreaDataStash.CloseAndDelete();
		Owlcat.Runtime.Core.Logging.Logger.Instance.DisposeLogSinks();
	}

	public void AdvanceGameTime(TimeSpan delta)
	{
		Instance.Controllers.TimeController.AdvanceGameTime(delta);
	}

	public void HandleAreaBeginUnloading()
	{
		if (Player.IsInCombat)
		{
			foreach (BaseUnitEntity allCrossSceneUnit in Player.AllCrossSceneUnits)
			{
				if (allCrossSceneUnit.IsInCombat)
				{
					allCrossSceneUnit.CombatState.LeaveCombat();
				}
			}
		}
		EventBus.RaiseEvent(delegate(IAreaHandler h)
		{
			h.OnAreaBeginUnloading();
		});
		Controllers.EntitySpawner.Tick();
		Controllers.EntityDestroyer.Tick();
		StopAllModes();
		FxHelper.DestroyAll();
		GameObjectsPool.ClearPool();
		Controllers.EntityBoundsController.HandleAreaBeginUnloading();
	}

	public void HandleAdditiveAreaBeginDeactivating()
	{
		if (Player.IsInCombat)
		{
			foreach (BaseUnitEntity allCrossSceneUnit in Player.AllCrossSceneUnits)
			{
				if (allCrossSceneUnit.IsInCombat)
				{
					allCrossSceneUnit.CombatState.LeaveCombat();
				}
			}
		}
		EventBus.RaiseEvent(delegate(IAdditiveAreaSwitchHandler h)
		{
			h.OnAdditiveAreaBeginDeactivated();
		});
		Controllers.EntitySpawner.Tick();
		Controllers.EntityDestroyer.Tick();
		StopAllModes();
		FxHelper.DestroyAll();
		GameObjectsPool.ClearPool();
	}

	public void Teleport([NotNull] BlueprintAreaEnterPoint areaEnterPoint, bool includeFollowers = false, Action callback = null)
	{
		if (areaEnterPoint == null)
		{
			throw new ArgumentException("areaEnterPoint is null", "areaEnterPoint");
		}
		if (CurrentlyLoadedArea != areaEnterPoint.Area)
		{
			throw new InvalidOperationException($"Cant teleport to {areaEnterPoint}. Target zone {areaEnterPoint.Area} should be same as current {CurrentlyLoadedArea}");
		}
		LoadingProcess.Instance.StartLoadingProcess("TeleportPartyCoroutine", TeleportPartyCoroutine(areaEnterPoint, includeFollowers), delegate
		{
			ExecuteSafe(callback);
		}, LoadingProcessTag.TeleportParty);
		EventBus.RaiseEvent(delegate(IAreaTransitionHandler h)
		{
			h.HandleAreaTransition();
		});
	}

	private IEnumerator<object> TeleportPartyCoroutine([NotNull] BlueprintAreaEnterPoint areaEnterPoint, bool includeFollowers)
	{
		if (areaEnterPoint == null)
		{
			throw new ArgumentException("areaEnterPoint is null", "areaEnterPoint");
		}
		BlueprintAreaPart areaPart = areaEnterPoint.AreaPart;
		if (areaPart == null)
		{
			throw new Exception(string.Format("{0} {1} has null {2}. Most likely this {3} is outside of mechanics bounds of any {4}.", "BlueprintAreaEnterPoint", areaEnterPoint, "AreaPart", "BlueprintAreaEnterPoint", "AreaPart"));
		}
		AreaEnterPoint enterPoint = AreaEnterPoint.FindAreaEnterPointOnScene(areaEnterPoint);
		if (enterPoint == null)
		{
			throw new Exception($"Cant find view of area enter point {areaEnterPoint}");
		}
		LoadingProcess.Instance.StartLoadingProcess("MatchTimeOfDayCoroutine", MatchTimeOfDayCoroutine(), null, LoadingProcessTag.TeleportParty);
		NetService.Instance.CancelCurrentCommands();
		if (CurrentlyLoadedAreaPart != areaPart)
		{
			IEnumerator switchPart = SceneLoader.SwitchToAreaPartCoroutine(areaPart);
			while (switchPart.MoveNext())
			{
				yield return null;
			}
		}
		Player.MoveCharacters(enterPoint, includeFollowers, moveCamera: true);
	}

	private static IEnumerator<object> ResetGame(BlueprintArea area)
	{
		Task resetTask = Reset();
		while (!resetTask.IsCompleted)
		{
			yield return null;
		}
		resetTask.Wait();
		Instance.RootUIContext.StartUI();
		Instance.RootUIContext.SetLoadingArea(area);
	}

	private void TryFixPartyPositions()
	{
		List<BaseUnitEntity> list = Player.PartyAndPets.Where(AreaEnterPoint.ShouldMoveCharacterOnAreaEnterPoint).ToList();
		BaseUnitEntity baseUnitEntity = list.Find((BaseUnitEntity c) => c == Player.MainCharacter.Entity && CurrentlyLoadedAreaPart.Bounds.MechanicBounds.Contains(c.Position));
		if (baseUnitEntity == null)
		{
			baseUnitEntity = list.Find((BaseUnitEntity c) => CurrentlyLoadedAreaPart.Bounds.MechanicBounds.Contains(c.Position));
		}
		if (baseUnitEntity == null)
		{
			return;
		}
		foreach (BaseUnitEntity item in list)
		{
			if (!CurrentlyLoadedAreaPart.Bounds.MechanicBounds.Contains(item.Position))
			{
				item.Position = baseUnitEntity.Position;
				item.SnapToGrid();
				PFLog.Pathfinding.Error("Fixing " + item.CharacterName + " position. Was in wrong area!");
			}
		}
	}

	public IEnumerable<BlueprintArea> GetAdditiveAreas(BlueprintArea area)
	{
		return Enumerable.Empty<BlueprintArea>();
	}

	private static void ExecuteSafe([CanBeNull] Action action)
	{
		try
		{
			action?.Invoke();
		}
		catch (Exception ex)
		{
			PFLog.System.Exception(ex);
		}
	}

	private void OnAreaLoaded()
	{
		TryFixPartyPositions();
		HandleActiveAreaChanged(wasSwitched: false);
	}

	private void OnAdditiveAreaSwitched()
	{
		HandleActiveAreaChanged(wasSwitched: true);
		m_LoadingProgress = 0.8f;
	}

	private void HandleActiveAreaChanged(bool wasSwitched)
	{
		PFLog.System.Log("HandleActiveAreaChanged: started");
		SimpleCaster.WarmupPool();
		EventBus.RaiseEvent(delegate(IAreaLoadingStagesHandler h)
		{
			h.OnAreaScenesLoaded();
		});
		m_AreaWasSwitched = wasSwitched;
		PFLog.System.Log("HandleActiveAreaChanged: OnAreaScenesLoaded() finished");
		EntityPools.SetNewAwakeUnits(Instance.EntityPools.AllUnits.NotDead());
		GameModeType gameModeType = (Player.GameOverReason.HasValue ? GameModeType.GameOver : CurrentlyLoadedArea.AreaStatGameMode);
		PFLog.System.Log($"HandleActiveAreaChanged: start mode {gameModeType} for {CurrentlyLoadedArea} {CurrentlyLoadedArea.AreaStatGameMode}");
		if (m_GameModeSwitchOnLoadHandler == null)
		{
			m_GameModeSwitchOnLoadHandler = new GameModeSwitchOnLoadHandler(this);
		}
		EventBus.Subscribe(m_GameModeSwitchOnLoadHandler);
		DoStartMode(gameModeType);
		PFLog.System.Log("HandleActiveAreaChanged: GameMode activated");
	}

	private void OnAreaLoadGameModeSet()
	{
		PFLog.System.Log("OnAreaLoaded: GameMode activated");
		EventBus.Unsubscribe(m_GameModeSwitchOnLoadHandler);
		if (m_AreaWasSwitched)
		{
			EventBus.RaiseEvent(delegate(IAdditiveAreaSwitchHandler h)
			{
				h.OnAdditiveAreaDidActivated();
			});
			PFLog.System.Log("HandleActiveAreaChanged: OnAdditiveAreaDidActivated() finished");
		}
		else
		{
			EventBus.RaiseEvent(delegate(IAreaHandler h)
			{
				h.OnAreaDidLoad();
			});
			PFLog.System.Log("HandleActiveAreaChanged: OnAreaDidLoad() finished");
		}
		m_AreaWasSwitched = false;
		Controllers.EntitySpawner.Tick();
		PFLog.System.Log("HandleActiveAreaChanged: RandomEncounterInitializer.HandleAreaLoaded() finished");
		LoadedAreaState.Activate();
		PFLog.System.Log("HandleActiveAreaChanged: CurrentScene.Activate() finished");
		EtudesSystem.UpdateEtudes();
		_ = MonoSingleton<ParticleSystemCustomSubEmitterDelegate>.Instance;
		_ = ConfigRoot.Instance.BlueprintDismembermentRoot?.WeaponsListDismArray;
		EventBus.RaiseEvent(delegate(IAreaActivationHandler h)
		{
			h.OnAreaActivated();
		});
		PFLog.System.Log("HandleActiveAreaChanged: OnAreaActivated() finished");
		Controllers.EntitySpawner.Tick();
		Controllers.EntityDestroyer.Tick();
		SpawnBuffsFxs();
		PFLog.System.Log("HandleActiveAreaChanged: Player.OnAreaLoaded() finished");
		foreach (BaseUnitEntity partyAndPet in Player.PartyAndPets)
		{
			partyAndPet.LifeState.HideIfDead();
		}
		UnitsPlacer.MovePartyToNavmesh();
		PFLog.System.Log("HandleActiveAreaChanged: finished");
		Metrics.LocationData.Id(Instance.CurrentlyLoadedArea.AssetGuid).Experience(Instance.Player.MainCharacterEntity.GetProgressionOptional()?.Experience ?? (-1)).ExperienceLevel(Instance.Player.MainCharacterEntity.GetProgressionOptional()?.ExperienceLevel ?? (-1))
			.Money(Instance.Player.Money)
			.Difficulty(MetricsUtils.GameDifficultyToString(SettingsRoot.Difficulty.GameDifficulty.GetValue()))
			.Formation(Instance.Player.FormationManager.SelectedFormation.AssetGuid)
			.CompanionCount(Instance.Player.Party.Count)
			.Reputation(from r in ReputationHelper.GetReputationsRespect()
				select r.ToString())
			.Send();
		foreach (BaseUnitEntity item in Instance.Player.Party)
		{
			Metrics.LocationCompanion.Id(item.Blueprint.AssetGuid).Level(item.GetOptional<PartUnitProgression>()?.CharacterLevel ?? (-1)).Equipment(from s in item.Body.AllSlots
				where s.HasItem
				select s.Item.Blueprint.AssetGuid)
				.Alignment(from r in item.Alignment.GetAlignmentRanks()
					select r.ToString())
				.Send();
		}
	}

	private IEnumerator<object> AreaLoadingComplete()
	{
		yield return null;
		LightProbes.TetrahedralizeAsync();
		Player.VisitedAreas.Add(CurrentlyLoadedArea);
		yield return null;
		EntityService.Instance.GetTempList<Entity>().ForEach(delegate(Entity i)
		{
			i.AreaLoadingComplete();
		});
		EventBus.RaiseEvent(delegate(IAreaLoadingStagesHandler h)
		{
			h.OnAreaLoadingComplete();
		});
		MaybeSuggestDLCImport();
		yield return null;
		ParticleSystemsQualityController.Instance.Init();
	}

	private void SpawnBuffsFxs()
	{
		foreach (BaseUnitEntity item in State.CrossSceneState.AllEntityData.OfType<BaseUnitEntity>())
		{
			item.Buffs.SpawnBuffsFxs();
		}
		foreach (BaseUnitEntity item2 in State.LoadedAreaState.AllEntityData.OfType<BaseUnitEntity>())
		{
			item2.Buffs.SpawnBuffsFxs();
		}
	}

	public void MaybeSuggestDLCImport()
	{
		if (Player.Campaign.DlcReward != null)
		{
			return;
		}
		foreach (IBlueprintDlcReward dlc in (ConfigRoot.Instance.DlcSettings.Dlcs?.SelectMany((IBlueprintDlc dlc) => dlc.Rewards)).EmptyIfNull())
		{
			if (Player.UsedDlcRewards.Any((BlueprintDlcReward dlcReward) => dlcReward == dlc))
			{
				continue;
			}
			BlueprintDlcRewardCampaign dlcCampaign = dlc as BlueprintDlcRewardCampaign;
			if (dlcCampaign == null || !dlcCampaign.IsAvailable)
			{
				continue;
			}
			SaveImportSettings importSettings = Player.Campaign.GetImportSettings(dlcCampaign.Campaign, newGame: false);
			if (importSettings == null || !importSettings.Condition.Check())
			{
				continue;
			}
			List<SaveInfo> saves = DlcSaveImporter.GetSavesForImport(dlcCampaign.Campaign);
			if (saves != null && saves.Count > 0)
			{
				PFLog.System.Log($"Can import save from {dlcCampaign.Campaign} campaign from {dlcCampaign} DLC reward.");
				EventBus.RaiseEvent(delegate(ICampaignImportHandler h)
				{
					h.HandleSaveImport(dlcCampaign.Campaign, saves);
				});
			}
		}
	}

	[NotNull]
	private BaseUnitEntity AddUnitToPersistentSate(BlueprintUnit unit)
	{
		BaseUnitEntity baseUnitEntity = unit.CreateEntity();
		Player.CrossSceneState.AddEntityData(baseUnitEntity);
		return baseUnitEntity;
	}

	private void CrossSceneStateHandler(SceneEntitiesState state, Entity data)
	{
		if (state == State.CrossSceneState)
		{
			Player.Ref.Entity?.InvalidateCharacterLists();
		}
	}

	public void ResetState()
	{
		GamesModeFactoryFacade.ResetControllers();
		State.Reset();
		SceneLoader.ClearLoadedArea();
		EntityPools.ClearAwakeUnits();
		UnitGroups.Clear();
		Services.EndLifetime(ServiceLifetimeType.GameSession);
	}

	public static async Task Reset()
	{
		ControllerModeType controllerMode = s_Instance.ControllerMode;
		await GamesModeFactoryFacade.Instance.Reset();
		SceneEntitiesState.ClearSubscriptions();
		object obj = s_Instance?.RootUIContext.GetLoadingScreenVM();
		s_Instance?.Dispose();
		KeyboardAccess.Instance.UnbindAll();
		InterfaceServiceLocator.UnregisterService(typeof(ITimeController));
		InterfaceServiceLocator.UnregisterService(typeof(IPersistentState));
		s_Instance = new Game
		{
			ControllerMode = controllerMode,
			m_LoadingProgress = 0.2f
		};
		DontChangeController = true;
		s_Instance.Initialize();
		if (obj != null)
		{
			s_Instance.RootUIContext.SetLoadingScreenVM(obj);
		}
		ObjectExtensions.Or(CameraRig.Instance, null)?.RenewKeyboardBindings();
		IngameConsoleKeybinds.Refresh();
	}

	private void Dispose()
	{
		Statistic?.Reset();
		RootUIContext.Clear();
		CursorController?.Deactivate();
		StopAllModes();
		ResetState();
		GridNodeToEntityCache.Dispose();
		m_QuickSlotReplenish.Dispose();
		GlobalEffectDirector.Shared.Reset();
	}
}
