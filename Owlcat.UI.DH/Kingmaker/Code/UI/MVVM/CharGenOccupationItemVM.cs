using System;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenOccupationItemVM : CharGenBackgroundBaseItemVM
{
	public CharGenOccupationItemVM(FeatureSelectionItem selectionItem, SelectionStateFeature selectionStateFeature, CharGenPhaseType phaseType, Action<CharGenBackgroundBaseItemVM> onHover = null, LevelUpManager levelUpManager = null)
		: base(selectionItem, selectionStateFeature, phaseType, onHover)
	{
		base.Template = new TooltipTemplateChargenOccupation(selectionItem.Feature, null, levelUpManager);
	}
}
