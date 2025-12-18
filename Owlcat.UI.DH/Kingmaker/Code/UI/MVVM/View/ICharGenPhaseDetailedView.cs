using Kingmaker.Code.View.Bridge.Root;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM.View;

public interface ICharGenPhaseDetailedView : IInitializable
{
	bool HasYScrollBind { get; }

	bool PressConfirmOnPhase();

	bool PressDeclineOnPhase();

	void Unbind();

	void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget, ReadOnlyReactiveProperty<bool> isMainCharacter);

	ReadOnlyReactiveProperty<bool> GetCanGoNextOnConfirmProperty();

	ReadOnlyReactiveProperty<bool> CanGoNextInMenuProperty();

	ReadOnlyReactiveProperty<bool> GetCanGoBackOnDeclineProperty();
}
