using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class AbilitySelectorWindowConsoleView : SelectorWindowConsoleView<FeatureSelectorSlotConsoleView, FeatureSelectorSlotVM>
{
	protected override void OnBind()
	{
		base.OnBind();
		m_Header.text = UIStrings.Instance.CharacterSheet.ChooseActiveAbilityFeatureGroupHint.Text;
	}

	protected override void EntityFocused(IConsoleEntity entity)
	{
		base.EntityFocused(entity);
		if (entity != null)
		{
			FeatureSelectorSlotVM obj = (entity as FeatureSelectorSlotConsoleView)?.ViewModel;
			(base.ViewModel as AbilitySelectorWindowVM)?.OnFeatureFocused?.Invoke(obj);
		}
	}
}
