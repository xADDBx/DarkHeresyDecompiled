using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.Code.View.Bridge.Services;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.Utility.GameConst;
using Kingmaker.Utility.StatefulRandom;
using Owlcat.UI;

namespace Kingmaker.Code.Framework.GameLog;

public class ItemsCollectionLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventItemsCollection>, IGameLogEventHandler<MergeGameLogEvent<GameLogEventItemsCollection>>
{
	public void HandleEvent(GameLogEventItemsCollection evt)
	{
		CombatLogMessage combatLogMessage = GetCombatLogMessage(evt);
		AddMessage(combatLogMessage);
		if (combatLogMessage != null)
		{
			ShowNotification(combatLogMessage.Message);
		}
	}

	public void HandleEvent(MergeGameLogEvent<GameLogEventItemsCollection> evt)
	{
		IReadOnlyList<GameLogEventItemsCollection> events = evt.GetEvents();
		GameLogEventItemsCollection evt2 = events[0];
		if (events.Count == 1)
		{
			CombatLogMessage combatLogMessage = GetCombatLogMessage(evt2);
			AddMessage(combatLogMessage);
			if (combatLogMessage != null)
			{
				ShowNotification(combatLogMessage.Message);
			}
			return;
		}
		CombatLogMessage combatLogMessage2 = LogThreadBase.Strings.ItemGroup.CreateCombatLogMessage();
		TooltipBaseTemplate tooltipBaseTemplate = combatLogMessage2?.Tooltip;
		if (tooltipBaseTemplate != null)
		{
			ITooltipBrick[] array = CollectExtraBricks(events).ToArray();
			if (array.Length == 0)
			{
				return;
			}
			if (array.Length == 1)
			{
				CombatLogMessage combatLogMessage3 = GetCombatLogMessage(evt2);
				AddMessage(combatLogMessage3);
				return;
			}
			CombatLogTooltipService.SetTooltipTemplateCombatLogMessageExtraBricks(tooltipBaseTemplate, array, arg3: false);
			CombatLogTooltipService.SetTooltipTemplateCombatLogMessageExtraBricks(tooltipBaseTemplate, array, arg3: true);
		}
		AddMessage(combatLogMessage2);
		CollectNotifications(events);
	}

	private static CombatLogMessage GetCombatLogMessage(GameLogEventItemsCollection evt)
	{
		if (TurnController.IsInTurnBasedCombat())
		{
			return null;
		}
		if (evt.Count <= 0)
		{
			return null;
		}
		ItemEntity item = evt.Item;
		int count = evt.Count;
		ItemEntity itemEntity;
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			itemEntity = ItemsEntityFactory.CreateItemCopy(item, count);
		}
		GameLogContext.Text = item.Name;
		GameLogContext.Tooltip = itemEntity;
		GameLogContext.Count = count;
		switch (evt.Event)
		{
		case GameLogEventItemsCollection.EventType.Added:
		{
			GameLogMessage obj2 = ((count > 1) ? LogThreadBase.Strings.ItemsGained : LogThreadBase.Strings.ItemGained);
			return new CombatLogMessage(template: CombatLogTooltipService.CreateTooltipTemplateItemForLog(itemEntity, null), message: obj2.CreateCombatLogMessage());
		}
		case GameLogEventItemsCollection.EventType.Removed:
		{
			GameLogContext.Description = item.Name;
			GameLogMessage obj = ((count > 1) ? LogThreadBase.Strings.ItemsLost : LogThreadBase.Strings.ItemLost);
			return new CombatLogMessage(template: CombatLogTooltipService.CreateTooltipTemplateItemForLog(itemEntity, null), message: obj.CreateCombatLogMessage());
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private static IEnumerable<ITooltipBrick> CollectExtraBricks(IEnumerable<GameLogEventItemsCollection> events)
	{
		Func<CombatLogMessage, bool, ITooltipBrick> nestedMessageTemplate = CombatLogTooltipService.CreateBrickNestedMessage;
		if (nestedMessageTemplate == null)
		{
			yield break;
		}
		foreach (GameLogEventItemsCollection @event in events)
		{
			CombatLogMessage combatLogMessage = GetCombatLogMessage(@event);
			if (combatLogMessage != null)
			{
				yield return nestedMessageTemplate(combatLogMessage, arg2: true);
			}
		}
	}

	private void CollectNotifications(IReadOnlyList<GameLogEventItemsCollection> events)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int itemsToShowLootNotificationWindow = UIConsts.ItemsToShowLootNotificationWindow;
		int num = 0;
		foreach (GameLogEventItemsCollection @event in events)
		{
			CombatLogMessage combatLogMessage = GetCombatLogMessage(@event);
			if (combatLogMessage != null)
			{
				if (num >= itemsToShowLootNotificationWindow)
				{
					break;
				}
				num++;
				stringBuilder.AppendLine(combatLogMessage.Message);
			}
		}
		if (stringBuilder.Length > 0)
		{
			ShowNotification(stringBuilder.ToString());
		}
	}

	private void ShowNotification(string message)
	{
		EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
		{
			h.HandleWarning(message, addToLog: false);
		});
	}
}
