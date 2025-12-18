using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoClassEntryVM : ViewModel
{
	public string ClassName { get; }

	public int Level { get; }

	public TooltipBaseTemplate Tooltip { get; }
}
