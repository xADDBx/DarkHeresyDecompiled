using Owlcat.UI;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class CaseCardInfoEntityVM : ViewModel
{
	public readonly string Text;

	public readonly CardState State;

	public CaseCardInfoEntityVM(string text, CardState state)
	{
		Text = text;
		State = state;
	}
}
