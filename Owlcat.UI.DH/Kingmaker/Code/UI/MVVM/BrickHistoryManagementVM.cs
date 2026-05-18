using Kingmaker.Blueprints.Encyclopedia;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class BrickHistoryManagementVM : TooltipBrickVM
{
	public readonly string Title;

	private readonly ReactiveProperty<bool> m_PreviousButtonInteractable = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_NextButtonInteractable = new ReactiveProperty<bool>();

	public ReadOnlyReactiveProperty<bool> PreviousButtonInteractable => m_PreviousButtonInteractable;

	public ReadOnlyReactiveProperty<bool> NextButtonInteractable => m_NextButtonInteractable;

	public BrickHistoryManagementVM(BlueprintEncyclopediaGlossaryEntry glossaryEntry, BlueprintEncyclopediaEntry encyclopediaEntry)
	{
		Title = encyclopediaEntry?.Title ?? glossaryEntry.Title;
		CheckDirectionButtons();
	}

	public void CheckDirectionButtons()
	{
		m_PreviousButtonInteractable.Value = TooltipHelper.HistoryPointer?.Previous != null;
		m_NextButtonInteractable.Value = TooltipHelper.HistoryPointer?.Next != null;
	}

	public void OnPreviousButtonClick()
	{
		TooltipHelper.GlossaryHistoryPrevious();
	}

	public void OnNextButtonClick()
	{
		TooltipHelper.GlossaryHistoryNext();
	}
}
