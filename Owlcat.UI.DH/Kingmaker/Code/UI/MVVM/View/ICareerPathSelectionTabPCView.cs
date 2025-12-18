namespace Kingmaker.Code.UI.MVVM.View;

public interface ICareerPathSelectionTabPCView : ICareerPathSelectionTabView
{
	bool CanCommit { get; }

	void SetButtonsBlock(CareerButtonsBlock buttonsBlock);
}
