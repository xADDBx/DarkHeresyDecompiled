using System.Collections.Generic;
using Kingmaker.Items;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class VendorLevelItemsVM : ViewModel
{
	public readonly VendorReputationLevelVM ReputationLevelVM;

	public readonly List<ItemSlotVM> VendorSlots = new List<ItemSlotVM>();

	public readonly bool IsLastList;

	public VendorLevelItemsVM(int level, bool locked, bool isLastList)
	{
		ReputationLevelVM = new VendorReputationLevelVM(level, locked).AddTo(this);
		IsLastList = isLastList;
	}

	public void AddItem(ItemEntity item, int index)
	{
		ItemSlotVM item2 = new ItemSlotVM(item, index).AddTo(this);
		VendorSlots.Add(item2);
	}
}
