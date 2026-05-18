using System;
using System.Collections.Generic;
using System.Linq;
using Code.GameCore.Mics;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Root;
using Kingmaker.Cheats;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.View;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker;

public class GameHistoryLog : IQuestHandler, ISubscriber, IQuestObjectiveHandler, IUnlockHandler, IUnlockValueHandler, IItemsCollectionHandler, IUnitHandler, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>, IPartyHandler, ISubscriber<IBaseUnitEntity>, ICompanionChangeHandler, ILevelUpCompleteUIHandler, ICutsceneHandler, ISubscriber<CutscenePlayerData>, IScriptZoneHandler, IPartyCombatHandler, IRollSkillCheckHandler, IUnitFactionHandler, ISelectAnswerHandler, IGlobalRulebookHandler<RulePerformSkillCheck>, IRulebookHandler<RulePerformSkillCheck>, IGlobalRulebookSubscriber
{
	[CanBeNull]
	private static GameHistoryLog s_Instance;

	[NotNull]
	public static GameHistoryLog Instance
	{
		get
		{
			Initialize();
			return s_Instance;
		}
	}

	public static void Initialize()
	{
		if (s_Instance == null)
		{
			s_Instance = new GameHistoryLog();
			EventBus.Subscribe(s_Instance);
			GameCoreHistoryLog instance = GameCoreHistoryLog.Instance;
			instance.EtudeEventAction = (Action<UnityEngine.Object, string>)Delegate.Combine(instance.EtudeEventAction, new Action<UnityEngine.Object, string>(s_Instance.EtudeEvent));
		}
	}

	public void DialogEvent(UnityEngine.Object context, string message)
	{
		if (!(context.name == "DefaultContinue"))
		{
			AddMessage(PFLog.History.Dialog, context, message);
		}
	}

	public void DialogEvent(ICanBeLogContext context, string message)
	{
		if (!((context as SimpleBlueprint)?.name == "DefaultContinue"))
		{
			AddMessage(PFLog.History.Dialog, context, message);
		}
	}

	public void EtudeEvent(UnityEngine.Object context, string message)
	{
		AddMessage(PFLog.History.Etudes, context, message);
	}

	public void SystemEvent(string message)
	{
		AddMessage(PFLog.History.System, null, message);
	}

	public void AreaLoading(BlueprintArea oldArea, BlueprintArea newArea, SceneReference[] scenes)
	{
		if (oldArea != newArea)
		{
			AddMessage(PFLog.History.Area, oldArea, "unloading area");
			AddMessage(PFLog.History.Area, newArea, "loading area");
		}
		else
		{
			AddMessage(PFLog.History.Area, newArea, "reloading area");
		}
		foreach (SceneReference sceneReference in scenes)
		{
			AddMessage(PFLog.History.Area, null, "scene: " + sceneReference.SceneName);
		}
	}

	public void HandleQuestStarted(Quest quest)
	{
		AddMessage(PFLog.History.Quests, quest.Blueprint, "quest started");
	}

	public void HandleQuestCompleted(Quest quest)
	{
		AddMessage(PFLog.History.Quests, quest.Blueprint, "quest completed");
	}

	public void HandleQuestFailed(Quest quest)
	{
		AddMessage(PFLog.History.Quests, quest.Blueprint, "quest failed");
	}

	public void HandleQuestUpdated(Quest objective)
	{
	}

	public void HandleQuestObjectiveStarted(QuestObjective objective)
	{
		AddMessage(PFLog.History.Quests, objective.Blueprint, "objective started");
	}

	public void HandleQuestObjectiveBecameVisible(QuestObjective objective)
	{
		AddMessage(PFLog.History.Quests, objective.Blueprint, "objective became visible");
	}

	public void HandleQuestObjectiveCompleted(QuestObjective objective)
	{
		AddMessage(PFLog.History.Quests, objective.Blueprint, "objective completed");
	}

	public void HandleQuestObjectiveFailed(QuestObjective objective)
	{
		AddMessage(PFLog.History.Quests, objective.Blueprint, "objective failed");
	}

	public void HandleUnlock(BlueprintUnlockableFlag flag)
	{
		AddMessage(PFLog.History.Unlocks, flag, "unlocked");
	}

	public void HandleLock(BlueprintUnlockableFlag flag)
	{
		AddMessage(PFLog.History.Unlocks, flag, "locked");
	}

	public void HandleFlagValue(BlueprintUnlockableFlag flag, int value)
	{
		AddMessage(PFLog.History.Unlocks, flag, $"set to {value}");
	}

	public void OnNewDay()
	{
	}

	public void HandleItemsAdded(ItemsCollection collection, ItemEntity item, int count)
	{
		if (collection == Game.Instance.PartySharedInventory.Collection && !(item?.Owner?.GetBodyOptional()?.AdditionalLimbs?.Contains(item.HoldingSlot)).GetValueOrDefault())
		{
			string message = ((count == 1) ? "item found" : $"{count} items found");
			AddMessage(PFLog.History.Items, item?.Blueprint, message);
		}
	}

	public void HandleItemsRemoved(ItemsCollection collection, ItemEntity item, int count)
	{
		if (collection == Game.Instance.PartySharedInventory.Collection && !(item?.Owner?.GetBodyOptional()?.AdditionalLimbs?.Contains(item.HoldingSlot)).GetValueOrDefault())
		{
			string message = ((count == 1) ? "item lost" : $"{count} items lost");
			AddMessage(PFLog.History.Items, item?.Blueprint, message);
		}
	}

	public void HandleUnitSpawned()
	{
		AbstractUnitEntity abstractUnitEntity = EventInvokerExtensions.AbstractUnitEntity;
		if (abstractUnitEntity != null && !abstractUnitEntity.IsPlayerFaction)
		{
			AddMessage(PFLog.History.Area, abstractUnitEntity.Blueprint, "unit spawned");
		}
	}

	public void HandleUnitDestroyed()
	{
		AbstractUnitEntity abstractUnitEntity = EventInvokerExtensions.AbstractUnitEntity;
		if (abstractUnitEntity != null && !abstractUnitEntity.IsPlayerFaction)
		{
			AddMessage(PFLog.History.Area, abstractUnitEntity.Blueprint, "unit destroyed");
		}
	}

	public void HandleUnitDeath()
	{
		AbstractUnitEntity abstractUnitEntity = EventInvokerExtensions.AbstractUnitEntity;
		if (abstractUnitEntity != null)
		{
			if (abstractUnitEntity.IsPlayerFaction)
			{
				AddMessage(PFLog.History.Party, abstractUnitEntity.Blueprint, "companion dies");
			}
			else
			{
				AddMessage(PFLog.History.Area, abstractUnitEntity.Blueprint, "unit dies");
			}
		}
	}

	public void HandleAddCompanion()
	{
		AddMessage(PFLog.History.Party, EventInvokerExtensions.BaseUnitEntity.Blueprint, "add companion");
	}

	public void HandleCompanionActivated()
	{
		AddMessage(PFLog.History.Party, EventInvokerExtensions.BaseUnitEntity.Blueprint, "activate companion");
	}

	public void HandleCompanionRemoved(bool stayInGame)
	{
		AddMessage(PFLog.History.Party, EventInvokerExtensions.BaseUnitEntity.Blueprint, "remove companion");
	}

	public void HandleCapitalModeChanged()
	{
	}

	public void HandleRecruit()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		AddMessage(PFLog.History.Party, baseUnitEntity.Blueprint, "recruit companion");
	}

	public void HandleUnrecruit()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		AddMessage(PFLog.History.Party, baseUnitEntity.Blueprint, "unrecruit companion");
	}

	public void HandleLevelUpComplete()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		AddMessage(PFLog.History.Party, baseUnitEntity.Blueprint, $"level up: {baseUnitEntity.Progression.CharacterLevel}");
	}

	public void HandleCutsceneStarted(bool queued)
	{
		string text = "cutscene started";
		if (queued)
		{
			text += " (queued)";
		}
		CutscenePlayerData cutscenePlayerData = EventInvokerExtensions.Entity as CutscenePlayerData;
		AddMessage(PFLog.History.Area, cutscenePlayerData.Cutscene, text);
	}

	public void HandleCutsceneRestarted()
	{
		CutscenePlayerData cutscenePlayerData = EventInvokerExtensions.Entity as CutscenePlayerData;
		AddMessage(PFLog.History.Area, cutscenePlayerData.Cutscene, "cutscene restarted");
	}

	public void HandleCutscenePaused(CutscenePauseReason reason)
	{
		string name = Enum.GetName(typeof(CutscenePauseReason), reason);
		CutscenePlayerData cutscenePlayerData = EventInvokerExtensions.Entity as CutscenePlayerData;
		AddMessage(PFLog.History.Area, cutscenePlayerData.Cutscene, "cutscene paused because of " + name);
	}

	public void HandleCutsceneResumed()
	{
		CutscenePlayerData cutscenePlayerData = EventInvokerExtensions.Entity as CutscenePlayerData;
		AddMessage(PFLog.History.Area, cutscenePlayerData.Cutscene, "cutscene resumed");
	}

	public void HandleCutsceneStopped()
	{
		CutscenePlayerData cutscenePlayerData = EventInvokerExtensions.Entity as CutscenePlayerData;
		AddMessage(PFLog.History.Area, cutscenePlayerData.Cutscene, "cutscene stopped");
	}

	public void OnUnitEnteredScriptZone(ScriptZoneEntity zone)
	{
		AddMessage(PFLog.History.Area, zone, $"{EventInvokerExtensions.BaseUnitEntity.Blueprint} enters script zone");
	}

	public void OnUnitExitedScriptZone(ScriptZoneEntity zone)
	{
		AddMessage(PFLog.History.Area, zone, $"{EventInvokerExtensions.BaseUnitEntity.Blueprint} leaves script zone");
	}

	public void HandlePartyCombatStateChanged(bool inCombat)
	{
		if (inCombat)
		{
			List<BlueprintUnit> list = (from u in Game.Instance.EntityPools.AllBaseUnits
				where !u.Faction.IsPlayer
				where u.IsInCombat
				select u.Blueprint).ToList();
			int totalChallengeRating = Utilities.GetTotalChallengeRating(list);
			AddMessage(PFLog.History.Combat, null, $"party combat started | enemies count = {list.Count} | enemies cr = {totalChallengeRating}");
		}
		else
		{
			AddMessage(PFLog.History.Combat, null, "party combat finished");
		}
	}

	public void HandlePartySkillCheckRolled(RulePerformPartySkillCheck check)
	{
		string text = (check.Success ? "passed" : "failed");
		AddMessage(PFLog.History.Skill, check.Roller?.Blueprint, $"{check.StatType} check {text} (party) | dc = {check.Difficulty} | result = {check.RollResult} | d100 = {check.D100} | stat value = {check.StatValue}");
	}

	public void HandleUnitSkillCheckRolled(RulePerformSkillCheck check)
	{
	}

	public void OnEventAboutToTrigger(RulePerformSkillCheck evt)
	{
	}

	public void OnEventDidTrigger(RulePerformSkillCheck check)
	{
		string text = (check.ResultIsSuccess ? "passed" : "failed");
		AddMessage(PFLog.History.Skill, check.ConcreteInitiator?.Blueprint, $"{check.StatType} check {text} | dc = {check.Difficulty} | result = {check.RollResult} | d100 = {check.RollRule} | stat value = {check.StatValue}");
	}

	public void HandleFactionChanged()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		AddMessage(PFLog.History.Area, baseUnitEntity?.Blueprint, $"{baseUnitEntity} faction changed to {baseUnitEntity?.Faction}");
	}

	public void HandleSelectAnswer(BlueprintAnswer answer)
	{
		DialogEvent(answer, "Selected answer");
	}

	private static void AddMessage(LogChannel channel, [CanBeNull] object context, string message)
	{
		CalendarRoot calendar = ConfigRoot.Instance.Calendar;
		TimeSpan gameTime = Game.Instance.Controllers.TimeController.GameTime;
		DateTime dateTime = calendar.GetStartDate() + gameTime;
		int num = dateTime.Year + calendar.YearsShift;
		string text = $"{dateTime.Hour:D2}:{dateTime.Minute:D2}:{dateTime.Second:D2} {dateTime.Day:D2}.{dateTime.Month:D2}.{num}";
		string text2 = "";
		if (context != null)
		{
			if (context is EntityViewBase entityViewBase)
			{
				text2 = entityViewBase.name + " [" + entityViewBase.UniqueId + "]";
			}
			else if (context is Entity)
			{
				text2 = context.ToString();
			}
			else if (context is UnityEngine.Object @object)
			{
				text2 = @object.name + ": ";
			}
		}
		if (context is UnityEngine.Object ctx)
		{
			channel.Log(ctx, "{0} - {1}{2}", text, text2, message);
		}
		else if (context is ICanBeLogContext ctx2)
		{
			channel.Log(ctx2, "{0} - {1}{2}", text, text2, message);
		}
		else
		{
			channel.Log("{0} - {1}{2}", text, text2, message);
		}
	}

	private static void AddMessage(LogChannel channel, [CanBeNull] ICanBeLogContext context, string message)
	{
		CalendarRoot calendar = ConfigRoot.Instance.Calendar;
		TimeSpan gameTime = Game.Instance.Controllers.TimeController.GameTime;
		DateTime dateTime = calendar.GetStartDate() + gameTime;
		int num = dateTime.Year + calendar.YearsShift;
		string text = $"{dateTime.Hour:D2}:{dateTime.Minute:D2}:{dateTime.Second:D2} {dateTime.Day:D2}.{dateTime.Month:D2}.{num}";
		string text2 = "";
		if (context != null)
		{
			text2 = ((!(context is BlueprintScriptableObject blueprintScriptableObject)) ? $"{context}: " : (blueprintScriptableObject.name + " (" + blueprintScriptableObject.AssetGuid + "): "));
		}
		channel.Log(context, "{0} - {1}{2}", text, text2, message);
	}
}
