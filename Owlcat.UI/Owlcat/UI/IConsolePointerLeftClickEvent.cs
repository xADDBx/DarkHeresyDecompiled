using R3;

namespace Owlcat.UI;

public interface IConsolePointerLeftClickEvent : IConsoleNavigationEntity, IConsoleEntity
{
	ReactiveCommand<Unit> PointerLeftClickCommand { get; }
}
