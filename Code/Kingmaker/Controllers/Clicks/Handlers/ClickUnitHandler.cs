using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.Facades;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.Inspect;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Networking;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Settings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Interaction;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Controllers.Clicks.Handlers;

public sealed class ClickUnitHandler : IClickEventHandler
{
	public PointerMode GetMode()
	{
		return PointerMode.Default;
	}

	public HandlerPriorityResult GetPriority(GameObject gameObject, Vector3 worldPosition)
	{
		return new HandlerPriorityResult(GetPriorityInternal(gameObject, worldPosition));
	}

	private float GetPriorityInternal(GameObject gameObject, Vector3 worldPosition)
	{
		if (!gameObject)
		{
			return 0f;
		}
		if (TurnController.IsInTurnBasedCombat() && Game.Instance.Controllers.TurnController.IsPreparationTurn)
		{
			return 0f;
		}
		AbstractUnitEntityView componentNonAlloc = gameObject.GetComponentNonAlloc<AbstractUnitEntityView>();
		if (componentNonAlloc.Or(null)?.Data == null)
		{
			return 0f;
		}
		if (!componentNonAlloc.EntityData.IsVisibleForPlayer)
		{
			return 0f;
		}
		if ((bool)componentNonAlloc.EntityData.Features.IsUntargetable)
		{
			return 0f;
		}
		UnitPartPersonalEnemy optional = componentNonAlloc.EntityData.GetOptional<UnitPartPersonalEnemy>();
		if (optional != null && !optional.IsCurrentlyTargetable)
		{
			return 0f;
		}
		if (componentNonAlloc.EntityData.LifeState.IsDead)
		{
			if (Game.Instance.Player.IsInCombat)
			{
				return 0f;
			}
			if (!componentNonAlloc.EntityData.IsDeadAndHasLoot && (!Game.Instance.Player.UISettings.ShowInspect || Game.Instance.Player.IsInCombat))
			{
				return 0f;
			}
			return 0.9f;
		}
		if (!Game.Instance.Player.IsInCombat)
		{
			return 1f;
		}
		if (componentNonAlloc.EntityData.IsDirectlyControllable)
		{
			return 2f;
		}
		if (componentNonAlloc.EntityData.IsEnemy(Game.Instance.Player.MainCharacterEntity))
		{
			return 2f;
		}
		return 1f;
	}

	public bool OnClick(GameObject gameObject, Vector3 worldPosition, int button, bool simulate = false, bool muteEvents = false)
	{
		AbstractUnitEntityView targetUnit = gameObject.GetComponent<AbstractUnitEntityView>();
		if (targetUnit == null)
		{
			return false;
		}
		if (button == 1 && InspectUnitsHelper.IsInspectAllow(targetUnit.EntityData))
		{
			EventBus.RaiseEvent(delegate(IUnitClickUIHandler h)
			{
				h.HandleUnitRightClick(targetUnit.Data);
			});
			return false;
		}
		if (TurnController.IsInTurnBasedCombat())
		{
			AbstractUnitEntity entityData = targetUnit.EntityData;
			if (entityData != null && entityData.IsDeadOrUnconscious)
			{
				Game.Instance.Controllers.ClickEventsController.GetHandler<ClickGroundHandler>().OnClick(gameObject, worldPosition, button, simulate, muteEvents);
			}
		}
		if (PhotonManager.Ping.CheckPingCoop(delegate
		{
			PhotonManager.Ping.PingEntity(targetUnit.EntityData);
		}))
		{
			return true;
		}
		if (IsUnitControllable(targetUnit))
		{
			if (!TurnController.IsInTurnBasedCombat() && !Game.Instance.IsControllerGamepad)
			{
				HandleClickControllableUnit(targetUnit.Data);
			}
			return true;
		}
		if (targetUnit.EntityData is BaseUnitEntity item && Game.Instance.Player.Party.Contains(item))
		{
			return false;
		}
		return HandleClickUnit(SelectionManagerFacade.GetNearestSelectedUnit(targetUnit.transform.position), targetUnit);
	}

	public static bool HandleClickUnit(BaseUnitEntity selectedUnit, AbstractUnitEntityView targetUnit)
	{
		if (PhotonManager.Ping.CheckPingCoop(delegate
		{
			PhotonManager.Ping.PingEntity(targetUnit.EntityData);
		}))
		{
			return true;
		}
		if (selectedUnit == null)
		{
			return false;
		}
		if (!selectedUnit.IsInCombat && targetUnit.EntityData.IsDeadAndHasLoot)
		{
			Vector3 targetPosition = ((targetUnit is UnitEntityView unitEntityView && unitEntityView.EntityData.Inventory.IsLootDropped) ? targetUnit.Data.Position : targetUnit.ViewTransform.position);
			PathfindingService.Instance.FindPathRT(selectedUnit.MovementAgent, targetPosition, targetUnit.MovementAgent.Corpulence * 2f, delegate(ForcedPath path)
			{
				if (path.error)
				{
					PFLog.Pathfinding.Error("An error path was returned. Ignoring");
				}
				else if (selectedUnit.IsMovementLockedByGameModeOrCombat())
				{
					PFLog.Pathfinding.Log("Movement is locked due to GameMode or Combat. Ignoring");
				}
				else
				{
					UnitMoveToParams cmdParams = new UnitMoveToParams(path, targetPosition, targetUnit.MovementAgent.Corpulence * 2f)
					{
						IsSynchronized = true
					};
					selectedUnit.Commands.Run(cmdParams);
					selectedUnit.Commands.AddToQueue(new UnitLootUnitParams(targetUnit.EntityData, targetPosition)
					{
						IsSynchronized = true
					});
				}
			});
			return true;
		}
		bool flag = targetUnit.EntityData.IsInCombat || selectedUnit.IsInCombat;
		IUnitInteraction unitInteraction = targetUnit.EntityData.SelectClickInteraction(selectedUnit);
		if (unitInteraction != null && (unitInteraction.AllowInCombat || !flag))
		{
			if (flag)
			{
				InteractInCombat(unitInteraction, selectedUnit, targetUnit.EntityData);
			}
			else
			{
				InteractOutOfCombat(unitInteraction, selectedUnit, targetUnit.EntityData);
			}
			return true;
		}
		return false;
	}

	public static void HandleClickControllableUnit(MechanicEntity mechanicEntity, bool isDoubleClick = false)
	{
		if (Game.Instance.CurrentModeType == GameModeType.Cutscene || (mechanicEntity != null && !mechanicEntity.IsPlayerFaction) || !(mechanicEntity is BaseUnitEntity baseUnitEntity))
		{
			return;
		}
		if (Game.Instance.Controllers.SelectedAbilityHandler?.Ability != null)
		{
			Game.Instance.Controllers.SelectedAbilityHandler.OnClick(baseUnitEntity.View.gameObject, baseUnitEntity.View.transform.position, -1);
			return;
		}
		Game.Instance.Controllers.SelectionCharacter.SetSelected(baseUnitEntity);
		if (isDoubleClick)
		{
			if (!UtilityGame.IsGlobalMap() && baseUnitEntity.IsViewActive)
			{
				CameraRig.Instance.ScrollTo(baseUnitEntity.Position);
				if ((bool)SettingsRoot.Controls.CameraFollowsUnit)
				{
					Game.Instance.Controllers.CameraController?.Follower?.Follow(baseUnitEntity);
				}
			}
		}
		else if (baseUnitEntity.IsViewActive)
		{
			Game.Instance.Controllers.CameraController?.Follower?.ScrollTo(baseUnitEntity);
		}
	}

	private static void RunMoveCommandTB(BaseUnitEntity unit, MoveCommandSettings settings)
	{
		if (Game.Instance.Controllers.TurnController.TurnBasedModeActive && unit == Game.Instance.Controllers.TurnController.CurrentUnit)
		{
			UnitMoveToProperParams unitMoveToProperParams = unit.TryCreateMoveCommandTB(settings, showMovePrediction: false);
			if (unitMoveToProperParams != null)
			{
				unit?.Commands.Run(unitMoveToProperParams);
			}
		}
	}

	private bool IsUnitControllable(AbstractUnitEntityView unit)
	{
		return unit.EntityData.IsDirectlyControllable;
	}

	private static void InteractOutOfCombat(IUnitInteraction interaction, BaseUnitEntity selectedUnit, AbstractUnitEntity targetUnit)
	{
		if (interaction.MainPlayerPreferred)
		{
			BaseUnitEntity mainCharacterEntity = Game.Instance.Player.MainCharacterEntity;
			if (SelectionManagerFacade.IsSelected(mainCharacterEntity) && mainCharacterEntity.IsDirectlyControllable && !mainCharacterEntity.Stealth.Active)
			{
				float num = mainCharacterEntity.DistanceTo(targetUnit);
				float num2 = selectedUnit.DistanceTo(targetUnit);
				if (num < 15f || num - num2 < 5f)
				{
					selectedUnit = mainCharacterEntity;
				}
			}
		}
		PathfindingService.Instance.FindPathRT(selectedUnit.MovementAgent, targetUnit.ViewTransform.position, interaction.Distance, delegate(ForcedPath path)
		{
			if (path.error)
			{
				PFLog.Pathfinding.Error("An error path was returned. Ignoring");
			}
			else if (selectedUnit.IsMovementLockedByGameModeOrCombat())
			{
				PFLog.Pathfinding.Log("Movement is locked due to GameMode or Combat. Ignoring");
			}
			else
			{
				if (path.vectorPath.Count > 2 || (path.vectorPath[1] - path.vectorPath[0]).sqrMagnitude > 1f || (path.vectorPath[0] - targetUnit.ViewTransform.position).magnitude > (float)interaction.Distance - 1f)
				{
					UnitMoveToParams cmdParams = new UnitMoveToParams(path, targetUnit.ViewTransform.position, interaction.Distance)
					{
						IsSynchronized = true
					};
					selectedUnit.Commands.Run(cmdParams);
				}
				selectedUnit.Commands.AddToQueue(new UnitInteractWithUnitParams(targetUnit)
				{
					IsSynchronized = true
				});
			}
		});
	}

	private static void InteractInCombat(IUnitInteraction interaction, BaseUnitEntity selectedUnit, AbstractUnitEntity targetUnit)
	{
		if (selectedUnit.DistanceToInCells(targetUnit) > 1)
		{
			EventBus.RaiseEvent(delegate(ICursorNotificationUIHandler h)
			{
				h.HandleNotification(ConfigRoot.Instance.LocalizedTexts.Reasons.TargetTooFar, WarningNotificationFormat.Attention);
			});
		}
		else
		{
			selectedUnit.Commands.Run(new UnitInteractWithUnitParams(targetUnit));
		}
	}
}
