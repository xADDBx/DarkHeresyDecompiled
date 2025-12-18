using Kingmaker.Code.UI.MVVM;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;

namespace Kingmaker;

public class EpilogConsoleView : EpilogBaseView
{
	[Header("Hints")]
	[SerializeField]
	private ConsoleHint m_Hint;

	[SerializeField]
	private ConsoleHint m_ScrollHint;

	private InputLayer m_InputLayer;

	protected override void OnBind()
	{
		base.OnBind();
		m_InputLayer = InputLayer.FromView(this);
		GamePad.Instance.PushLayer(m_InputLayer).AddTo(this);
		m_Hint.Bind(m_InputLayer.AddButton(delegate
		{
			Confirm();
		}, 8)).AddTo(this);
		m_ScrollHint.Bind(m_InputLayer.AddAxis(Scroll, 3)).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_InputLayer = null;
	}

	protected override void OnAnswersChanged()
	{
		string label = (base.ViewModel.Answers.CurrentValue?.FirstOrDefault()?.AnswerRawText ?? string.Empty).Replace(".", string.Empty);
		m_Hint.SetLabel(label);
	}

	private void Scroll(InputActionEventData obj, float value)
	{
		m_CueScrollRect.Scroll(value * 0.2f, smooth: true);
	}
}
