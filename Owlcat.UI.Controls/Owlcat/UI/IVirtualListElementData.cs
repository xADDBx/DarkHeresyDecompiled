using R3;

namespace Owlcat.UI;

public interface IVirtualListElementData
{
	ReadOnlyReactiveProperty<bool> ActiveInVirtualList { get; }

	ReadOnlyReactiveProperty<bool> IsAvailable { get; }

	ReactiveCommand<Unit> ContentChanged { get; }
}
