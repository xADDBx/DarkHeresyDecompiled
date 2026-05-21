using System;
using Code.View.UI.MVVM.Tooltip.Templates;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenHomeworldItemVM : CharGenBackgroundBaseItemVM
{
	public CharGenHomeworldItemVM(FeatureSelectionItem selectionItem, SelectionStateFeature selectionStateFeature, CharGenPhaseType phaseType, Action<CharGenBackgroundBaseItemVM> onHover, LevelUpManager levelUpManager)
		: base(selectionItem, selectionStateFeature, phaseType, onHover)
	{
		base.Template = new TooltipTemplateChargenHomeworld(selectionItem.Feature, selectionStateFeature.Blueprint, null, levelUpManager);
	}
}
