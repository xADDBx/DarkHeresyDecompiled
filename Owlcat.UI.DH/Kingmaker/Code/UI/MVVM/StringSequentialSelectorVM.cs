using System.Collections.Generic;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class StringSequentialSelectorVM : SequentialSelectorVM<StringSequentialEntity>
{
	private readonly ReactiveProperty<string> m_SecondaryValue = new ReactiveProperty<string>();

	private readonly ReactiveProperty<string> m_Value = new ReactiveProperty<string>();

	public ReadOnlyReactiveProperty<string> Value => m_Value;

	public ReadOnlyReactiveProperty<string> SecondaryValue => m_SecondaryValue;

	public StringSequentialSelectorVM(bool cyclical = true)
		: base(cyclical)
	{
	}

	public StringSequentialSelectorVM(List<StringSequentialEntity> valueList, StringSequentialEntity current = null, bool cyclical = true)
		: base(valueList, current, cyclical)
	{
	}

	protected override void SetCurrentEntity()
	{
		m_Value.Value = ValueList[base.CurrentIndex.CurrentValue].Title;
		m_SecondaryValue.Value = ValueList[base.CurrentIndex.CurrentValue].Description;
	}
}
