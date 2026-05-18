using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class WeaponSetBaseView : SelectionGroupEntityView<WeaponSetVM>
{
	[SerializeField]
	protected InventoryEquipSlotView m_PrimaryHand;

	[SerializeField]
	protected InventoryEquipSlotView m_SecondaryHand;

	[SerializeField]
	private TextMeshProUGUI[] m_WeaponSetIndexes;

	[SerializeField]
	private GameObject m_FrameForeground;

	protected override void OnBind()
	{
		base.OnBind();
		m_PrimaryHand.Bind(base.ViewModel.Primary);
		m_SecondaryHand.Bind(base.ViewModel.Secondary);
		UISounds.Instance.SetClickAndHoverSound(m_Button, ButtonSoundsEnum.PlastickSound);
		TextMeshProUGUI[] weaponSetIndexes = m_WeaponSetIndexes;
		for (int i = 0; i < weaponSetIndexes.Length; i++)
		{
			weaponSetIndexes[i].text = UIUtilityText.ArabicToRoman(base.ViewModel.Index + 1);
		}
		base.ViewModel.IsEnabled.Subscribe(delegate(bool value)
		{
			m_FrameForeground.SetActive(value);
			base.gameObject.SetActive(value);
		}).AddTo(this);
	}

	public void RefreshItems()
	{
		m_PrimaryHand.RefreshItem();
		m_SecondaryHand.RefreshItem();
	}
}
