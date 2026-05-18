using System;
using Kingmaker.Code.View.Bridge.Services;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.Utility.StatefulRandom;

namespace Kingmaker.Code.Framework.GameLog;

public class IdentifyLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventIdentify>
{
	public void HandleEvent(GameLogEventIdentify evt)
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			ItemEntity targetItem = evt.TargetItem;
			BaseUnitEntity actor = evt.Actor;
			GameLogContext.Text = targetItem.Name;
			switch (evt.Result)
			{
			case GameLogEventIdentify.ResultType.Success:
			{
				ItemEntity itemEntity = ItemsEntityFactory.CreateItemCopy(targetItem, 1);
				GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)actor;
				GameLogContext.Tooltip = itemEntity;
				GameLogContext.Description = targetItem.Description;
				AddMessage(new CombatLogMessage(LogThreadBase.Strings.ItemIdentified.CreateCombatLogMessage(), CombatLogTooltipService.CreateTooltipTemplateItemForLog(itemEntity, actor)));
				break;
			}
			case GameLogEventIdentify.ResultType.Fail:
				GameLogContext.Description = "{m|" + StatType.SkillTechUse.ToString() + "}" + UtilitySkillcheck.GetSkillCheckName(StatType.SkillTechUse) + "{/m}";
				AddMessage(LogThreadBase.Strings.ItemUnidentified.CreateCombatLogMessage());
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}
}
