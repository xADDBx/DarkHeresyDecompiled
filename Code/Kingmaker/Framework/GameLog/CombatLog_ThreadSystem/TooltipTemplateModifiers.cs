using System.Collections.Generic;
using Kingmaker.Code.View.Bridge.Services;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Owlcat.UI;

namespace Kingmaker.Framework.GameLog.CombatLog_ThreadSystem;

public sealed class TooltipTemplateModifiers : TooltipBaseTemplate
{
	private readonly TooltipModifiersUtility.ModifiersListDescription _modifiersList;

	private readonly bool _excludeBaseValue;

	public TooltipTemplateModifiers(TooltipModifiersUtility.ModifiersListDescription modifiersList, bool excludeBaseValue)
	{
		_modifiersList = modifiersList;
		_excludeBaseValue = excludeBaseValue;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return CombatLogTooltipService.CreateBrickText(_modifiersList.LocalizedTitle + " = " + _modifiersList.TitleValue);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		foreach (TooltipModifiersUtility.ModifierDescription modifier in _modifiersList)
		{
			if (_excludeBaseValue && modifier.IsBaseValue)
			{
				continue;
			}
			yield return CombatLogTooltipService.CreateBrickIconTextValue(new BrickIconTextValueArgs(modifier.LocalizedName, modifier.Value, 0, isResultValue: true));
			IReadonlyModifiersComposite details = modifier.Details;
			if (details == null || details.List.Count <= 0)
			{
				continue;
			}
			foreach (Modifier item in details.List)
			{
				if (!item.Zero)
				{
					TooltipModifiersUtility.ModifierDescription modifierDescription = new TooltipModifiersUtility.ModifierDescription(item);
					yield return CombatLogTooltipService.CreateBrickIconTextValue(new BrickIconTextValueArgs(modifierDescription.LocalizedName, modifierDescription.Value, 1, isResultValue: true));
				}
			}
		}
	}
}
