using Kingmaker.UI.Common;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharInfoStoriesConsoleView : CharInfoStoriesView
{
	[Header("Console")]
	[SerializeField]
	private ScrollRectExtended m_ScrollView;

	[SerializeField]
	private HintView m_ScrollHint;

	protected override void RefreshView()
	{
		base.RefreshView();
		m_ScrollView.ScrollToTop();
	}

	public void AddInput()
	{
	}

	private void Scroll(float value)
	{
		m_ScrollView.Scroll(value, smooth: true);
	}
}
