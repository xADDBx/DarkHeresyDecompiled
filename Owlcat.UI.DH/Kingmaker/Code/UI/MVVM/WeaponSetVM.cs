using System;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class WeaponSetVM : SelectionGroupEntityVM
{
	private readonly ReactiveProperty<bool> m_IsEnabled = new ReactiveProperty<bool>(value: true);

	public readonly Action OnSetChange;

	public int Index { get; set; }

	public EquipSlotVM Primary { get; set; }

	public EquipSlotVM Secondary { get; set; }

	public ReadOnlyReactiveProperty<bool> IsEnabled => m_IsEnabled;

	public WeaponSetVM(int index, Action onSetChange)
		: base(allowSwitchOff: false)
	{
		Index = index;
		OnSetChange = onSetChange;
	}

	public WeaponSetVM(int index, EquipSlotVM primary, EquipSlotVM secondary, Action onSetChange)
		: this(index, onSetChange)
	{
		Index = index;
		Primary = primary;
		Secondary = secondary;
	}

	protected override void DoSelectMe()
	{
		OnSetChange?.Invoke();
	}

	public void SetAvailable(bool state)
	{
		SetAvailableState(state);
	}

	public void SetEnabled(bool enabled)
	{
		m_IsEnabled.Value = enabled;
		SetAvailableState(enabled);
	}
}
