using System;
using System.Collections.Generic;
using System.Text;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Controllers.Dialog;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Interaction;
using Kingmaker.Settings;
using Kingmaker.UI.Common;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using UnityEngine;

namespace Kingmaker.Code.View.UI.UIUtilities;

public static class UtilitySkillcheck
{
	private const char NewLineChar = '\n';

	public static string SkillCheckText(List<SkillCheckResult> skillCheck, SkillCheckColors colors, string suffix = " ")
	{
		string text = "";
		if ((bool)SettingsRoot.Game.Dialogs.ShowSkillcheckResult)
		{
			if (skillCheck == null)
			{
				return text;
			}
			foreach (SkillCheckResult item in skillCheck)
			{
				text += SkillCheckText(item, colors, suffix);
			}
		}
		return text;
	}

	public static string SkillCheckText(SkillCheckResult skillCheck, SkillCheckColors colors, string suffix)
	{
		if (skillCheck == null)
		{
			return null;
		}
		string arg = ColorUtility.ToHtmlStringRGB(skillCheck.Passed ? colors.Success : colors.Failure);
		UIDialog dialog = ConfigRoot.Instance.LocalizedTexts.UserInterfacesText.Dialog;
		string text = (skillCheck.Passed ? dialog.Succeeded : dialog.Failed);
		string text2 = EntityLink.Type.UnitStat.GetTag() + ":" + skillCheck.StatType.ToString() + ":" + skillCheck.ActingUnit.UniqueId;
		string format = (skillCheck.Passed ? dialog.SucccedeedCheckFormat : dialog.FailedCheckFormat);
		string arg2 = "<link=\"" + EntityLink.Type.SkillcheckResult.ToString() + "\">" + text + "</link>";
		string arg3 = "<link=\"" + text2 + "\">" + LocalizedTexts.Instance.Stats.GetText(skillCheck.StatType) + "</link>";
		return string.Format(format, arg2, arg, arg3) + suffix;
	}

	public static string GetInteractionVariantActorText(IInteractionVariantActor interactionActor, List<BaseUnitEntity> units, out bool needChanceText)
	{
		needChanceText = false;
		if (interactionActor == null)
		{
			return string.Empty;
		}
		int? interactionDC = interactionActor.InteractionDC;
		string interactionName = interactionActor.GetInteractionName();
		if (!interactionDC.HasValue)
		{
			return "[" + interactionName + "]";
		}
		needChanceText = true;
		BaseUnitEntity baseUnitEntity = interactionActor.InteractionPart?.SelectUnit(units, muteEvents: false, interactionActor);
		interactionDC = InteractionHelper.GetInteractionSkillCheckChance(baseUnitEntity, interactionActor.Skill, interactionDC.Value);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(interactionName + ": ");
		if (baseUnitEntity != null)
		{
			stringBuilder.Append($"{interactionDC}% [{baseUnitEntity.Name}]");
		}
		else
		{
			stringBuilder.Append("Locked%");
		}
		return stringBuilder.ToString();
	}

	public static string GetInteractionChance(IInteractionVariantActor interactionActor, List<BaseUnitEntity> units)
	{
		int? num = interactionActor?.InteractionDC;
		if (!num.HasValue)
		{
			return string.Empty;
		}
		return InteractionHelper.GetInteractionSkillCheckChance(interactionActor.InteractionPart?.SelectUnit(units, muteEvents: false, interactionActor), interactionActor.Skill, num.Value).ToString();
	}

	public static string GetOvertipSkillCheckText(InteractionSkillCheckPart skillCheck, List<BaseUnitEntity> units, out bool needChanceText)
	{
		needChanceText = false;
		if (skillCheck == null)
		{
			return string.Empty;
		}
		StatType statType = (skillCheck.SkillOverride.IsSkill() ? skillCheck.SkillOverride : skillCheck.Settings.Skill);
		if (statType == StatType.Unknown)
		{
			return string.Empty;
		}
		string text = ConfigRoot.Instance.LocalizedTexts.Stats.GetText(statType);
		if (skillCheck.Settings.HideDC)
		{
			return "[" + text + "]";
		}
		int num = skillCheck.DCOverride;
		if (num == 0)
		{
			num = skillCheck.Settings.GetDC();
		}
		needChanceText = true;
		num = InteractionHelper.GetInteractionSkillCheckChance(skillCheck.SelectUnit(units), statType, num);
		return $"[{text}: {num}%]";
	}

	public static int GetSkillCheckChance(SkillCheckDC skillCheckDC)
	{
		return Mathf.Clamp(skillCheckDC.ConditionDC + skillCheckDC.ValueDC, 0, 100);
	}

	public static string GetSkillCheckName(StatType statType)
	{
		return LocalizedTexts.Instance.Stats.GetText(statType);
	}

	public static string GetLootSkillCheck(SkillCheckResult skillCheckResult)
	{
		if (skillCheckResult == null)
		{
			return string.Empty;
		}
		StringBuilder builder = ContextData<PooledStringBuilder>.Request().Builder;
		UILoot lootWindow = UIStrings.Instance.LootWindow;
		builder.Append(string.Format(arg0: LocalizedTexts.Instance.Stats.GetText(skillCheckResult.StatType), format: lootWindow.SkillCheckTitle.Text));
		builder.Append('\n');
		builder.Append(string.Format(arg0: (string)(skillCheckResult.Passed ? UIStrings.Instance.Dialog.Succeeded : UIStrings.Instance.Dialog.Failed), format: lootWindow.SkillCheckResult.Text));
		builder.Append('\n');
		builder.Append(string.Format(lootWindow.SkillCheckValueAgainst.Text, skillCheckResult.RollResult, skillCheckResult.DC));
		builder.Append('\n');
		builder.Append(string.Format(lootWindow.SkillCheckSkillValue.Text, skillCheckResult.TotalSkill));
		return builder.ToString();
	}

	public static UIInteractionType GetUITypeFromActor(InteractionActorType actorType)
	{
		return actorType switch
		{
			InteractionActorType.Default => UIInteractionType.Action, 
			InteractionActorType.Destroy => UIInteractionType.Destroy, 
			InteractionActorType.TechUse => UIInteractionType.TechUse, 
			InteractionActorType.LoreXenos => UIInteractionType.LoreXenos, 
			InteractionActorType.Key => UIInteractionType.Key, 
			InteractionActorType.Unlock => UIInteractionType.Unlock, 
			InteractionActorType.MeltaCharge => UIInteractionType.MeltaCharge, 
			InteractionActorType.Ritual => UIInteractionType.Ritual, 
			_ => throw new ArgumentOutOfRangeException("actorType", actorType, null), 
		};
	}
}
