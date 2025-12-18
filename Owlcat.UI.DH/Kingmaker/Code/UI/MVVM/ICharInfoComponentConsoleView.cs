using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public interface ICharInfoComponentConsoleView : ICharInfoComponentView
{
	void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget);
}
