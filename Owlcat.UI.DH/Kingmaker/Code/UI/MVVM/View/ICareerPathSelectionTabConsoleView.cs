using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public interface ICareerPathSelectionTabConsoleView : ICareerPathSelectionTabView
{
	void AddInput(InputLayer inputLayer, ConsoleHintsWidget hintsWidget);
}
