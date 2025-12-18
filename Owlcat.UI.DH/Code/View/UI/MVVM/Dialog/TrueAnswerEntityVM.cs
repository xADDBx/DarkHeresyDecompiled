using Kingmaker.Blueprints.Root;
using Owlcat.UI;
using R3;

namespace Code.View.UI.MVVM.Dialog;

public class TrueAnswerEntityVM : ViewModel
{
	public readonly string Text;

	public readonly int ClueStartId;

	public readonly int ClueEndId;

	private readonly ReactiveProperty<bool> m_AnswerShown = new ReactiveProperty<bool>();

	public ReadOnlyReactiveProperty<bool> AnswerShown => m_AnswerShown;

	public TrueAnswerEntityVM(string decorText, string answerText)
	{
		string text = string.Format(UIConfig.Instance.TextFormats.PlainTextTrueAnswerFormat, decorText);
		Text = text + "\n" + answerText;
		ClueStartId = decorText.Length + 1;
		ClueEndId = decorText.Length + answerText.Length;
	}

	public void ShowAnswer()
	{
		m_AnswerShown.Value = true;
	}
}
