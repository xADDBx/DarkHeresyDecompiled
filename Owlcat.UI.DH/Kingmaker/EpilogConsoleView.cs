using Kingmaker.Code.UI.MVVM;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker;

public class EpilogConsoleView : EpilogBaseView
{
	[Header("Hints")]
	[SerializeField]
	private HintView m_Hint;

	[SerializeField]
	private HintView m_ScrollHint;

	protected override void OnBind()
	{
		base.OnBind();
	}

	protected override void OnAnswersChanged()
	{
		(base.ViewModel.Answers.CurrentValue?.FirstOrDefault()?.AnswerRawText ?? string.Empty).Replace(".", string.Empty);
	}

	private void Scroll(float value)
	{
		m_CueScrollRect.Scroll(value * 0.2f, smooth: true);
	}
}
