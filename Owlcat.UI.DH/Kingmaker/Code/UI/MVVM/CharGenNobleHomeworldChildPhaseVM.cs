using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenNobleHomeworldChildPhaseVM : CharGenBackgroundBasePhaseVM<CharGenBackgroundBaseItemVM>
{
	public CharGenNobleHomeworldChildPhaseVM(CharGenContext charGenContext, SelectionStateFeature selectionStateFeature, InfoSectionVM infoSectionVM)
		: base(charGenContext, selectionStateFeature, CharGenPhaseType.NobleHomeworldChild, infoSectionVM, (ReactiveProperty<CharGenPhaseBaseVM>)null)
	{
	}

	protected override CharGenBackgroundBaseItemVM CreateItem(FeatureSelectionItem selectionItem, SelectionStateFeature selectionStateFeature, CharGenPhaseType phaseType)
	{
		return new CharGenNobleHomeworldItemVM(selectionItem, selectionStateFeature, phaseType, base.OnHoverItem, m_CharGenContext.LevelUpManager.CurrentValue);
	}
}
