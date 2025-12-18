using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Items;
using Kingmaker.Code.Framework;
using Kingmaker.Code.Framework.GameLog;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Owlcat.UI;

namespace Kingmaker.Code.View.Bridge.Services;

public static class CombatLogTooltipService
{
	public static Func<string, string, float, TooltipBaseTemplate> CreateTooltipTemplateCombatLogMessage;

	public static Action<TooltipBaseTemplate, IEnumerable<ITooltipBrick>, bool> SetTooltipTemplateCombatLogMessageExtraBricks;

	public static Func<ItemEntity, TooltipBaseTemplate> CreateTooltipTemplateItem;

	public static Func<BlueprintItem, TooltipBaseTemplate> CreateTooltipTemplateItemBlueprint;

	public static Func<AbilityData, TooltipBaseTemplate> CreateTooltipTemplateAbility;

	public static Func<ToggleAbility, MechanicEntity, TooltipBaseTemplate> CreateTooltipTemplateToggleAbility;

	public static Func<BlueprintAbility, TooltipBaseTemplate> CreateTooltipTemplateAbilityBlueprint;

	public static Func<TooltipTemplateBuffArgs, TooltipBaseTemplate> CreateTooltipTemplateBuff;

	public static Func<Feature, TooltipBaseTemplate> CreateTooltipTemplateFeature;

	public static Func<string, TooltipBaseTemplate> CreateTooltipTemplateGlossary;

	public static Func<string, ITooltipBrick> CreateTooltipBrickText;

	public static Func<string, bool, ITooltipBrick> CreateTooltipBrickIconText;

	public static Func<TooltipBrickTextValueArgs, ITooltipBrick> CreateTooltipBrickTextValue;

	public static Func<TooltipBrickIconTextValueArgs, ITooltipBrick> CreateTooltipBrickIconTextValue;

	public static Func<TooltipBrickChanceArgs, ITooltipBrick> CreateTooltipBrickChance;

	public static Func<TooltipBrickTriggeredAutoArgs, ITooltipBrick> CreateTooltipBrickTriggeredAuto;

	public static Func<TooltipBrickDamageRangeArgs, ITooltipBrick> CreateTooltipBrickDamageRange;

	public static Func<int, string, ITooltipBrick> CreateTooltipBrickMinimalAdmissibleDamage;

	public static Func<TooltipBrickDamageNullifierArgs, ITooltipBrick> CreateTooltipBrickDamageNullifier;

	public static Func<CombatLogMessage, bool, ITooltipBrick> CreateTooltipBrickNestedMessage;

	public static Func<TooltipBrickElementType, ITooltipBrick> CreateTooltipBrickSeparator;

	public static Func<ITooltipBrick> CreateTooltipBricksGroupStart;

	public static Func<ITooltipBrick> CreateTooltipBricksGroupEnd;
}
