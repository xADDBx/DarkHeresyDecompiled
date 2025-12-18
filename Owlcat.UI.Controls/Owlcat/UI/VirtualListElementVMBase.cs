using R3;

namespace Owlcat.UI;

public abstract class VirtualListElementVMBase : VMBase, IVirtualListElementData
{
	protected readonly ReactiveCommand<Unit> m_ContentChanged = new ReactiveCommand<Unit>();

	protected ReactiveProperty<bool> m_IsAvailable = new ReactiveProperty<bool>(value: true);

	public ReactiveProperty<bool> Active = new ReactiveProperty<bool>(value: true);

	public ReadOnlyReactiveProperty<bool> ActiveInVirtualList => Active;

	public ReadOnlyReactiveProperty<bool> IsAvailable => m_IsAvailable;

	public ReactiveCommand<Unit> ContentChanged => m_ContentChanged;

	public bool HasView { get; internal set; }

	protected override void DisposeImplementation()
	{
	}
}
