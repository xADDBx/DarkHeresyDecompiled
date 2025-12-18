using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public interface ICharGenAppearancePageComponent : IConsoleNavigationEntity, IConsoleEntity
{
	void AddInput(ref InputLayer inputLayer, ConsoleHintsWidget hintsWidget);

	void RemoveInput();
}
