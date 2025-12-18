using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class LoadingScreenPCView : LoadingScreenBaseView
{
	[SerializeField]
	private float m_DefaultFontTitleSize = 26f;

	[SerializeField]
	private float m_DefaultFontDescriptionSize = 23f;

	protected override void SetTextFontSize(float multiplier)
	{
		base.SetTextFontSize(multiplier);
		m_BottomTitleText.fontSize = m_DefaultFontTitleSize * multiplier;
		m_BottomDescriptionText.fontSize = m_DefaultFontDescriptionSize * multiplier;
	}
}
