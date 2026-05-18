using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.SceneControllables;
using Kingmaker.Code.Controllers.Interactions;
using Kingmaker.Code.Framework.GameLog;
using Kingmaker.Code.Gameplay.Controllers;
using Kingmaker.Code.Gameplay.Controllers.Asks;
using Kingmaker.Code.Gameplay.Controllers.Combat;
using Kingmaker.Code.Gameplay.Controllers.DetectiveRadar;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Clicks;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.Dialog;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.MapObjects;
using Kingmaker.Controllers.MovePrediction;
using Kingmaker.Controllers.Net;
using Kingmaker.Controllers.Optimization;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.Controllers.Rest;
using Kingmaker.Controllers.Timer;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Controllers.Units;
using Kingmaker.Controllers.UnityEventsReplacements;
using Kingmaker.Framework.Pathfinding;
using Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;
using Kingmaker.Gameplay.Features.Elevator;
using Kingmaker.Gameplay.Features.Encounter;
using Kingmaker.Plugins.CoopDesyncAnalyzer.Attributes;
using Kingmaker.Tutorial;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.AI;

namespace Kingmaker.GameModes;

public class GamesModeFactoryFacade
{
	private static GamesModeFactoryFacade s_Instance;

	private static readonly GameModeType None = GameModeType.None;

	private static readonly GameModeType Default = GameModeType.Default;

	private static readonly GameModeType GlobalMap = GameModeType.GlobalMap;

	private static readonly GameModeType Dialog = GameModeType.Dialog;

	private static readonly GameModeType Pause = GameModeType.Pause;

	private static readonly GameModeType Cutscene = GameModeType.Cutscene;

	private static readonly GameModeType GameOver = GameModeType.GameOver;

	private static readonly GameModeType BugReport = GameModeType.BugReport;

	private static readonly GameModeType CutsceneGlobalMap = GameModeType.CutsceneGlobalMap;

	[Obsolete]
	private static readonly GameModeType SpaceCombat = GameModeType.SpaceCombat;

	[Obsolete]
	private static readonly GameModeType StarSystem = GameModeType.StarSystem;

	public static GamesModeFactoryFacade Instance => s_Instance ?? (s_Instance = new GamesModeFactoryFacade());

	private static ReadonlyList<GameModeType> All => GameModeType.All;

	[CanBeNull]
	public T GetController<T>() where T : class, IController
	{
		return GameModesFactory.GetController<T>();
	}

	private GamesModeFactoryFacade()
	{
		Initialize();
	}

	public GameMode Create(GameModeType type)
	{
		Initialize();
		return GameModesFactory.Create(type);
	}

	public async Task Reset()
	{
		await GameModesFactory.Reset();
		Initialize();
	}

	[SkipAnalysis]
	private void Initialize()
	{
		if (!GameModesFactory.AllControllers.Any())
		{
			PointerController controller = new PointerController(new ClickWithSelectedAbilityHandler(), new ClickUnitHandler(), new ClickMapObjectHandler(), new ClickGroundHandler(), new ClickOnDetectClicksObjectHandler(), new ClickSurfaceDeploymentHandler(), new ClickCheatRemoteTeleportHandler());
			Register(new SuppressEntitiesController(), Default, Dialog, Pause, Cutscene);
			Register(new UnitGroupsController(), Default, SpaceCombat, StarSystem, Dialog, Cutscene);
			Register(new SlowMoController(), Default, Cutscene);
			Register(new TimeController(), Default, SpaceCombat, StarSystem, Dialog, Cutscene, GlobalMap, CutsceneGlobalMap);
			Register(new KeyboardAccessTicker(), Default, SpaceCombat, StarSystem, Dialog, Pause, Cutscene, GlobalMap, CutsceneGlobalMap, GameOver, BugReport);
			Register(new UnitPlaceOnGroundController(), Default, Pause);
			Register(new PauseController(), All);
			Register(new UnpauseController(), Default, SpaceCombat, StarSystem, Pause);
			Register(controller, Default, SpaceCombat, Pause);
			Register(new InteractionHighlightController(), Default, SpaceCombat, StarSystem, Pause);
			Register(new DirectorAdapterController(), Default, Dialog, Cutscene, CutsceneGlobalMap);
			Register(new DoorUpdateController(), Default, Pause, Cutscene, StarSystem, Dialog);
			Register(new PathfindingController(), GameModeType.All);
			Register(new CutsceneController(tickBackground: false), Dialog, Cutscene, CutsceneGlobalMap);
			Register(new CutsceneController(tickBackground: true), Default);
			Register(new ElevatorController(), Default, Cutscene);
			Register(new SummonedUnitsController(), Default);
			Register(new UnitLineOfSightCacheController(), Default, Cutscene);
			Register(new FollowersFormationController(), Default);
			Register(new UnitFollowUnitController(), Default);
			Register(new UnitMoveController(), Default, SpaceCombat, StarSystem, Dialog, Cutscene, GlobalMap);
			Register(new ElevatorControllerLate(), Default, Dialog, Cutscene);
			Register(new UnitMoveControllerLate(), Default, SpaceCombat, StarSystem, Dialog, Cutscene, GlobalMap);
			Register(new MovePredictionController(), Default);
			Register(new ModePredictionInterpolationController(), Default);
			Register(new CustomUpdateBeforePhysicsController(), All);
			Register(new PhysicsSimulationController(), All);
			Register(new DetectiveServoskullController(), Default);
			Register(new DetectiveServoskullFlashlightController(), Default);
			Register(new CameraFollowController(), Default, SpaceCombat);
			Register(new EntityBoundsController(), Default, SpaceCombat, StarSystem, Dialog, Pause, Cutscene);
			Register(new FogOfWarRevealerTriggerController(), All);
			Register(new FogOfWarBlockerController(), Default, SpaceCombat, StarSystem, Dialog, Pause, Cutscene, GlobalMap, CutsceneGlobalMap);
			Register(new FogOfWarScheduleController(), Default, SpaceCombat, StarSystem, Dialog, Pause, Cutscene, GlobalMap, CutsceneGlobalMap);
			Register(new PartyAwarenessController(), Default);
			Register(new EntityVisibilityForPlayerController(), Default, SpaceCombat, StarSystem, Dialog, Pause, Cutscene, GlobalMap);
			Register(new UnitMimicController(), Default);
			Register(new EncounterController(), Default);
			Register(new SelectionCharacterController(), Default, SpaceCombat, StarSystem, Dialog, Pause, Cutscene, GlobalMap, CutsceneGlobalMap, GameOver, BugReport);
			Register(new UnitGuardController(), Default);
			Register(new UnitMemoryController(), Default, SpaceCombat, StarSystem);
			Register(new BehaviourTreeTickController(), Default);
			Register(new AttackOfOpportunityController(), Default);
			Register(new SynchronizedDataController(), All);
			Register(new GamepadInputController(), All);
			Register(new UnitCommandBuffer(), All);
			Register(new NetSendController(), All);
			Register(new UnitCommandController(), Default, SpaceCombat, StarSystem, Dialog, Cutscene);
			Register(new GroupCommandsController(), Default, Dialog, StarSystem, Cutscene);
			Register(new BuffsController(), Default, SpaceCombat, StarSystem, Dialog, GlobalMap);
			Register(new UnitHandEquipmentController(), Default, SpaceCombat, StarSystem, Dialog, Pause, Cutscene);
			Register(new UnitRoamingController(), Default, SpaceCombat, Dialog, Cutscene);
			Register(new UnitsProximityController(), Default, Cutscene);
			Register(new ScriptZoneController(), Default, Dialog, StarSystem, Cutscene);
			Register(new AreaEffectsController(), Default, SpaceCombat, Dialog);
			Register(new ProjectileController(), Default, SpaceCombat, StarSystem, Dialog, Cutscene);
			Register(new ProjectileSpawnerController(), Default, SpaceCombat, StarSystem, Dialog, Cutscene);
			Register(new ProjectileHitController(), Default, SpaceCombat, StarSystem, Dialog, Cutscene);
			Register(new AbilityExecutionController(), Default, SpaceCombat, StarSystem, Dialog, Cutscene);
			Register(new PsychicPhenomenaController(), Default, Dialog, Cutscene);
			Register(new DialogController(), Dialog);
			Register(new UnitLifeController(), Default, SpaceCombat, StarSystem, Dialog, Cutscene, GlobalMap, CutsceneGlobalMap);
			Register(new UnitReturnToConsciousController(), Default, Dialog, Cutscene, GlobalMap, CutsceneGlobalMap);
			Register(new OutOfCombatHealController(), All.Except(Pause, Cutscene, CutsceneGlobalMap, GameOver));
			Register(new UnitAnimationController(), Default, SpaceCombat, StarSystem, Dialog, Cutscene);
			Register(new FootprintsController(), Default, Dialog, Cutscene);
			Register(new UnitForceMoveController(), Default);
			Register(new UnitJumpMoveController(), Default);
			Register(new UpdatePreviousPositionController(), Default, SpaceCombat, StarSystem, Dialog, Cutscene, GlobalMap);
			Register(new CameraController(), Default, SpaceCombat, Pause);
			Register(new CameraController(allowScroll: false), Dialog);
			Register(new CameraController(allowScroll: false, allowZoom: false, clamp: false, rotate: false), Cutscene);
			Register(new GameOverController(), Default, SpaceCombat, StarSystem, GlobalMap);
			Register(new InspectUnitsController(), Default, Dialog, Cutscene);
			Register(new EtudeSystemController(), Default, SpaceCombat, StarSystem, Dialog, Cutscene, GlobalMap, CutsceneGlobalMap);
			Register(new OffensiveActionsController(), Default);
			Register(new GameOverIronmanController(), GameOver);
			Register(new TutorialController(), Default, SpaceCombat, Dialog, Pause, Cutscene, StarSystem, GlobalMap);
			Register(new CustomUpdateController(), Default, SpaceCombat, Pause, Cutscene, StarSystem, Dialog);
			Register(new BirdUpdateController(), Default, Pause, Cutscene, StarSystem, Dialog);
			Register(new TurnController(), Default, SpaceCombat);
			Register(new AutoEndTurnController(), Default, SpaceCombat);
			Register(new CombatStateSwitchController(), Default, SpaceCombat);
			Register(new UnitMovableAreaController(), Default);
			Register(new OverwatchController(), Default, SpaceCombat);
			Register(new AutoPauseController(), Default);
			Register(new TimerController(), Default);
			Register(new BarkBanterController(), Default, StarSystem);
			Register(new BarkController(), Default);
			Register(new InteractionGlobalCooldownController(), Default, Dialog, Pause, Cutscene);
			Register(new DopplerSoundController(), Default, SpaceCombat);
			Register(new PreciseAttackController(), Default);
			Register(new VisualEffectsController(), All.Except(None, BugReport));
			Register(new OccluderClipController(), Default, Dialog, Cutscene, Pause);
			Register(new GameLogController(), All);
			Register(new EntitySpawnController(), Default, SpaceCombat, StarSystem, Dialog, Cutscene, GlobalMap);
			Register(new EntityDestructionController(), Default, SpaceCombat, StarSystem, Dialog, Cutscene, GlobalMap);
			Register(new GarbageEntityController(), All);
			Register(new VirtualPositionController(), All);
			Register(new EntitiesInCameraFrustumController(), Default, Dialog, Pause, Cutscene);
			Register(new SleepingUnitsController(), Default, SpaceCombat, StarSystem, Dialog, Pause, Cutscene, GlobalMap);
			Register(new GridNodeToEntityCacheController(), All);
			Register(new AnimationManagerController(), All);
			Register(new UpdateRigidbodyCreatureController(), All);
			Register(new UnitMovementController(), All);
			Register(new CustomCallbackController(), All);
			Register(new CoroutinesController(), All);
			Register(new StateSerializationController(), All);
			Register(new SyncStateCheckerController(), All);
			Register(new CustomLateUpdateController(), All);
			Register(new DefaultInterpolationController(), All);
			Register(new TimeSpeedController(), All);
			Register(new InteractionFXController(), All);
			Register(new InteractionNavigationController(), Default);
			Register(new FogOfWarCompleteController(), Default, SpaceCombat, StarSystem, Dialog, Pause, Cutscene, GlobalMap, CutsceneGlobalMap);
			Register(new ForcedCoversController(), All);
			Register(new LevelUpFxController(), Default);
			Register(new MoraleController(), Default);
			Register(new SceneControllablesController(), All);
			Register(new DetectiveRadarController(), Default);
			Register(new WarhammerPathCostModifier.InvalidateCacheController(), All);
			Register(new VoiceOverController(), All);
			Register(new ProximityAsksController(), Default);
			Register(new GPUSoundController(), All);
			Register(new PerformanceMetricsController(), All);
		}
	}

	private static void Register(IController controller, IEnumerable<GameModeType> gameModes)
	{
		Register(controller, gameModes.ToArray());
	}

	private static void Register(IController controller, params GameModeType[] gameModes)
	{
		GameModesFactory.Register(controller, gameModes);
	}

	public static void ResetControllers()
	{
		foreach (ControllerData allController in GameModesFactory.AllControllers)
		{
			if (allController.Controller is IControllerReset controllerReset)
			{
				try
				{
					controllerReset.OnReset();
				}
				catch (Exception ex)
				{
					PFLog.Default.Exception(ex);
				}
			}
		}
	}
}
