using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoLevelClassConsoleView : CharInfoLevelClassScoresPCView, ICharInfoComponentConsoleView, ICharInfoComponentView
{
	public void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget)
	{
		if (m_AdditionalStatsView is InventoryDollAdditionalStatsConsoleView inventoryDollAdditionalStatsConsoleView)
		{
			inventoryDollAdditionalStatsConsoleView.AddInput(ref inputLayer, ref navigationBehaviour, hintsWidget);
		}
	}

	public GridConsoleNavigationBehaviour GetNavigation()
	{
		if (m_AdditionalStatsView is InventoryDollAdditionalStatsConsoleView inventoryDollAdditionalStatsConsoleView)
		{
			return inventoryDollAdditionalStatsConsoleView.GetNavigation();
		}
		return null;
	}
}
