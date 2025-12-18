using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Controllers.Dialog;
using Kingmaker.Designers.EventConditionActionSystem.Events;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Persistence.SavesStorage;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Networking;
using Kingmaker.Networking.Settings;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.View.MapObjects;
using Kingmaker.Visual.CharacterSystem;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.GameCommands;

public static class GameCommandQueueExtensions
{
	public static void SkipBark([NotNull] this GameCommandQueue gameCommandQueue)
	{
		gameCommandQueue.AddCommand(new SkipBarkGameCommand());
	}

	public static void SkipCutscene([NotNull] this GameCommandQueue gameCommandQueue)
	{
		gameCommandQueue.AddCommand(new SkipCutsceneGameCommand());
	}

	public static void ScheduleSwitchCutsceneLock([NotNull] this GameCommandQueue gameCommandQueue, bool @lock)
	{
		gameCommandQueue.AddCommand(new SwitchCutsceneLockCommand(@lock));
	}

	public static void ScheduleDialogStart([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] DialogData dialog)
	{
		gameCommandQueue.AddCommand(new StartScheduledDialogCommand(dialog));
	}

	public static void SchedulePostSaveCallback([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] SaveInfo saveInfo, [NotNull] SaveCreateDTO dto)
	{
		PFLog.System.Log("Post-save callback requested");
		gameCommandQueue.AddCommand(new PostSaveCallbackCommand(saveInfo, dto));
	}

	public static void EndTurnManually([NotNull] this GameCommandQueue gameCommandQueue, MechanicEntity entity)
	{
		if (!gameCommandQueue.ContainsCommand((EndTurnGameCommand cmd) => cmd.MechanicEntity == entity))
		{
			gameCommandQueue.AddCommand(new EndTurnGameCommand(entity));
		}
	}

	public static void SetPauseManualState([NotNull] this GameCommandQueue gameCommandQueue, bool toPause)
	{
		gameCommandQueue.AddCommand(new SetPauseGameCommand(toPause));
	}

	public static void RequestPauseUi([NotNull] this GameCommandQueue gameCommandQueue, bool toPause)
	{
		if (!gameCommandQueue.ContainsCommand((RequestPauseGameCommand cmd) => cmd.ToPause == toPause))
		{
			gameCommandQueue.AddCommand(new RequestPauseGameCommand(toPause));
		}
	}

	public static void DialogAnswer([NotNull] this GameCommandQueue gameCommandQueue, int tick, [NotNull] string answer)
	{
		gameCommandQueue.AddCommand(new DialogAnswerGameCommand(tick, answer));
	}

	public static void AreaTransition([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] AreaTransitionPart areaTransitionPart, bool isPlayerCommand, BaseUnitEntity executorEntity)
	{
		gameCommandQueue.AddCommand(new AreaTransitionPartGameCommand(areaTransitionPart, isPlayerCommand, executorEntity));
	}

	public static void AreaTransition([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] BlueprintMultiEntranceEntry multiEntrance)
	{
		gameCommandQueue.AddCommand(new AreaTransitionGameCommand(multiEntrance));
	}

	public static void ClearAreaTransitionGroupDuplicates([NotNull] this GameCommandQueue gameCommandQueue)
	{
		gameCommandQueue.AddCommand(new ClearAreaTransitionGroupGameCommand());
	}

	public static void DropItem([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] ItemEntity item, bool split, int splitCount)
	{
		gameCommandQueue.AddCommand(new DropItemGameCommand(item, split, splitCount));
	}

	public static void EquipItem([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] ItemEntity item, [NotNull] MechanicEntity unit, ItemSlotRef to)
	{
		gameCommandQueue.AddCommand(new EquipItemGameCommand(item, unit, to));
	}

	public static void UnequipItem([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] MechanicEntity owner, [NotNull] ItemSlotRef from, ItemSlotRef to)
	{
		gameCommandQueue.AddCommand(new UnequipItemGameCommand(owner, from, to));
	}

	public static void SwapSlots([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] MechanicEntity entity, [NotNull] ItemSlotRef from, [NotNull] ItemSlotRef to, bool isLoot)
	{
		gameCommandQueue.AddCommand(new SwapSlotsGameCommand(entity, from, to, isLoot));
	}

	public static void SplitSlot([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] ItemSlotRef from, ItemSlotRef to, bool isLoot, int count)
	{
		gameCommandQueue.AddCommand(new SplitSlotGameCommand(from, to, isLoot, count));
	}

	public static void MergeSlot([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] ItemSlotRef from, [NotNull] ItemSlotRef to)
	{
		gameCommandQueue.AddCommand(new MergeSlotGameCommand(from, to));
	}

	public static void SwitchHandEquipment([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] BaseUnitEntity unit, int handEquipmentSetIndex)
	{
		gameCommandQueue.AddCommand(new SwitchHandEquipmentGameCommand(unit, handEquipmentSetIndex));
	}

	public static void RemoveFromBuyVendor([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] ItemEntity item, int count)
	{
		gameCommandQueue.AddCommand(new RemoveFromBuyVendorGameCommand(item, count));
	}

	public static void AddForBuyVendor([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] ItemEntity item, int count, bool makeDeal = false)
	{
		gameCommandQueue.AddCommand(new AddForBuyVendorGameCommand(item, count, makeDeal));
	}

	public static void RemoveFromSellVendor([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] ItemEntity item, int count)
	{
		gameCommandQueue.AddCommand(new RemoveFromSellVendorGameCommand(item, count));
	}

	public static void AddForSellVendor([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] ItemEntity item, int count, bool makeDeal = false)
	{
		gameCommandQueue.AddCommand(new AddForSellVendorGameCommand(item, count, makeDeal));
	}

	public static void TransferItem([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] ItemEntity item, [NotNull] ItemsCollectionRef to, int count)
	{
		gameCommandQueue.AddCommand(new TransferItemGameCommand(to, item, count));
	}

	public static void CollectLoot([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] List<EntityRef<ItemEntity>> items)
	{
		gameCommandQueue.AddCommand(new CollectLootGameCommand(items));
	}

	public static void TransferItemsToInventory([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] List<EntityRef<ItemEntity>> items)
	{
		gameCommandQueue.AddCommand(new TransferItemToInventoryGameCommand(items));
	}

	public static void StartTrading([NotNull] this GameCommandQueue gameCommandQueue, MechanicEntity vendor, bool isSynchronized)
	{
		gameCommandQueue.AddCommand(new StartTradingGameCommand(vendor, isSynchronized));
	}

	public static void EndTrading([NotNull] this GameCommandQueue gameCommandQueue)
	{
		gameCommandQueue.AddCommand(new EndTradingGameCommand());
	}

	public static void SaveGame([NotNull] this GameCommandQueue gameCommandQueue, [CanBeNull] SaveInfo saveInfo, [CanBeNull] string saveName = null, Action callback = null)
	{
		gameCommandQueue.AddCommand(new SaveGameCommand(saveInfo, saveName, callback));
	}

	public static void SetInventorySorter([NotNull] this GameCommandQueue gameCommandQueue, ItemsSorterType sorterType)
	{
		gameCommandQueue.AddCommand(new SetInventorySorterGameCommand(sorterType));
	}

	public static void CloseScreen([NotNull] this GameCommandQueue gameCommandQueue, IScreenUIHandler.ScreenType screenType, bool isSynchronized = true)
	{
		gameCommandQueue.AddCommand(new CloseScreenCommand(screenType, isSynchronized));
	}

	public static void SetCurrentQuest([NotNull] this GameCommandQueue gameCommandQueue, Quest quest)
	{
		gameCommandQueue.AddCommand(new SetCurrentQuestGameCommand(quest));
	}

	public static void CommitLvlUp([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] LevelUpManager levelUpManager)
	{
		if (levelUpManager.Path != null)
		{
			gameCommandQueue.AddCommand(new CommitLevelUpGameCommand(levelUpManager));
		}
	}

	public static void AcceptChangeGroup([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] List<UnitReference> partyCharacters, [NotNull] List<UnitReference> remoteCharacters, [NotNull] List<BlueprintUnitReference> requiredCharacters, bool reInitPartyCharacters)
	{
		gameCommandQueue.AddCommand(new AcceptChangeGroupGameCommand(partyCharacters, remoteCharacters, requiredCharacters, reInitPartyCharacters));
	}

	public static void LoadArea([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] BlueprintAreaEnterPoint enterPoint, AutoSaveMode autoSaveMode)
	{
		gameCommandQueue.LoadArea(enterPoint.ToReference<BlueprintAreaEnterPointReference>(), autoSaveMode);
	}

	public static void LoadArea([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] BlueprintAreaEnterPointReference enterPoint, AutoSaveMode autoSaveMode)
	{
		gameCommandQueue.AddCommand(new LoadAreaGameCommand(enterPoint, autoSaveMode));
	}

	public static void SetSettings([NotNull] this GameCommandQueue gameCommandQueue, List<BaseSettingNetData> settingCommand)
	{
		gameCommandQueue.AddCommand(new SettingGameCommand(settingCommand));
	}

	public static void DoSpeedUp([NotNull] this GameCommandQueue gameCommandQueue)
	{
		gameCommandQueue.AddCommand(new SpeedUpGameCommand());
	}

	public static void StopSpeedUp([NotNull] this GameCommandQueue gameCommandQueue)
	{
		gameCommandQueue.AddCommand(new StopSpeedUpGameCommand());
	}

	public static void PingPosition([NotNull] this GameCommandQueue gameCommandQueue, Vector3 position)
	{
		gameCommandQueue.AddCommand(new PingPositionGameCommand(position));
	}

	public static void PingEntity([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] Entity entity)
	{
		gameCommandQueue.AddCommand(new PingEntityGameCommand(entity));
	}

	public static void PingDialogAnswer([NotNull] this GameCommandQueue gameCommandQueue, string answer, bool isHover)
	{
		gameCommandQueue.AddCommand(new PingDialogAnswerGameCommand(answer, isHover));
	}

	public static void PingDialogAnswerVote([NotNull] this GameCommandQueue gameCommandQueue, string answer)
	{
		gameCommandQueue.AddCommand(new PingDialogAnswerVoteGameCommand(answer));
	}

	public static void PingActionBarAbility([NotNull] this GameCommandQueue gameCommandQueue, string keyName, Entity characterEntityRef, int slotIndex)
	{
		gameCommandQueue.AddCommand(new PingActionBarAbilityGameCommand(keyName, characterEntityRef, slotIndex));
	}

	public static void ChangePlayerRole([NotNull] this GameCommandQueue gameCommandQueue, string entityId, NetPlayer player, bool enable)
	{
		gameCommandQueue.AddCommand(new ChangePlayerRoleGameCommand(entityId, player, enable));
	}

	public static void SetQuestViewed([NotNull] this GameCommandQueue gameCommandQueue, Quest quest)
	{
		gameCommandQueue.AddCommand(new SetQuestViewedGameCommand(quest));
	}

	public static void SetQuestObjectiveViewed([NotNull] this GameCommandQueue gameCommandQueue, QuestObjective questObjective)
	{
		gameCommandQueue.AddCommand(new SetQuestObjectiveViewedGameCommand(questObjective));
	}

	public static void PartyFormationIndex([NotNull] this GameCommandQueue gameCommandQueue, int formationIndex)
	{
		gameCommandQueue.AddCommand(new PartyFormationIndexGameCommand(formationIndex));
	}

	public static void PartyFormationResetGameCommand([NotNull] this GameCommandQueue gameCommandQueue)
	{
		gameCommandQueue.AddCommand(new PartyFormationResetGameCommand(Game.Instance.Player.FormationManager.CurrentFormationIndex));
	}

	public static void PartyFormationOffset([NotNull] this GameCommandQueue gameCommandQueue, int index, BaseUnitEntity unit, Vector2 vector)
	{
		gameCommandQueue.AddCommand(new PartyFormationOffsetGameCommand(Game.Instance.Player.FormationManager.CurrentFormationIndex, index, unit, vector));
	}

	public static void TriggerLoot([NotNull] this GameCommandQueue gameCommandQueue, InteractionLootPart interactionLootPart, TriggerLootGameCommand.TriggerType type, [CanBeNull] ItemEntity item = null)
	{
		gameCommandQueue.AddCommand(new TriggerLootGameCommand(interactionLootPart, type, item));
	}

	public static void TestLoadingProcessCommandsLogicGameCommand([NotNull] this GameCommandQueue gameCommandQueue, int counter, NetPlayer player)
	{
		gameCommandQueue.AddCommand(new TestLoadingProcessCommandsLogicGameCommand(counter, player));
	}

	public static void StopUnits([NotNull] this GameCommandQueue gameCommandQueue, IList<BaseUnitEntity> units)
	{
		gameCommandQueue.AddCommand(new StopUnitsGameCommand(units));
	}

	public static void HoldUnits([NotNull] this GameCommandQueue gameCommandQueue, IList<BaseUnitEntity> units)
	{
		gameCommandQueue.AddCommand(new HoldUnitsGameCommand(units));
	}

	public static void UIEventTrigger([NotNull] this GameCommandQueue gameCommandQueue, UIEventTrigger uiEventTrigger)
	{
		gameCommandQueue.AddCommand(new UIEventTriggerGameCommand(uiEventTrigger));
	}

	public static void FinishRespec([NotNull] this GameCommandQueue gameCommandQueue, BaseUnitEntity respecEntity, bool forFree)
	{
		gameCommandQueue.AddCommand(new FinishRespecGameCommand(respecEntity, forFree));
	}

	public static void DrawMovePrediction([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] BaseUnitEntity unit, [NotNull] Path path, [CanBeNull] float[] costPerEveryCell, [CanBeNull] UnitCommandParams unitCommandParams)
	{
		gameCommandQueue.AddCommand(new DrawMovePredictionGameCommand(unit, path, costPerEveryCell, unitCommandParams));
	}

	public static void ClearMovePrediction([NotNull] this GameCommandQueue gameCommandQueue)
	{
		bool isSynchronized = !ContextData<GameCommandContext>.Current && !ContextData<UnitCommandContext>.Current;
		gameCommandQueue.AddCommand(new ClearMovePredictionGameCommand(isSynchronized));
	}

	public static void SetEquipmentColor([NotNull] this GameCommandQueue gameCommandQueue, BaseUnitEntity unit, RampColorPreset.IndexSet indexSet)
	{
		gameCommandQueue.AddCommand(new SetEquipmentColorGameCommand(indexSet, unit));
	}

	public static void GroupChanger([NotNull] this GameCommandQueue gameCommandQueue, UnitReference unitReference)
	{
		gameCommandQueue.AddCommand(new GroupChangerGameCommand(unitReference));
	}

	public static void CameraFollowTimeScale([NotNull] this GameCommandQueue gameCommandQueue, float scale, bool force)
	{
		gameCommandQueue.AddCommand(new CameraFollowTimeScaleGameCommand(scale, force));
	}

	public static void ClearPointerMode([NotNull] this GameCommandQueue gameCommandQueue)
	{
		if (!ContextData<GameCommandContext>.Current && !ContextData<UnitCommandContext>.Current)
		{
			Game.Instance.Controllers.ClickEventsController.ClearPointerMode();
		}
		else
		{
			gameCommandQueue.AddCommand(new ClearPointerModeGameCommand());
		}
	}

	public static void InterruptMoveUnit([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] AbstractUnitEntity unit)
	{
		gameCommandQueue.AddCommand(new InterruptMoveUnitGameCommand(unit));
	}
}
