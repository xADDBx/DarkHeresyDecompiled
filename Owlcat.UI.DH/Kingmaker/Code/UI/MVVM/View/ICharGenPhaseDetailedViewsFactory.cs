namespace Kingmaker.Code.UI.MVVM.View;

public interface ICharGenPhaseDetailedViewsFactory
{
	ICharGenPhaseDetailedView GetDetailedPhaseView(CharGenPhaseBaseVM viewModel);
}
