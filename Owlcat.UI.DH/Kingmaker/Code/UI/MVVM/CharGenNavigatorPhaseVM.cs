using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenNavigatorPhaseVM : CharGenBackgroundBasePhaseVM<CharGenBackgroundBaseItemVM>
{
	public CharGenNavigatorPhaseVM(CharGenContext charGenContext)
		: base(charGenContext, FeatureGroup.ChargenNavigator, CharGenPhaseType.Navigator, (ReactiveProperty<CharGenPhaseBaseVM>)null)
	{
	}

	protected override CharGenBackgroundBaseItemVM CreateItem(FeatureSelectionItem selectionItem, SelectionStateFeature selectionStateFeature, CharGenPhaseType phaseType)
	{
		return new CharGenNavigatorItemVM(selectionItem, selectionStateFeature, phaseType);
	}
}
