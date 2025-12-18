using System.Collections.Generic;
using Kingmaker.Blueprints.Items;
using Kingmaker.Code.Framework;
using Kingmaker.Code.Framework.GameLog;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Services;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.View.Bridge.Init;

internal static class CombatLogTooltipServiceInit
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	public static void InitializeCombatLogTooltipServices()
	{
		InitializeCombatLogMessage();
		InitializeTemplates();
		InitializeBricks();
	}

	private static void InitializeCombatLogMessage()
	{
		CombatLogTooltipService.CreateTooltipTemplateCombatLogMessage = (string header, string description, float contentSpacing) => new TooltipTemplateCombatLogMessage(header, description, contentSpacing);
		CombatLogTooltipService.SetTooltipTemplateCombatLogMessageExtraBricks = delegate(TooltipBaseTemplate template, IEnumerable<ITooltipBrick> bricks, bool isInfo)
		{
			if (template is TooltipTemplateCombatLogMessage tooltipTemplateCombatLogMessage)
			{
				if (isInfo)
				{
					tooltipTemplateCombatLogMessage.ExtraInfoBricks = bricks;
				}
				else
				{
					tooltipTemplateCombatLogMessage.ExtraTooltipBricks = bricks;
				}
			}
		};
	}

	private static void InitializeTemplates()
	{
		CombatLogTooltipService.CreateTooltipTemplateItem = (ItemEntity item) => new TooltipTemplateItem(item);
		CombatLogTooltipService.CreateTooltipTemplateItemBlueprint = (BlueprintItem blueprint) => new TooltipTemplateItem(blueprint);
		CombatLogTooltipService.CreateTooltipTemplateAbility = (AbilityData ability) => new TooltipTemplateAbility(ability);
		CombatLogTooltipService.CreateTooltipTemplateToggleAbility = (ToggleAbility ability, MechanicEntity unit) => new TooltipTemplateToggleAbility(ability, unit);
		CombatLogTooltipService.CreateTooltipTemplateBuff = (TooltipTemplateBuffArgs args) => new TooltipTemplateBuff(args.Buff, args.OverrideCaster, args.IsConcentration, args.OverrideIcon, args.OverrideName);
		CombatLogTooltipService.CreateTooltipTemplateFeature = (Feature feature) => new TooltipTemplateFeature(feature);
		CombatLogTooltipService.CreateTooltipTemplateGlossary = (string key) => new TooltipTemplateGlossary(key);
	}

	private static void InitializeBricks()
	{
		CombatLogTooltipService.CreateTooltipBrickText = (string text) => new TooltipBrickText(text);
		CombatLogTooltipService.CreateTooltipBrickIconText = (string text, bool isShowIcon) => new TooltipBrickIconText(text, isShowIcon);
		CombatLogTooltipService.CreateTooltipBrickTextValue = (TooltipBrickTextValueArgs args) => new TooltipBrickTextValue(args.Text, args.Value, args.NestedLevel, args.IsResultValue, args.NeedShowLine);
		CombatLogTooltipService.CreateTooltipBrickIconTextValue = (TooltipBrickIconTextValueArgs args) => new TooltipBrickIconTextValue(args.Name, args.Value, args.NestedLevel, args.IsResultValue, args.ResultValue, args.IsProtectionIcon, args.IsTargetHitIcon, args.IsBorderChanceIcon, args.IsGrayBackground, args.IsBeigeBackground, args.IsRedBackground, args.Tooltip);
		CombatLogTooltipService.CreateTooltipBrickChance = (TooltipBrickChanceArgs args) => new TooltipBrickChance(args.Name, args.SufficientValue, args.CurrentValue, args.NestedLevel, args.IsResultValue, args.ResultValue, args.IsProtectionIcon, args.IsTargetHitIcon, args.IsBorderChanceIcon, args.IsGrayBackground, args.IsBeigeBackground, args.IsRedBackground);
		CombatLogTooltipService.CreateTooltipBrickTriggeredAuto = (TooltipBrickTriggeredAutoArgs args) => new TooltipBrickTriggeredAuto(args.TriggeredAutoText, args.ReasonItems, args.IsSuccess);
		CombatLogTooltipService.CreateTooltipBrickDamageRange = (TooltipBrickDamageRangeArgs args) => new TooltipBrickDamageRange(args.Name, args.CurrentValue, args.MinValue, args.MaxValue, args.NestedLevel, args.IsResultValue, args.ResultValue, args.IsProtectionIcon, args.IsTargetHitIcon, args.IsBorderChanceIcon, args.IsGrayBackground, args.IsBeigeBackground, args.IsRedBackground);
		CombatLogTooltipService.CreateTooltipBrickMinimalAdmissibleDamage = (int minimalAdmissibleDamage, string reasonValue) => new TooltipBrickMinimalAdmissibleDamage(minimalAdmissibleDamage, reasonValue);
		CombatLogTooltipService.CreateTooltipBrickDamageNullifier = (TooltipBrickDamageNullifierArgs args) => new TooltipBrickDamageNullifier(args.ChanceRoll, args.ResultRoll, args.ResultValue, args.ReasonText, args.ReasonItems, args.ResultText);
		CombatLogTooltipService.CreateTooltipBrickNestedMessage = (CombatLogMessage message, bool needShowLine) => new TooltipBrickNestedMessage(message, needShowLine);
		CombatLogTooltipService.CreateTooltipBrickSeparator = (TooltipBrickElementType type) => new TooltipBrickSeparator(type);
		CombatLogTooltipService.CreateTooltipBricksGroupStart = () => new TooltipBricksGroupStart();
		CombatLogTooltipService.CreateTooltipBricksGroupEnd = () => new TooltipBricksGroupEnd();
	}
}
