using Kingmaker.Settings;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BookEventAnswerConsoleView : BookEventAnswerView
{
	[SerializeField]
	private float m_DefaultConsoleFontSize = 18f;

	protected override void OnBind()
	{
		base.OnBind();
		m_AnswerText.fontSize = m_DefaultConsoleFontSize * SettingsRoot.Accessiability.FontSizeMultiplier;
	}
}
