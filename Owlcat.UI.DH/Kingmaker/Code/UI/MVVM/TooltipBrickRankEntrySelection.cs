using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickRankEntrySelection : ITooltipBrick
{
	private readonly RankEntrySelectionVM m_RankEntrySelectionVM;

	public TooltipBrickRankEntrySelection(RankEntrySelectionVM rankEntrySelectionVM)
	{
		m_RankEntrySelectionVM = rankEntrySelectionVM;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickRankEntrySelectionVM(m_RankEntrySelectionVM);
	}
}
