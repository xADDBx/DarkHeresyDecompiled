using System;
using System.Collections.Generic;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class ChargenProgressionHeaderVM : ViewModel
{
	private List<ChargenProgressionHeaderLevelVM> m_Levels = new List<ChargenProgressionHeaderLevelVM>();

	public IReadOnlyList<ChargenProgressionHeaderLevelVM> Levels => m_Levels;

	public ReadOnlyReactiveProperty<int> CurrentLevel { get; private set; }

	public ReadOnlyReactiveProperty<int> LastFinishedRank { get; private set; }

	public ReadOnlyReactiveProperty<int> HoveredLevel { get; private set; }

	private event Action m_ToggleOpen;

	public ChargenProgressionHeaderVM(ReadOnlyReactiveProperty<int> currentLevel, ReadOnlyReactiveProperty<int> lastFinishedRank, ReadOnlyReactiveProperty<int> hoveredLevel, int levelCount, Action toggleOpen)
	{
		CurrentLevel = currentLevel;
		LastFinishedRank = lastFinishedRank;
		HoveredLevel = hoveredLevel;
		this.m_ToggleOpen = toggleOpen;
		for (int i = 1; i <= levelCount; i++)
		{
			m_Levels.Add(new ChargenProgressionHeaderLevelVM(i, LastFinishedRank, HoveredLevel));
		}
	}

	public void ToggleOpen()
	{
		this.m_ToggleOpen?.Invoke();
	}
}
