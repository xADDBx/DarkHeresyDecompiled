using System.Collections.Generic;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoAbilityScoresBlockConsoleView : CharInfoAbilityScoresBlockBaseView
{
	public List<CharInfoAbilityScorePCView> AbilityScores => m_StatEntries;

	public GridConsoleNavigationBehaviour GetNavigationBehaviour()
	{
		GridConsoleNavigationBehaviour gridConsoleNavigationBehaviour = new GridConsoleNavigationBehaviour();
		gridConsoleNavigationBehaviour.AddColumn(AbilityScores);
		return gridConsoleNavigationBehaviour;
	}
}
