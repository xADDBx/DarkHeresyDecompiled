namespace Kingmaker.Code.UI.MVVM;

public class BrickRankEntrySelectionVM : TooltipBrickVM
{
	public readonly RankEntrySelectionVM RankEntrySelectionVM;

	public BrickRankEntrySelectionVM(RankEntrySelectionVM rankEntrySelectionVM)
	{
		RankEntrySelectionVM = rankEntrySelectionVM;
	}
}
