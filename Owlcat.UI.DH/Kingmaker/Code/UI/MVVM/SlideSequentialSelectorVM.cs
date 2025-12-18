using System.Collections.Generic;

namespace Kingmaker.Code.UI.MVVM;

public class SlideSequentialSelectorVM : StringSequentialSelectorVM
{
	public SlideSequentialSelectorVM(bool cyclical = true)
		: base(cyclical)
	{
	}

	public SlideSequentialSelectorVM(List<StringSequentialEntity> valueList, StringSequentialEntity current = null, bool cyclical = true)
		: base(valueList, current, cyclical)
	{
	}
}
