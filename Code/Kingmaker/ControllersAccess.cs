using Kingmaker.AreaLogic.SceneControllables;
using Kingmaker.Code.Controllers.Interactions;
using Kingmaker.Code.Gameplay.Controllers;
using Kingmaker.Code.Gameplay.Controllers.Asks;
using Kingmaker.Code.Gameplay.Controllers.Combat;
using Kingmaker.Code.Gameplay.Controllers.DetectiveRadar;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Clicks;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.Dialog;
using Kingmaker.Controllers.MapObjects;
using Kingmaker.Controllers.MovePrediction;
using Kingmaker.Controllers.Net;
using Kingmaker.Controllers.Optimization;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.Controllers.Rest;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Controllers.Units;
using Kingmaker.Controllers.UnityEventsReplacements;
using Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;
using Kingmaker.UI.Sound;
using Owlcat.AI;

namespace Kingmaker;

public class ControllersAccess
{
	public GamepadInputController GamepadInputController => Game.Instance.GetController<GamepadInputController>();

	public UnitGroupsController UnitGroupsController => Game.Instance.GetController<UnitGroupsController>();

	public TurnController TurnController => Game.Instance.GetController<TurnController>();

	public PauseController PauseController => Game.Instance.GetController<PauseController>();

	public TimeController TimeController => Game.Instance.GetController<TimeController>();

	public TimeSpeedController TimeSpeedController => Game.Instance.GetController<TimeSpeedController>();

	public EntitySpawnController EntitySpawner => Game.Instance.GetController<EntitySpawnController>();

	public EntityDestructionController EntityDestroyer => Game.Instance.GetController<EntityDestructionController>();

	public UnitMemoryController UnitMemoryController => Game.Instance.GetController<UnitMemoryController>();

	public UnitCommandBuffer UnitCommandBuffer => Game.Instance.GetController<UnitCommandBuffer>();

	public GroupCommandsController GroupCommands => Game.Instance.GetController<GroupCommandsController>();

	public DialogController DialogController => Game.Instance.GetController<DialogController>();

	public RealtimeLightsController LightsController => Game.Instance.GetController<RealtimeLightsController>();

	public AbilityExecutionController AbilityExecutor => Game.Instance.GetController<AbilityExecutionController>();

	public FogOfWarScheduleController FogOfWar => Game.Instance.GetController<FogOfWarScheduleController>();

	public FogOfWarCompleteController FogOfWarComplete => Game.Instance.GetController<FogOfWarCompleteController>();

	public EntityVisibilityForPlayerController EntityVisibilityForPlayerController => Game.Instance.GetController<EntityVisibilityForPlayerController>();

	public FollowersFormationController FollowersFormationController => Game.Instance.GetController<FollowersFormationController>();

	public ProjectileSpawnerController ProjectileSpawnerController => Game.Instance.GetController<ProjectileSpawnerController>();

	public DirectorAdapterController DirectorAdapterController => Game.Instance.GetController<DirectorAdapterController>();

	public CustomUpdateBeforePhysicsController CustomUpdateBeforePhysicsController => Game.Instance.GetController<CustomUpdateBeforePhysicsController>();

	public CustomUpdateController CustomUpdateController => Game.Instance.GetController<CustomUpdateController>();

	public BirdUpdateController BirdUpdateController => Game.Instance.GetController<BirdUpdateController>();

	public FogOfWarRevealerTriggerController FogOfWarRevealerTriggerController => Game.Instance.GetController<FogOfWarRevealerTriggerController>();

	public DoorUpdateController DoorUpdateController => Game.Instance.GetController<DoorUpdateController>();

	public EntityBoundsController EntityBoundsController => Game.Instance.GetController<EntityBoundsController>();

	public SelectionCharacterController SelectionCharacter => Game.Instance.GetController<SelectionCharacterController>();

	public BehaviourTreeTickController BehaviourTreeTickController => Game.Instance.GetController<BehaviourTreeTickController>();

	public AnimationManagerController AnimationManagerController => Game.Instance.GetController<AnimationManagerController>();

	public UpdateRigidbodyCreatureController UpdateRigidbodyCreatureController => Game.Instance.GetController<UpdateRigidbodyCreatureController>();

	public CustomCallbackController CustomCallbackController => Game.Instance.GetController<CustomCallbackController>();

	public AttackOfOpportunityController AttackOfOpportunityController => Game.Instance.GetController<AttackOfOpportunityController>();

	public CustomLateUpdateController CustomLateUpdateController => Game.Instance.GetController<CustomLateUpdateController>();

	public DefaultInterpolationController InterpolationController => Game.Instance.GetController<DefaultInterpolationController>();

	public CoroutinesController CoroutinesController => Game.Instance.GetController<CoroutinesController>();

	public SynchronizedDataController SynchronizedDataController => Game.Instance.GetController<SynchronizedDataController>();

	public SyncStateCheckerController SyncStateCheckerController => Game.Instance.GetController<SyncStateCheckerController>();

	public InteractionFXController InteractionFXController => Game.Instance.GetController<InteractionFXController>();

	public UnitMovableAreaController UnitMovableAreaController => Game.Instance.GetController<UnitMovableAreaController>();

	public GridNodeToEntityCacheController EntityToGridNodeCacheController => Game.Instance.GetController<GridNodeToEntityCacheController>();

	public ForcedCoversController ForcedCoversController => Game.Instance.GetController<ForcedCoversController>();

	public SceneControllablesController SceneControllables => Game.Instance.GetController<SceneControllablesController>();

	public MovePredictionController MovePredictionController => Game.Instance.GetController<MovePredictionController>();

	public PointerController PointerController => Game.Instance.GetController<PointerController>();

	public VirtualPositionController VirtualPositionController => Game.Instance.GetController<VirtualPositionController>();

	public PointerController ClickEventsController => Game.Instance.GetController<PointerController>();

	public CameraController CameraController => Game.Instance.GetController<CameraController>();

	public InteractionHighlightController InteractionHighlightController => Game.Instance.GetController<InteractionHighlightController>();

	public UnitHandEquipmentController HandsEquipmentController => Game.Instance.GetController<UnitHandEquipmentController>();

	public ProjectileController ProjectileController => Game.Instance.GetController<ProjectileController>();

	public ClickWithSelectedAbilityHandler SelectedAbilityHandler => PointerController.GetHandler<ClickWithSelectedAbilityHandler>();

	public DetectiveRadarController DetectiveRadarController => Game.Instance.GetController<DetectiveRadarController>();

	public PreciseAttackController PreciseAttackController => Game.Instance.GetController<PreciseAttackController>();

	public MoraleController MoraleController => Game.Instance.GetController<MoraleController>();

	public VoiceOverController VoiceOverController => Game.Instance.GetController<VoiceOverController>();

	public ProximityAsksController ProximityAsksController => Game.Instance.GetController<ProximityAsksController>();

	public GPUSoundController GPUSoundController => Game.Instance.GetController<GPUSoundController>();

	public BarkController BarkController => Game.Instance.GetController<BarkController>();

	public PartyAwarenessController PartyAwarenessController => Game.Instance.GetController<PartyAwarenessController>();

	public DetectiveServoskullFlashlightController ServoskullFlashlightController => Game.Instance.GetController<DetectiveServoskullFlashlightController>();
}
