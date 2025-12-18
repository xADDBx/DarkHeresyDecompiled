using Kingmaker.UI.Common;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharInfoStoriesConsoleView : CharInfoStoriesView, ICharInfoComponentConsoleView, ICharInfoComponentView
{
	[Header("Console")]
	[SerializeField]
	private ScrollRectExtended m_ScrollView;

	[SerializeField]
	private ConsoleHint m_ScrollHint;

	protected override void RefreshView()
	{
		base.RefreshView();
		m_ScrollView.ScrollToTop();
	}

	public void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget)
	{
		if ((bool)m_ScrollHint && m_ScrollView.content.sizeDelta.y >= m_ScrollView.viewport.sizeDelta.y)
		{
			InputBindStruct inputBindStruct = inputLayer.AddAxis(Scroll, 3);
			m_ScrollHint.Bind(inputBindStruct).AddTo(this);
			inputBindStruct.AddTo(this);
		}
	}

	private void Scroll(InputActionEventData obj, float value)
	{
		m_ScrollView.Scroll(value, smooth: true);
	}
}
