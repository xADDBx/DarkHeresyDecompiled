using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenImperialHomeworldChildPhaseVM : CharGenBackgroundBasePhaseVM<CharGenBackgroundBaseItemVM>
{
	public CharGenImperialHomeworldChildPhaseVM(CharGenContext charGenContext)
		: base(charGenContext, FeatureGroup.ChargenImperialWorld, CharGenPhaseType.ImperialHomeworldChild, (ReactiveProperty<CharGenPhaseBaseVM>)null)
	{
	}

	protected override CharGenBackgroundBaseItemVM CreateItem(FeatureSelectionItem selectionItem, SelectionStateFeature selectionStateFeature, CharGenPhaseType phaseType)
	{
		return new CharGenImperialHomeworldItemVM(selectionItem, selectionStateFeature, phaseType);
	}
}
