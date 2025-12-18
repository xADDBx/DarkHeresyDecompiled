using System;
using System.Collections.Generic;
using System.Text;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.Code.View.Bridge.Services;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.Framework.GameLog;

public class QuickSlotsReplenishLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventQuickSlotsReplenish>
{
	private const string ItemCountFormat = "{0} x{1}";

	private readonly Dictionary<BlueprintItem, int> m_CachedItemsCount = new Dictionary<BlueprintItem, int>();

	private readonly StringBuilder m_StringBuilder = new StringBuilder();

	public void HandleEvent(GameLogEventQuickSlotsReplenish evt)
	{
		IReadOnlyDictionary<MechanicEntity, List<BlueprintItem>> replenishSuccess = evt.Result.ReplenishSuccess;
		bool flag = replenishSuccess != null && replenishSuccess.Count > 0;
		replenishSuccess = evt.Result.ReplenishFailure;
		bool num = replenishSuccess != null && replenishSuccess.Count > 0;
		if (flag)
		{
			AddQuickSlotsResultMessage(evt.Result.ReplenishSuccess, LogThreadBase.Strings.QuickSlotsReplenishSuccess);
		}
		if (num)
		{
			AddQuickSlotsResultMessage(evt.Result.ReplenishFailure, LogThreadBase.Strings.QuickSlotsReplenishFailure);
		}
	}

	private void AddQuickSlotsResultMessage(IReadOnlyDictionary<MechanicEntity, List<BlueprintItem>> result, GameLogMessage messageTemplate)
	{
		string text = messageTemplate.Message.Text;
		TooltipBaseTemplate tooltipBaseTemplate = CombatLogTooltipService.CreateTooltipTemplateCombatLogMessage(text, string.Empty, 0f);
		IEnumerable<ITooltipBrick> tooltipBricks = GetTooltipBricks(result);
		CombatLogTooltipService.SetTooltipTemplateCombatLogMessageExtraBricks(tooltipBaseTemplate, tooltipBricks, arg3: false);
		PrefixIcon icon = messageTemplate.Icon;
		Color32 color = messageTemplate.Color;
		CombatLogMessage newMessage = new CombatLogMessage(text, color, icon, tooltipBaseTemplate);
		AddMessage(newMessage);
	}

	private IEnumerable<ITooltipBrick> GetTooltipBricks(IReadOnlyDictionary<MechanicEntity, List<BlueprintItem>> result)
	{
		Func<CombatLogMessage, bool, ITooltipBrick> getTemplate = CombatLogTooltipService.CreateTooltipBrickNestedMessage;
		if (getTemplate == null || result == null)
		{
			yield break;
		}
		foreach (KeyValuePair<MechanicEntity, List<BlueprintItem>> item in result)
		{
			item.Deconstruct(out var key, out var value);
			MechanicEntity mechanicEntity = key;
			List<BlueprintItem> list = value;
			GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)mechanicEntity;
			m_CachedItemsCount.Clear();
			m_StringBuilder.Clear();
			BlueprintItem key2;
			foreach (BlueprintItem item2 in list)
			{
				if (!m_CachedItemsCount.ContainsKey(item2))
				{
					m_CachedItemsCount[item2] = 1;
					continue;
				}
				Dictionary<BlueprintItem, int> cachedItemsCount = m_CachedItemsCount;
				key2 = item2;
				cachedItemsCount[key2]++;
			}
			bool flag = true;
			foreach (KeyValuePair<BlueprintItem, int> item3 in m_CachedItemsCount)
			{
				item3.Deconstruct(out key2, out var value2);
				BlueprintItem blueprintItem = key2;
				int num = value2;
				if (!flag)
				{
					m_StringBuilder.Append(", ");
				}
				flag = false;
				m_StringBuilder.AppendFormat("{0} x{1}", blueprintItem.Name, num);
			}
			GameLogContext.Text = m_StringBuilder.ToString();
			CombatLogMessage arg = LogThreadBase.Strings.QuickSlotReplenishItem.CreateCombatLogMessage();
			yield return getTemplate(arg, arg2: true);
		}
	}
}
