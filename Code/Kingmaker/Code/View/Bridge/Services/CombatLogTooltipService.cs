using System;
using System.Collections.Generic;
using Kingmaker.Code.Framework;
using Kingmaker.Code.Framework.GameLog;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Owlcat.UI;

namespace Kingmaker.Code.View.Bridge.Services;

public static class CombatLogTooltipService
{
	public static Func<string, string, float, TooltipBaseTemplate> CreateTooltipTemplateCombatLogMessage;

	public static Action<TooltipBaseTemplate, IEnumerable<ITooltipBrick>, bool> SetTooltipTemplateCombatLogMessageExtraBricks;

	public static Func<ItemEntity, TooltipBaseTemplate> CreateTooltipTemplateItem;

	public static Func<ItemEntity, MechanicEntity, TooltipBaseTemplate> CreateTooltipTemplateItemForLog;

	public static Func<AbilityData, TooltipBaseTemplate> CreateTooltipTemplateAbility;

	public static Func<AbilityData, TooltipBaseTemplate> CreateTooltipTemplateAbilityForActionBar;

	public static Func<ToggleAbility, MechanicEntity, TooltipBaseTemplate> CreateTooltipTemplateToggleAbility;

	public static Func<BlueprintAbility, TooltipBaseTemplate> CreateTooltipTemplateAbilityBlueprint;

	public static Func<TooltipTemplateBuffArgs, TooltipBaseTemplate> CreateTooltipTemplateBuff;

	public static Func<Feature, TooltipBaseTemplate> CreateTooltipTemplateFeature;

	public static Func<BlueprintMechanicEntityFact, TooltipBaseTemplate> CreateTooltipTemplateMechanicEntityFact;

	public static Func<string, TooltipBaseTemplate> CreateTooltipTemplateGlossary;

	public static Func<string, ITooltipBrick> CreateBrickText;

	public static Func<string, bool, ITooltipBrick> CreateBrickIconText;

	public static Func<TooltipBrickTextValueArgs, ITooltipBrick> CreateBrickTextValue;

	public static Func<BrickIconTextValueArgs, ITooltipBrick> CreateBrickIconTextValue;

	public static Func<BrickChanceArgs, ITooltipBrick> CreateBrickChance;

	public static Func<TooltipBrickTriggeredAutoArgs, ITooltipBrick> CreateBrickTriggeredAuto;

	public static Func<BrickDamageRangeArgs, ITooltipBrick> CreateBrickDamageRange;

	public static Func<int, string, ITooltipBrick> CreateBrickMinimalAdmissibleDamage;

	public static Func<TooltipBrickDamageNullifierArgs, ITooltipBrick> CreateBrickDamageNullifier;

	public static Func<CombatLogMessage, bool, ITooltipBrick> CreateBrickNestedMessage;

	public static Func<TooltipBrickElementType, ITooltipBrick> CreateBrickSeparator;

	public static Func<IReadOnlyList<ITooltipBrick>, ITooltipBrick> CreateBricksGroupOneColumn;
}
