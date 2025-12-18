using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickRankEntrySelectionVM : TooltipBaseBrickVM
{
	public readonly RankEntrySelectionVM RankEntrySelectionVM;

	public TooltipBrickRankEntrySelectionVM(RankEntrySelectionVM rankEntrySelectionVM)
	{
		RankEntrySelectionVM = rankEntrySelectionVM;
	}
}
