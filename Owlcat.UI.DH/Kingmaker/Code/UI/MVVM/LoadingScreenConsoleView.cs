using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class LoadingScreenConsoleView : LoadingScreenBaseView
{
	[SerializeField]
	private float m_DefaultConsoleFontTitleSize = 26f;

	[SerializeField]
	private float m_DefaultConsoleFontDescriptionSize = 23f;

	private InputLayer m_InputLayer;

	protected override void OnUnbind()
	{
		m_InputLayer = null;
		base.OnUnbind();
	}

	protected override void SetTextFontSize(float multiplier)
	{
		base.SetTextFontSize(multiplier);
		m_BottomTitleText.fontSize = m_DefaultConsoleFontTitleSize * multiplier;
		m_BottomDescriptionText.fontSize = m_DefaultConsoleFontDescriptionSize * multiplier;
	}

	protected override void ShowUserInputWaiting(bool state)
	{
		if (state)
		{
			m_InputLayer = new InputLayer
			{
				ContextName = "LoadingScreen"
			};
			m_InputLayer.AddButton(delegate
			{
				CloseWait();
			}, 6).AddTo(this);
			m_InputLayer.AddButton(delegate
			{
				CloseWait();
			}, 7).AddTo(this);
			m_InputLayer.AddButton(delegate
			{
				CloseWait();
			}, 5).AddTo(this);
			m_InputLayer.AddButton(delegate
			{
				CloseWait();
			}, 4).AddTo(this);
			GamePad.Instance.PushLayer(m_InputLayer).AddTo(this);
		}
		base.ShowUserInputWaiting(state);
	}
}
