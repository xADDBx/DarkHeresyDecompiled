using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenForgeHomeworldChildPhaseVM : CharGenBackgroundBasePhaseVM<CharGenBackgroundBaseItemVM>
{
	public CharGenForgeHomeworldChildPhaseVM(CharGenContext charGenContext)
		: base(charGenContext, FeatureGroup.ChargenForgeWorld, CharGenPhaseType.ForgeHomeworldChild, (ReactiveProperty<CharGenPhaseBaseVM>)null)
	{
	}

	protected override CharGenBackgroundBaseItemVM CreateItem(FeatureSelectionItem selectionItem, SelectionStateFeature selectionStateFeature, CharGenPhaseType phaseType)
	{
		return new CharGenForgeHomeworldItemVM(selectionItem, selectionStateFeature, phaseType);
	}
}
