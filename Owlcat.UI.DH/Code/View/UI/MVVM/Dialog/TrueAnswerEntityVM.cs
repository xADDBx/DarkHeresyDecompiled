using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Gameplay.Components;
using Kingmaker.UI.Models.Log.GameLogCntxt;
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

	public TrueAnswerEntityVM(DetectiveCasePage casePage)
	{
		using (GameLogContext.Scope)
		{
			GameLogContext.TextStyle = UIConfig.Instance.DefaultTextStyle;
			GameLogContext.Case = casePage.BlueprintCase.MaybeBlueprint;
			string text = casePage.TrueAnswerFlavorText.Text;
			string text2 = casePage.TrueAnswerText.Text;
			string text3 = string.Format(UIConfig.Instance.TextFormats.PlainTextTrueAnswerFormat, text);
			Text = text3 + "\n" + text2;
			ClueStartId = text.Length + 1;
			ClueEndId = ClueStartId + text2.Length;
		}
	}

	public void ShowAnswer()
	{
		m_AnswerShown.Value = true;
	}
}
