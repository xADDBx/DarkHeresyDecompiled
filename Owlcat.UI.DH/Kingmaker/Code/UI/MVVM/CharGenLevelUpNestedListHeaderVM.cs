using Kingmaker.Blueprints.Facts;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenLevelUpNestedListHeaderVM : CharGenLevelUpSelectorBaseItemVM
{
	private ReactiveProperty<bool> m_HasSelectedInChild = new ReactiveProperty<bool>(value: false);

	private ReactiveProperty<bool> m_IsExpanded = new ReactiveProperty<bool>(value: true);

	public ReadOnlyReactiveProperty<bool> IsExpanded => m_IsExpanded;

	public ReadOnlyReactiveProperty<bool> HasSelectedInChild => m_HasSelectedInChild;

	public CharGenLevelUpNestedListHeaderVM(BlueprintUnitFact source, string customLabel = null, CharGenLevelUpNestedListHeaderVM parenNodeVm = null, int nestingLevel = 0, bool isRecommended = false)
		: base(source, null, parenNodeVm)
	{
		base.NestingLevel = nestingLevel;
		m_IsRecommended.Value = isRecommended;
		if (customLabel != null)
		{
			m_Label.Value = customLabel;
		}
	}

	public void ToggleExpand()
	{
		m_IsExpanded.Value = !m_IsExpanded.Value;
	}

	public void SetExpand(bool value)
	{
		m_IsExpanded.Value = value;
	}

	public void UpdateSelectionFromChild(bool value)
	{
		m_HasSelectedInChild.Value = value;
	}
}
