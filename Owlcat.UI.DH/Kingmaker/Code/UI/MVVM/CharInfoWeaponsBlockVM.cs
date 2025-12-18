using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoWeaponsBlockVM : CharInfoComponentVM
{
	public readonly List<CharInfoWeaponSetVM> WeaponSets = new List<CharInfoWeaponSetVM>();

	public CharInfoWeaponsBlockVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit)
		: base(unit)
	{
		Disposable.Create(DisposeImplementation).AddTo(this);
	}

	private void DisposeImplementation()
	{
		ClearSets();
	}

	protected override void RefreshData()
	{
		ClearSets();
		CreateSets();
	}

	private void CreateSets()
	{
		for (int i = 0; i < 2; i++)
		{
			HandsEquipmentSet handsEquipmentSet = Unit.CurrentValue.Body.HandsEquipmentSets[i];
			WeaponSets.Add(handsEquipmentSet.IsEmpty() ? null : new CharInfoWeaponSetVM(handsEquipmentSet, Unit.CurrentValue));
		}
	}

	private void ClearSets()
	{
		WeaponSets.ForEach(delegate(CharInfoWeaponSetVM vm)
		{
			vm?.Dispose();
		});
		WeaponSets.Clear();
	}
}
