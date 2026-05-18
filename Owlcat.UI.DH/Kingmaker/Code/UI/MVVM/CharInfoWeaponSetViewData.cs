namespace Kingmaker.Code.UI.MVVM;

public sealed class CharInfoWeaponSetViewData
{
	public readonly CharInfoWeaponSetVM WeaponSetVM;

	public readonly bool IsPrimary;

	public CharInfoWeaponSetViewData(CharInfoWeaponSetVM vm, bool isPrimary)
	{
		WeaponSetVM = vm;
		IsPrimary = isPrimary;
	}
}
