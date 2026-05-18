using Kingmaker.Blueprints.Root.Strings;

namespace Kingmaker.Code.UI.MVVM;

public class AbilitySelectorWindowConsoleView : SelectorWindowConsoleView<FeatureSelectorSlotConsoleView, FeatureSelectorSlotVM>
{
	protected override void OnBind()
	{
		base.OnBind();
		m_Header.text = UIStrings.Instance.CharacterSheet.ChooseActiveAbilityFeatureGroupHint.Text;
	}
}
