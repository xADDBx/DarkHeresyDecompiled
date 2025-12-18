using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenSanctionedPsykerChildPhaseVM : CharGenBackgroundBasePhaseVM<CharGenBackgroundBaseItemVM>
{
	public CharGenSanctionedPsykerChildPhaseVM(CharGenContext charGenContext)
		: base(charGenContext, FeatureGroup.ChargenPsyker, CharGenPhaseType.SanctionedPsyker, (ReactiveProperty<CharGenPhaseBaseVM>)null)
	{
	}

	protected override CharGenBackgroundBaseItemVM CreateItem(FeatureSelectionItem selectionItem, SelectionStateFeature selectionStateFeature, CharGenPhaseType phaseType)
	{
		return new CharGenSanctionedPsykerItemVM(selectionItem, selectionStateFeature, phaseType);
	}
}
