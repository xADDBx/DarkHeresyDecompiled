using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenDeathWorldFeaturePhaseVM : CharGenBackgroundBasePhaseVM<CharGenBackgroundBaseItemVM>
{
	public CharGenDeathWorldFeaturePhaseVM(CharGenContext charGenContext, SelectionStateFeature selectionStateFeature, InfoSectionVM infoSectionVM)
		: base(charGenContext, selectionStateFeature, CharGenPhaseType.DeathHomeworldChild, infoSectionVM, (ReactiveProperty<CharGenPhaseBaseVM>)null)
	{
		base.DisplayMode = CharGenDisplayMode.DollOnly;
	}

	protected override CharGenBackgroundBaseItemVM CreateItem(FeatureSelectionItem selectionItem, SelectionStateFeature selectionStateFeature, CharGenPhaseType phaseType)
	{
		return new CharGenDeathWorldFeatureItemVM(selectionItem, selectionStateFeature, phaseType, base.OnHoverItem, m_CharGenContext.LevelUpManager.CurrentValue);
	}
}
