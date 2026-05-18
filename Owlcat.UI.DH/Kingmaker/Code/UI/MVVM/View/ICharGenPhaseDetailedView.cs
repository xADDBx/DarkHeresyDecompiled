using Kingmaker.Code.View.Bridge.Root;
using R3;

namespace Kingmaker.Code.UI.MVVM.View;

public interface ICharGenPhaseDetailedView : IInitializable
{
	bool HasYScrollBind { get; }

	bool PressConfirmOnPhase();

	bool PressDeclineOnPhase();

	void Unbind();

	ReadOnlyReactiveProperty<bool> GetCanGoNextOnConfirmProperty();

	ReadOnlyReactiveProperty<bool> CanGoNextInMenuProperty();

	ReadOnlyReactiveProperty<bool> GetCanGoBackOnDeclineProperty();
}
