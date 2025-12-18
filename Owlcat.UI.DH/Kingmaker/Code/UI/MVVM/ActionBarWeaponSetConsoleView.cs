using Kingmaker.Code.View.UI.UIUtilities;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ActionBarWeaponSetConsoleView : View<ActionBarPartWeaponSetVM>
{
	[SerializeField]
	private ActionBarSlotWeaponConsoleView m_MainHandWeapon;

	[SerializeField]
	private ActionBarSlotWeaponConsoleView m_OffHandWeapon;

	[SerializeField]
	private TextMeshProUGUI[] m_WeaponSetIndexes;

	protected override void OnBind()
	{
		base.ViewModel.MainHandWeapon.CombineLatest(base.ViewModel.OffHandWeapon, (ItemSlotVM _, ItemSlotVM _) => true).Subscribe(delegate
		{
			if (base.ViewModel.IsTwoHanded)
			{
				m_MainHandWeapon.Bind(base.ViewModel.MainHandWeapon.CurrentValue);
				m_OffHandWeapon.Bind(base.ViewModel.MainHandWeapon.CurrentValue);
			}
			else
			{
				m_MainHandWeapon.Bind(base.ViewModel.MainHandWeapon.CurrentValue);
				m_OffHandWeapon.Bind(base.ViewModel.OffHandWeapon.CurrentValue);
			}
			m_OffHandWeapon.SetFakeMode(base.ViewModel.IsTwoHanded);
		}).AddTo(this);
		TextMeshProUGUI[] weaponSetIndexes = m_WeaponSetIndexes;
		for (int i = 0; i < weaponSetIndexes.Length; i++)
		{
			weaponSetIndexes[i].text = UIUtilityText.ArabicToRoman(base.ViewModel.Index + 1);
		}
	}

	protected override void OnUnbind()
	{
	}
}
