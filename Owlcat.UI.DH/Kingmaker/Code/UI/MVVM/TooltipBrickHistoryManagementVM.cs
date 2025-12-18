using Kingmaker.Blueprints.Encyclopedia;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickHistoryManagementVM : TooltipBaseBrickVM
{
	public readonly string Title;

	private readonly ReactiveProperty<bool> m_PreviousButtonInteractable = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_NextButtonInteractable = new ReactiveProperty<bool>();

	public ReadOnlyReactiveProperty<bool> PreviousButtonInteractable => m_PreviousButtonInteractable;

	public ReadOnlyReactiveProperty<bool> NextButtonInteractable => m_NextButtonInteractable;

	public TooltipBrickHistoryManagementVM(BlueprintEncyclopediaGlossaryEntry glossaryEntry, BlueprintEncyclopediaEntry encyclopediaEntry)
	{
		Title = encyclopediaEntry?.Title ?? glossaryEntry.Title;
		CheckDirectionButtons();
	}

	public void CheckDirectionButtons()
	{
		m_PreviousButtonInteractable.Value = TooltipHelper.HistoryPointer?.Previous != null;
		m_NextButtonInteractable.Value = TooltipHelper.HistoryPointer?.Next != null;
	}

	public void OnPreviousButtonClick(GridConsoleNavigationBehaviour ownerBehaviour)
	{
		TooltipHelper.GlossaryHistoryPrevious(ownerBehaviour);
	}

	public void OnNextButtonClick(GridConsoleNavigationBehaviour ownerBehaviour)
	{
		TooltipHelper.GlossaryHistoryNext(ownerBehaviour);
	}
}
