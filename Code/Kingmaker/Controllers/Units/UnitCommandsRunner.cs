using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Code.Visual.Animation;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.Code.Framework;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Formations;
using Kingmaker.GameCommands;
using Kingmaker.Interaction;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.UI.AR;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Pointer;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Controllers.Units;

public static class UnitCommandsRunner
{
	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("UnitCommandsRunner");

	private static readonly List<BaseUnitEntity> UnitWaitAgentList = new List<BaseUnitEntity>();

	private static VirtualMoveCommand s_VirtualMoveCommand;

	private static IDisposable s_EscManagerHandle;

	private static bool s_MovePreview;

	private static readonly List<BaseUnitEntity> PreviewSelectedUnits = new List<BaseUnitEntity>();

	private static readonly List<BaseUnitEntity> PreviewAllUnits = new List<BaseUnitEntity>();

	private static Vector3[] s_PreviewResultPositions = Array.Empty<Vector3>();

	private static NNConstraint s_ClosestWithAreaConstraint = new NNConstraint
	{
		constrainArea = true,
		constrainWalkability = true,
		walkable = true,
		distanceMetric = DistanceMetric.ClosestAsSeenFromAbove()
	};

	public static bool HasWaitingAgents => UnitWaitAgentList.Count > 0;

	public static void ClearWaitingAgents(bool delayed = false)
	{
		if (delayed)
		{
			UtilityTime.DoTaskLater(0.1f, delegate
			{
				UnitWaitAgentList.Clear();
			});
		}
		else
		{
			UnitWaitAgentList.Clear();
		}
	}

	public static void CancelMoveCommand()
	{
		Game.Instance.GameCommandQueue.ClearMovePrediction();
	}

	public static void CancelMoveCommandLocal()
	{
		UnitHelper.ClearPrediction();
		if (s_VirtualMoveCommand?.CmdParams != null)
		{
			s_VirtualMoveCommand.CmdParams.ForcedPath = null;
		}
		s_VirtualMoveCommand = null;
		TryUnsubscribeEscManager();
	}

	public static void SetVirtualMoveCommand([NotNull] BaseUnitEntity unit, [NotNull] UnitCommandParams cmdParams)
	{
		CombatSounds.Instance.Combat.CombatGridSetWaypointClick.Play();
		if (s_VirtualMoveCommand?.CmdParams != null)
		{
			s_VirtualMoveCommand.CmdParams.ForcedPath = null;
		}
		s_VirtualMoveCommand = new VirtualMoveCommand(cmdParams, unit);
		if (!unit.ToBaseUnitEntity().IsDirectlyControllable())
		{
			TryUnsubscribeEscManager();
		}
		else
		{
			SubscribeEscManager();
		}
	}

	public static void DirectInteract(BaseUnitEntity unit, AbstractInteractionPart interaction)
	{
		unit.Commands.Run(UnitDirectInteract.CreateCommandParams(interaction));
	}

	public static void TryApproachAndInteract(BaseUnitEntity unit, AbstractInteractionPart interaction, IInteractionVariantActor variantActor = null)
	{
		if (unit == null || !interaction.HasEnoughActionPoints(unit))
		{
			return;
		}
		if (!Game.Instance.Controllers.TurnController.TurnBasedModeActive)
		{
			float approachRadiousMeters = interaction.ApproachRadius.Cells().Meters;
			PathfindingService.Instance.FindPathRT(unit.MovementAgent, interaction.Owner.Position, approachRadiousMeters, delegate(ForcedPath path)
			{
				if (path.error)
				{
					PFLog.Pathfinding.Error("An error path was returned. Ignoring");
				}
				else
				{
					if (!unit.IsMovementLockedByGameModeOrCombat())
					{
						if (path.vectorPath.Count > 2 || (path.vectorPath[1] - path.vectorPath[0]).sqrMagnitude > 1f || (path.vectorPath[0] - interaction.Owner.Position).magnitude > approachRadiousMeters - 1f)
						{
							UnitMoveToParams unitMoveToParams = new UnitMoveToParams(path, interaction.Owner.Position, approachRadiousMeters)
							{
								IsSynchronized = true
							};
							if (unit.IsInPlayerParty && !unit.IsInCombat)
							{
								if ((interaction.Owner.Position - unit.Position).magnitude > (float)ConfigRoot.Instance.SystemMechanics.MinSprintDistance)
								{
									unitMoveToParams.MovementType = WalkSpeedType.Sprint;
								}
								else if ((interaction.Owner.Position - unit.Position).magnitude < (float)ConfigRoot.Instance.SystemMechanics.MaxWalkDistance)
								{
									unitMoveToParams.MovementType = WalkSpeedType.Walk;
								}
								else
								{
									unitMoveToParams.MovementType = WalkSpeedType.Run;
								}
							}
							RunMoveCommand(unit, unitMoveToParams);
						}
						unit.Commands.AddToQueue(new UnitInteractWithObjectParams(interaction, variantActor)
						{
							IsSynchronized = true
						});
						{
							foreach (BaseUnitEntity selectedUnit in Game.Instance.Controllers.SelectionCharacter.SelectedUnits)
							{
								if (unit != selectedUnit)
								{
									selectedUnit.Commands.InterruptMove(byPlayer: true);
								}
							}
							return;
						}
					}
					PFLog.Pathfinding.Log("Movement is locked due to GameMode or Combat. Ignoring");
				}
			}, new InteractionCustomDistanceCheck(unit, interaction));
		}
		else if (interaction.IsEnoughCloseForInteractionFromDesiredPosition(unit))
		{
			TryRunVirtualMoveCommand();
			unit.Commands.AddToQueue(new UnitInteractWithObjectParams(interaction)
			{
				IsSynchronized = true
			});
		}
		else
		{
			MoveSelectedUnitsToPoint(interaction.Owner.Position);
		}
	}

	public static void TryUnitUseAbility(AbilityData abilityData, TargetWrapper target, bool shouldApproach = false)
	{
		MechanicEntity caster2 = abilityData.Caster;
		BaseUnitEntity caster = caster2 as BaseUnitEntity;
		if (caster == null)
		{
			return;
		}
		TryRunVirtualMoveCommand();
		UnitCommandParams cmd = CreateUseAbilityCommandParams(abilityData, target);
		if (cmd != null)
		{
			PartUnitCommands commands = caster.GetCommandsOptional();
			if (commands != null)
			{
				if (shouldApproach)
				{
					PathfindingService.Instance.FindPathRT(caster.MovementAgent, target.Entity.Position, abilityData.RangeCells.Cells().Meters, delegate(ForcedPath path)
					{
						if (path.error)
						{
							PFLog.Pathfinding.Error("An error path was returned. Ignoring");
						}
						else if (caster.IsMovementLockedByGameModeOrCombat())
						{
							PFLog.Pathfinding.Log("Movement is locked due to GameMode or Combat. Ignoring");
						}
						else
						{
							UnitMoveToParams cmdParams = new UnitMoveToParams(path, target.Entity.Position, abilityData.RangeCells.Cells().Meters)
							{
								IsSynchronized = true
							};
							commands.AddToQueue(cmdParams);
							commands.AddToQueue(cmd);
						}
					});
				}
				else
				{
					commands.AddToQueue(cmd);
				}
				return;
			}
		}
		PFLog.Default.ErrorWithReport($"{abilityData.Caster} can't execute cast command");
	}

	public static void TryUnitToggleAbility(BaseUnitEntity caster, ToggleAbility ability)
	{
		PartUnitCommands commandsOptional = caster.GetCommandsOptional();
		if (commandsOptional != null)
		{
			commandsOptional.AddToQueue(new UnitToggleAbilityParams(ability.Blueprint)
			{
				IsSynchronized = true
			});
		}
		else
		{
			PFLog.Default.ErrorWithReport($"{caster} can't execute toggle ability command");
		}
	}

	public static void MoveSelectedUnitsToPoint(Vector3 worldPosition)
	{
		if (Game.Instance.Controllers.TurnController.TurnBasedModeActive)
		{
			MoveSelectedUnitToPointTB(worldPosition);
		}
		else
		{
			MoveSelectedUnitsToPointRT(worldPosition, ClickGroundHandler.GetDefaultDirection(worldPosition), isControllerGamepad: false);
		}
	}

	private static void MoveSelectedUnitToPointTB(Vector3 worldPosition)
	{
		GridNodeBase gridNodeBase = UnitPathManager.Instance?.CurrentNode ?? worldPosition.GetNearestNodeXZUnwalkable();
		if (gridNodeBase == null || !gridNodeBase.Walkable)
		{
			Logger.Log("Cant move to coord {0}, node {1}", worldPosition, gridNodeBase?.ToString() ?? "<null>");
			return;
		}
		if (!Game.Instance.Controllers.TurnController.TurnBasedModeActive)
		{
			throw new InvalidOperationException("Expecting to be in TBM mode here");
		}
		if (Game.Instance.Controllers.SelectionCharacter.SelectedUnits.Count != 1)
		{
			throw new InvalidOperationException(string.Format("Expecting only one selected unit, got #{0}: {1}", Game.Instance.Controllers.SelectionCharacter.SelectedUnits.Count, string.Join(", ", Game.Instance.Controllers.SelectionCharacter.SelectedUnits.Select((BaseUnitEntity v) => v.ToString()))));
		}
		BaseUnitEntity baseUnitEntity = Game.Instance.Controllers.SelectionCharacter.SelectedUnits.Single();
		if (baseUnitEntity != Game.Instance.Controllers.TurnController.CurrentUnit)
		{
			throw new InvalidOperationException($"Cant move unit {baseUnitEntity} on {Game.Instance.Controllers.TurnController.CurrentUnit}'s turn");
		}
		if (!baseUnitEntity.Commands.Empty)
		{
			Logger.Log("Unit {0} has active command {1}, cant move.", baseUnitEntity, baseUnitEntity.Commands.Current);
			return;
		}
		if (!WarhammerBlockManager.Instance.CanUnitStandOnNode(baseUnitEntity, gridNodeBase))
		{
			Logger.Log("Unit {0} can't stand on the node {1}", baseUnitEntity, gridNodeBase);
			return;
		}
		baseUnitEntity.TryCreateMoveCommandTB(new MoveCommandSettings
		{
			Destination = gridNodeBase.Vector3Position()
		}, showMovePrediction: true, out var status);
		switch (status)
		{
		case UnitHelper.MoveCommandStatus.SamePath:
			CombatSounds.Instance.Combat.CombatGridConfirmActionClick.Play();
			TryRunVirtualMoveCommand();
			break;
		case UnitHelper.MoveCommandStatus.NotEnoughPoints:
			Logger.Log("Move command: Not enough points to move");
			break;
		case UnitHelper.MoveCommandStatus.NoReachableTile:
			Logger.Log("Move command: No reachable tiles available");
			break;
		case UnitHelper.MoveCommandStatus.NoForcedPath:
			Logger.Log("Move command: Failed to create Forced path");
			break;
		case UnitHelper.MoveCommandStatus.NoStartingCell:
			Logger.Log("Move command: No starting cell in Forced path available");
			break;
		case UnitHelper.MoveCommandStatus.CannotMove:
			Logger.Log("Move command: Cannot move");
			break;
		default:
			Logger.Warning("Unknown move command status occured: {0}", status);
			break;
		case UnitHelper.MoveCommandStatus.NewCommandCreated:
			break;
		}
	}

	public static void MoveSelectedUnitsToPointRT(Vector3 worldPosition, Vector3 direction, bool isControllerGamepad, bool preview = false, float formationSpaceFactor = 1f, List<BaseUnitEntity> selectedUnits = null, Action<BaseUnitEntity, MoveCommandSettings> commandRunner = null)
	{
		MoveSelectedUnitsToPointRT(Game.Instance.Controllers.SelectionCharacter.SingleSelectedUnit.Value, worldPosition, direction, isControllerGamepad, preview, formationSpaceFactor, selectedUnits, commandRunner);
	}

	public static void MoveSelectedUnitsToPointRT(BaseUnitEntity mainUnit, Vector3 worldPosition, Vector3 direction, bool isControllerGamepad, bool preview = false, float formationSpaceFactor = 1f, List<BaseUnitEntity> selectedUnits = null, Action<BaseUnitEntity, MoveCommandSettings> commandRunner = null, List<BaseUnitEntity> allUnits = null)
	{
		using (ProfileScope.New(preview ? "UnitCommandsRunner.MoveSelectedUnitsToPointRT.Preview" : "UnitCommandsRunner.MoveSelectedUnitsToPointRT"))
		{
			if (Game.Instance.Controllers.TurnController.TurnBasedModeActive)
			{
				throw new InvalidOperationException("Not expecting to be in TBM mode here");
			}
			s_MovePreview = preview;
			if (!preview)
			{
				UnitWaitAgentList.Clear();
			}
			bool valueOrDefault = (mainUnit?.MaybeMovementAgent?.IsDirectionalMovementActive).GetValueOrDefault();
			IPartyFormation currentFormation = Game.Instance.Player.FormationManager.CurrentFormation;
			if (preview)
			{
				if (selectedUnits == null)
				{
					PreviewSelectedUnits.Clear();
					foreach (BaseUnitEntity selectedUnit in Game.Instance.Controllers.SelectionCharacter.SelectedUnits)
					{
						PreviewSelectedUnits.Add(selectedUnit);
					}
					selectedUnits = PreviewSelectedUnits;
				}
				if (allUnits == null)
				{
					if (selectedUnits.Count == 1)
					{
						allUnits = selectedUnits;
					}
					else
					{
						PreviewAllUnits.Clear();
						foreach (BaseUnitEntity partyAndPet in Game.Instance.Player.PartyAndPets)
						{
							if (partyAndPet.IsDirectlyControllable())
							{
								PreviewAllUnits.Add(partyAndPet);
							}
						}
						allUnits = PreviewAllUnits;
					}
				}
			}
			else
			{
				if (selectedUnits == null)
				{
					selectedUnits = Game.Instance.Controllers.SelectionCharacter.SelectedUnits.ToList();
				}
				if (allUnits == null)
				{
					allUnits = ((selectedUnits.Count == 1) ? selectedUnits : Game.Instance.Player.PartyAndPets.Where((BaseUnitEntity c) => c.IsDirectlyControllable()).ToList());
				}
			}
			if (selectedUnits.Count <= 0)
			{
				return;
			}
			Vector3[] array;
			if (preview)
			{
				if (s_PreviewResultPositions.Length < allUnits.Count)
				{
					s_PreviewResultPositions = new Vector3[allUnits.Count];
				}
				array = s_PreviewResultPositions;
			}
			else
			{
				array = new Vector3[allUnits.Count];
			}
			using (ProfileScope.New("UnitCommandsRunner.FillFormationPositions"))
			{
				PartyFormationHelper.FillFormationPositions(worldPosition, FormationAnchor.Front, direction, allUnits, selectedUnits, currentFormation, array, formationSpaceFactor);
			}
			if (preview)
			{
				using (ProfileScope.New("UnitCommandsRunner.PreviewMarkers"))
				{
					for (int i = 0; i < allUnits.Count; i++)
					{
						BaseUnitEntity baseUnitEntity = allUnits[i];
						if (selectedUnits.HasItem(baseUnitEntity))
						{
							Vector3 pathDestination = SizePathfindingHelper.FromViewToMechanicsPosition(baseUnitEntity, array[i]);
							baseUnitEntity.View?.OnMovementStarted(pathDestination, preview: true);
						}
					}
				}
			}
			else
			{
				CoroutineRunner.Start(MoveUnitsToDestinationConsistently(mainUnit, isControllerGamepad, preview, selectedUnits, commandRunner, allUnits, valueOrDefault, array));
			}
			using (ProfileScope.New("UnitCommandsRunner.CrossSceneUnits"))
			{
				float num = 0f;
				for (int j = 0; j < allUnits.Count; j++)
				{
					if (selectedUnits.HasItem(allUnits[j]))
					{
						float magnitude = (worldPosition - array[j]).To2D().magnitude;
						if (magnitude > num)
						{
							num = magnitude;
						}
					}
				}
				foreach (BaseUnitEntity selectedUnit2 in selectedUnits)
				{
					if (allUnits.HasItem(selectedUnit2))
					{
						continue;
					}
					if (isControllerGamepad && selectedUnit2 == mainUnit && valueOrDefault)
					{
						commandRunner?.Invoke(selectedUnit2, new MoveCommandSettings
						{
							Destination = selectedUnit2.Position
						});
						continue;
					}
					Vector3 vector = ((selectedUnits.Count == 1) ? worldPosition : GeometryUtils.ProjectToGround(worldPosition - direction.normalized * (num + 2f)));
					if (preview)
					{
						Vector3 pathDestination2 = SizePathfindingHelper.FromViewToMechanicsPosition(selectedUnit2, vector);
						selectedUnit2.View?.OnMovementStarted(pathDestination2, preview: true);
					}
					else
					{
						(commandRunner ?? new Action<BaseUnitEntity, MoveCommandSettings>(RunMoveCommandRT))(selectedUnit2, new MoveCommandSettings
						{
							Destination = vector
						});
					}
				}
			}
			if (preview)
			{
				using (ProfileScope.New("UnitCommandsRunner.ShowPreviewArrow"))
				{
					ClickPointerManager.Instance?.ShowPreviewArrow(worldPosition, direction);
					return;
				}
			}
			ClickPointerManager.Instance?.CancelPreview();
			EventBus.RaiseEvent(delegate(IClickActionHandler h)
			{
				h.OnMoveRequested(worldPosition);
			});
		}
	}

	private static IEnumerator MoveUnitsToDestinationConsistently(BaseUnitEntity mainUnit, bool isControllerGamepad, bool preview, List<BaseUnitEntity> selectedUnits, Action<BaseUnitEntity, MoveCommandSettings> commandRunner, List<BaseUnitEntity> allUnits, bool mainUnitMovingDirectly, Vector3[] resultPositions)
	{
		for (int i = 0; i < allUnits.Count; i++)
		{
			BaseUnitEntity baseUnitEntity = allUnits[i];
			if (isControllerGamepad && baseUnitEntity == mainUnit && mainUnitMovingDirectly)
			{
				commandRunner?.Invoke(baseUnitEntity, new MoveCommandSettings
				{
					Destination = baseUnitEntity.Position
				});
			}
			else if (selectedUnits.HasItem(baseUnitEntity))
			{
				Vector3 vector = SizePathfindingHelper.FromViewToMechanicsPosition(baseUnitEntity, resultPositions[i]);
				if (preview)
				{
					ShowDestination(baseUnitEntity, vector);
				}
				else
				{
					(commandRunner ?? new Action<BaseUnitEntity, MoveCommandSettings>(RunMoveCommandRT))(baseUnitEntity, new MoveCommandSettings
					{
						Destination = vector,
						FollowedUnit = mainUnit,
						IsControllerGamepad = isControllerGamepad
					});
				}
				yield return new WaitForSeconds(0.05f);
			}
		}
	}

	public static void ShowDestination(BaseUnitEntity unit, Vector3 point)
	{
		if (unit.GetSaddledUnit() != null || !unit.View.MovementAgent || UnitWaitAgentList.HasItem(unit))
		{
			return;
		}
		UnitWaitAgentList.Add(unit);
		PathfindingService.Instance.FindPathRT(unit.MovementAgent, point, 0.3f, delegate(ForcedPath p)
		{
			if (p.error)
			{
				UnitWaitAgentList.Remove(unit);
				PFLog.Pathfinding.Error("An error path was returned. Ignoring");
				return;
			}
			using (PathDisposable<ForcedPath>.Get(p, unit))
			{
				UnitWaitAgentList.Remove(unit);
				if (p.vectorPath.Count > 0)
				{
					List<Vector3> vectorPath = p.vectorPath;
					Vector3 pathDestination = vectorPath[vectorPath.Count - 1];
					unit.View.OnMovementStarted(pathDestination, s_MovePreview);
				}
			}
		});
	}

	public static void ShowDestination(BaseUnitEntity unit, Path path)
	{
		if (unit.GetSaddledUnit() == null && path.vectorPath.Count > 0)
		{
			List<Vector3> vectorPath = path.vectorPath;
			Vector3 pathDestination = vectorPath[vectorPath.Count - 1];
			unit.View.OnMovementStarted(pathDestination, s_MovePreview);
		}
	}

	private static UnitCommandParams CreateUseAbilityCommandParams(AbilityData abilityData, TargetWrapper target)
	{
		PlayerUseAbilityParams result = new PlayerUseAbilityParams(abilityData, target)
		{
			IsSynchronized = true
		};
		if (abilityData.SourceItem != null)
		{
			EventBus.RaiseEvent(delegate(IClickActionHandler h)
			{
				h.OnItemUseRequested(abilityData, target);
			});
			return result;
		}
		EventBus.RaiseEvent(delegate(IClickActionHandler h)
		{
			h.OnCastRequested(abilityData, target);
		});
		return result;
	}

	private static void RunMoveCommand(BaseUnitEntity unit, UnitCommandParams cmdParams)
	{
		if (unit.Commands.Current is UnitMoveTo)
		{
			UnitMovementAgent maybeMovementAgent = unit.MaybeMovementAgent;
			if ((object)maybeMovementAgent != null && maybeMovementAgent.IsTraverseInProgress)
			{
				cmdParams.PreprocessingFlags = CommandPreprocessingFlags.SoftInterruptAll;
				unit.Commands.AddToQueueFirst(cmdParams);
				goto IL_0047;
			}
		}
		unit.Commands.Run(cmdParams);
		goto IL_0047;
		IL_0047:
		if (cmdParams.ForcedPath != null && cmdParams.ForcedPath.vectorPath.Count > 0)
		{
			IUnitEntityView view = unit.View;
			List<Vector3> vectorPath = cmdParams.ForcedPath.vectorPath;
			view.TryShowPointer(vectorPath[vectorPath.Count - 1]);
		}
		else if (cmdParams.Target != null)
		{
			unit.View.TryShowPointer(cmdParams.Target.Point);
		}
		if (unit.Commands.Queue.FirstOrDefault() == cmdParams || Game.Instance.IsPaused)
		{
			ShowDestination(unit, cmdParams.Target.Point);
		}
	}

	private static void RunMoveCommandRT(BaseUnitEntity unit, MoveCommandSettings settings)
	{
		if (Game.Instance.Controllers.TurnController.TurnBasedModeActive)
		{
			throw new InvalidOperationException("Should not be here in TBM");
		}
		if (settings.IsControllerGamepad && unit != settings.FollowedUnit)
		{
			UnitCommandParams cmdParams = UnitHelper.CreateUnitFollowCommandParamsRT(unit, settings);
			RunMoveCommand(unit, cmdParams);
			return;
		}
		s_ClosestWithAreaConstraint.area = (int)unit.CurrentNode.node.Area;
		NNInfo nearest = AstarPath.active.data.gridGraph.GetNearest(settings.Destination, s_ClosestWithAreaConstraint);
		settings.Destination = UnitMovementAgent.PushAwayFromBorders(nearest.position, nearest.node as GridNodeBase, unit.Corpulence);
		unit.View.TryShowPointer(settings.Destination);
		PathfindingService.Instance.FindPathRT(unit.MovementAgent, settings.Destination, 0.3f, delegate(ForcedPath path)
		{
			if (path.error)
			{
				PFLog.Pathfinding.Error("An error path was returned. Ignoring");
			}
			else if (unit.IsMovementLockedByGameModeOrCombat())
			{
				PFLog.Pathfinding.Log("Movement is locked due to GameMode or Combat. Ignoring");
			}
			else
			{
				UnitCommandParams unitCommandParams = UnitHelper.CreateMoveCommandParamsRT(unit, settings, path);
				if (unitCommandParams != null)
				{
					RunMoveCommand(unit, unitCommandParams);
				}
			}
		});
	}

	private static void TryRunVirtualMoveCommand()
	{
		if (s_VirtualMoveCommand != null)
		{
			TryUnsubscribeEscManager();
			UnitHelper.ClearPrediction(s_VirtualMoveCommand.Unit.ToBaseUnitEntity());
			AbstractUnitCommand cmd = s_VirtualMoveCommand.CmdHandle?.Cmd;
			UnitReference unit = s_VirtualMoveCommand.Unit;
			s_VirtualMoveCommand.RunCommand();
			s_VirtualMoveCommand = null;
			EventBus.RaiseEvent(delegate(IRunVirtualMoveCommandHandler h)
			{
				h.HandleRunVirtualMoveCommand(cmd, unit);
			});
		}
	}

	private static void SubscribeEscManager()
	{
		TryUnsubscribeEscManager();
		s_EscManagerHandle = EscHotkeyManager.Instance.Subscribe(CancelMoveCommand);
	}

	private static void TryUnsubscribeEscManager()
	{
		s_EscManagerHandle?.Dispose();
	}

	public static void HandleRoleSet(string entityId)
	{
		VirtualMoveCommand virtualMoveCommand = s_VirtualMoveCommand;
		if (virtualMoveCommand == null || virtualMoveCommand.Unit.Id.Equals(entityId, StringComparison.Ordinal))
		{
			CancelMoveCommand();
		}
	}
}
