using JetBrains.Annotations;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public abstract class NestedSelectionGroupEntityVM : VirtualListElementVMBase, IVirtualListElementIdentifier
{
	private readonly ReactiveProperty<bool> m_IsExpanded = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveCommand<Unit> m_RefreshView = new ReactiveCommand<Unit>();

	private readonly ReactiveProperty<bool> m_IsSelected = new ReactiveProperty<bool>();

	public readonly bool AllowSwitchOff;

	public int VirtualListTypeId { get; }

	public abstract INestedListSource NextSource { get; }

	public INestedListSource Source { get; }

	public abstract bool HasNesting { get; }

	public abstract int NestingLimit { get; }

	public ReadOnlyReactiveProperty<bool> IsExpanded => m_IsExpanded;

	public Observable<Unit> RefreshView => m_RefreshView;

	public ReadOnlyReactiveProperty<bool> IsSelected => m_IsSelected;

	public bool IsHidden { get; private set; }

	public NestedSelectionGroupEntityVM([NotNull] INestedListSource source, bool allowSwitchOff)
	{
		AllowSwitchOff = allowSwitchOff;
		int num = 0;
		INestedListSource nestedListSource = source;
		while (nestedListSource.Source != null && num <= NestingLimit)
		{
			num++;
			nestedListSource = nestedListSource.Source;
		}
		VirtualListTypeId = num;
		Source = source;
	}

	public void SetExpanded(bool state)
	{
		if (HasNesting)
		{
			m_IsExpanded.Value = state;
		}
	}

	public void SetSelected(bool state)
	{
		m_IsSelected.Value = state;
		m_RefreshView.Execute(Unit.Default);
		if (state)
		{
			DoSelectMe();
		}
	}

	public void SetSelectedFromView(bool state)
	{
		if (state || AllowSwitchOff)
		{
			SetSelected(state);
		}
	}

	protected abstract void DoSelectMe();

	protected void SetAvailableState(bool state)
	{
		m_IsAvailable.Value = state;
	}

	protected void SetHiddenState(bool state)
	{
		IsHidden = state;
	}

	protected override void DisposeImplementation()
	{
	}
}
