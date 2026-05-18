using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.Framework;
using Kingmaker.Code.Framework.GameLog;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Services;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;
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
		CombatLogTooltipService.CreateTooltipTemplateItemForLog = delegate(ItemEntity item, MechanicEntity wielder)
		{
			ItemTooltipData itemTooltipData = UIUtilityItem.GetItemTooltipData(item, replenishing: false, wielder);
			return new TooltipTemplateItem(item.Blueprint, itemTooltipData);
		};
		CombatLogTooltipService.CreateTooltipTemplateAbility = (AbilityData ability) => new TooltipTemplateAbility(ability);
		CombatLogTooltipService.CreateTooltipTemplateAbilityForActionBar = (AbilityData ability) => new TooltipTemplateAbility(ability, showModifiedAPCost: true);
		CombatLogTooltipService.CreateTooltipTemplateToggleAbility = (ToggleAbility ability, MechanicEntity unit) => new TooltipTemplateToggleAbility(ability, unit);
		CombatLogTooltipService.CreateTooltipTemplateBuff = (TooltipTemplateBuffArgs args) => args.IsConcentration ? ((TooltipBaseTemplate)new TooltipTemplateConcentrationBuff(args.Buff, args.OverrideCaster, args.OverrideIcon, args.OverrideName)) : ((TooltipBaseTemplate)new TooltipTemplateBuff(args.Buff, args.OverrideCaster, args.OverrideIcon, args.OverrideName));
		CombatLogTooltipService.CreateTooltipTemplateFeature = (Feature feature) => new TooltipTemplateFeature(feature);
		CombatLogTooltipService.CreateTooltipTemplateMechanicEntityFact = delegate(BlueprintMechanicEntityFact blueprint)
		{
			if (blueprint is BlueprintBuff blueprintBuff)
			{
				return new TooltipTemplateBuff(blueprintBuff);
			}
			return (blueprint is BlueprintFeatureBase feature2) ? new TooltipTemplateFeature(feature2) : null;
		};
		CombatLogTooltipService.CreateTooltipTemplateGlossary = (string key) => new TooltipTemplateGlossary(key);
	}

	private static void InitializeBricks()
	{
		CombatLogTooltipService.CreateBrickText = (string text) => new BrickTextVM(text);
		CombatLogTooltipService.CreateBrickIconText = (string text, bool isShowIcon) => new BrickIconTextVM(text, isShowIcon);
		CombatLogTooltipService.CreateBrickTextValue = (TooltipBrickTextValueArgs args) => new BrickTextValueVM(args.Text, args.Value, args.NestedLevel, args.IsResultValue, args.NeedShowLine);
		CombatLogTooltipService.CreateBrickIconTextValue = (BrickIconTextValueArgs args) => new BrickIconTextValueVM(args.Name, args.Value, args.NestedLevel, args.IsResultValue, args.ResultValue, args.IconType, args.Palette, args.Tooltip);
		CombatLogTooltipService.CreateBrickChance = (BrickChanceArgs args) => new BrickChanceVM(args.Name, args.SufficientValue, args.CurrentValue, args.NestedLevel, args.IsResultValue, args.ResultValue, args.IconType, args.Palette);
		CombatLogTooltipService.CreateBrickTriggeredAuto = (TooltipBrickTriggeredAutoArgs args) => new BrickTriggeredAutoVM(args.TriggeredAutoText, args.ReasonItems, args.IsSuccess);
		CombatLogTooltipService.CreateBrickDamageRange = (BrickDamageRangeArgs args) => new BrickDamageRangeVM(args.Name, args.CurrentValue, args.MinValue, args.MaxValue, args.NestedLevel, args.IsResultValue, args.ResultValue, args.IconType, args.Palette);
		CombatLogTooltipService.CreateBrickMinimalAdmissibleDamage = (int minimalAdmissibleDamage, string reasonValue) => new BrickMinimalAdmissibleDamageVM(minimalAdmissibleDamage, reasonValue);
		CombatLogTooltipService.CreateBrickDamageNullifier = (TooltipBrickDamageNullifierArgs args) => new BrickDamageNullifierVM(args.ChanceRoll, args.ResultRoll, args.ResultValue, args.ReasonText, args.ReasonItems, args.ResultText);
		CombatLogTooltipService.CreateBrickNestedMessage = (CombatLogMessage message, bool needShowLine) => new BrickNestedMessageVM(message, needShowLine);
		CombatLogTooltipService.CreateBrickSeparator = (TooltipBrickElementType type) => new BrickSeparatorVM(type);
		CombatLogTooltipService.CreateBricksGroupOneColumn = (IReadOnlyList<ITooltipBrick> children) => new BricksGroupOneColumnVM(new List<TooltipBrickVM>(children.Cast<TooltipBrickVM>()));
	}
}
