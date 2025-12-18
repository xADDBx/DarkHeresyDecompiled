using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public interface IConsoleInputHandler
{
	void AddInputTo(InputLayer inputLayer, ConsoleHintsWidget hintsWidget, GridConsoleNavigationBehaviour ownerBehaviour);

	void UpdateTooltipBrick();
}
