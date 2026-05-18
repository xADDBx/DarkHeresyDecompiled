using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class LoadingScreenConsoleView : LoadingScreenBaseView
{
	[SerializeField]
	private float m_DefaultConsoleFontTitleSize = 26f;

	[SerializeField]
	private float m_DefaultConsoleFontDescriptionSize = 23f;

	protected override void SetTextFontSize(float multiplier)
	{
		base.SetTextFontSize(multiplier);
		m_BottomTitleText.fontSize = m_DefaultConsoleFontTitleSize * multiplier;
		m_BottomDescriptionText.fontSize = m_DefaultConsoleFontDescriptionSize * multiplier;
	}

	protected override void ShowUserInputWaiting(bool state)
	{
		base.ShowUserInputWaiting(state);
	}
}
