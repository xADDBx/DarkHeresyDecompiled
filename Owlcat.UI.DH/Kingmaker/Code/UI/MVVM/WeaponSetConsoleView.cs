using Kingmaker.Code.UI.MVVM.View;

namespace Kingmaker.Code.UI.MVVM;

public class WeaponSetConsoleView : WeaponSetBaseView
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		SetChangeSlotActions(m_PrimaryHand);
		SetChangeSlotActions(m_SecondaryHand);
	}

	private void SetChangeSlotActions(InventoryEquipSlotView slot)
	{
		if (slot is InventoryWeaponSlotConsoleView inventoryWeaponSlotConsoleView)
		{
			inventoryWeaponSlotConsoleView.SetOnWeaponSetChange(delegate
			{
				base.ViewModel.SetSelected(state: true);
			}, delegate
			{
				WeaponSetVM viewModel = base.ViewModel;
				return viewModel != null && !viewModel.IsSelected.Value;
			});
		}
	}
}
