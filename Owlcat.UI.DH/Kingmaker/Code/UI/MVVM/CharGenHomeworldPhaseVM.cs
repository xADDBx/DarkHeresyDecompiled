using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenHomeworldPhaseVM : CharGenBackgroundBasePhaseVM<CharGenBackgroundBaseItemVM>
{
	public CharGenHomeworldPhaseVM(CharGenContext charGenContext, SelectionStateFeature selectionStateFeature, InfoSectionVM infoSectionVM)
		: base(charGenContext, selectionStateFeature, CharGenPhaseType.Homeworld, infoSectionVM, (ReactiveProperty<CharGenPhaseBaseVM>)null)
	{
		base.DisplayMode = CharGenDisplayMode.DollOnly;
	}

	protected override CharGenBackgroundBaseItemVM CreateItem(FeatureSelectionItem selectionItem, SelectionStateFeature selectionStateFeature, CharGenPhaseType phaseType)
	{
		return new CharGenHomeworldItemVM(selectionItem, selectionStateFeature, phaseType, base.OnHoverItem, m_CharGenContext.LevelUpManager.CurrentValue);
	}
}
