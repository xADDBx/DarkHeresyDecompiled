using System;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class ExpandableTitleVM : VirtualListElementVMBase
{
	public readonly string Title;

	private readonly Action<bool> m_Switch;

	private readonly ReactiveProperty<bool> m_IsExpanded = new ReactiveProperty<bool>();

	public bool IsSwitchable => m_Switch != null;

	public ReadOnlyReactiveProperty<bool> IsExpanded => m_IsExpanded;

	public ExpandableTitleVM(string title, Action<bool> @switch, bool defaultExpanded = true)
	{
		Title = title;
		m_Switch = @switch;
		if (defaultExpanded)
		{
			Expand();
		}
		else
		{
			Collapse();
		}
	}

	protected override void DisposeImplementation()
	{
	}

	public void Expand()
	{
		m_IsExpanded.Value = true;
		m_Switch?.Invoke(IsExpanded.CurrentValue);
	}

	public void Collapse()
	{
		m_IsExpanded.Value = false;
		m_Switch?.Invoke(IsExpanded.CurrentValue);
	}

	public void Switch()
	{
		m_IsExpanded.Value = !IsExpanded.CurrentValue;
		m_Switch?.Invoke(IsExpanded.CurrentValue);
	}
}
