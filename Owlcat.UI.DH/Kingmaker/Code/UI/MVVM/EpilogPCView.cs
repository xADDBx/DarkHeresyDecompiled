using System.Linq;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class EpilogPCView : EpilogBaseView
{
	[SerializeField]
	protected OwlcatButton m_ContinueButton;

	[SerializeField]
	protected TextMeshProUGUI m_ContinueButtonTitle;

	protected override void OnBind()
	{
		base.OnBind();
		Game.Instance.Keyboard.Bind("NextOrEnd", Confirm).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_ContinueButton.OnLeftClickAsObservable(), delegate
		{
			Confirm();
		}).AddTo(this);
	}

	protected override void OnAnswersChanged()
	{
		string text = (base.ViewModel.Answers.CurrentValue?.FirstOrDefault()?.AnswerRawText ?? string.Empty).Replace(".", string.Empty);
		m_ContinueButtonTitle.text = text;
	}
}
