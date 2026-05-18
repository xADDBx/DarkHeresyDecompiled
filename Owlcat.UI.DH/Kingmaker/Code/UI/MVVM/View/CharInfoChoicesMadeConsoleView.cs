using Kingmaker.UI.Common;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharInfoChoicesMadeConsoleView : CharInfoChoicesMadeView
{
	[Header("Console")]
	[SerializeField]
	private ScrollRectExtended m_ScrollView;

	[SerializeField]
	private HintView m_ScrollHint;

	public void AddInput()
	{
	}

	private void Scroll(float value)
	{
		m_ScrollView.Scroll(value, smooth: true);
	}
}
