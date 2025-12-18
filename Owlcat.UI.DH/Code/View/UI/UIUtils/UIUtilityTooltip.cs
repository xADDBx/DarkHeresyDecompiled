using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;

namespace Code.View.UI.UIUtils;

public static class UIUtilityTooltip
{
	public static string GetTooltipElementLabel(TooltipElement type)
	{
		return UIStrings.Instance.TooltipsElementLabels.GetLabel(type);
	}
}
