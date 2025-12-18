using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenMomentOfTriumphPhaseVM : CharGenBackgroundBasePhaseVM<CharGenBackgroundBaseItemVM>
{
	public CharGenMomentOfTriumphPhaseVM(CharGenContext charGenContext)
		: base(charGenContext, FeatureGroup.ChargenMomentOfTriumph, CharGenPhaseType.MomentOfTriumph, (ReactiveProperty<CharGenPhaseBaseVM>)null)
	{
	}

	protected override CharGenBackgroundBaseItemVM CreateItem(FeatureSelectionItem selectionItem, SelectionStateFeature selectionStateFeature, CharGenPhaseType phaseType)
	{
		return new CharGenMomentOfTriumphItemVM(selectionItem, selectionStateFeature, phaseType);
	}
}
