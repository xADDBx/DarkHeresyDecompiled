using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM.Common;

public class LensSelectorVM : ViewModel
{
	public readonly bool NeedToResetPosition;

	public LensSelectorVM(bool needToResetPosition = true)
	{
		NeedToResetPosition = needToResetPosition;
	}
}
