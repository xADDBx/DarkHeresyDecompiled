using Kingmaker.GameCommands;
using Owlcat.UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.UI.MVVM;

public class VendorGenericSlotView<TSlot> : ItemSlotView<ItemSlotVM> where TSlot : ItemSlotBaseView
{
	[SerializeField]
	protected TSlot m_ItemSlotView;

	[FormerlySerializedAs("m_VendorSlotSelectable")]
	[SerializeField]
	protected OwlcatMultiButton m_VendorSlotButton;

	protected void OnClick()
	{
		Game.Instance.GameCommandQueue.AddForBuyVendor(base.ViewModel.ItemEntity, 1);
	}

	protected void OnDoubleClick()
	{
		base.ViewModel.VendorTryBuyAll();
	}
}
