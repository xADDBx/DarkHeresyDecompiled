using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenHomeworldPhaseVM : CharGenBackgroundBasePhaseVM<CharGenBackgroundBaseItemVM>
{
	public CharGenHomeworldPhaseVM(CharGenContext charGenContext)
		: base(charGenContext, FeatureGroup.ChargenHomeworld, CharGenPhaseType.Homeworld, (ReactiveProperty<CharGenPhaseBaseVM>)null)
	{
	}

	protected override CharGenBackgroundBaseItemVM CreateItem(FeatureSelectionItem selectionItem, SelectionStateFeature selectionStateFeature, CharGenPhaseType phaseType)
	{
		return new CharGenHomeworldItemVM(selectionItem, selectionStateFeature, phaseType);
	}
}
