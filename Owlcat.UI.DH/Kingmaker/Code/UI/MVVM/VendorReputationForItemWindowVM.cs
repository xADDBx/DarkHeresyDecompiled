using System.Collections.Generic;
using System.Linq;
using Kingmaker.Gameplay.Features.Vendor;
using Kingmaker.Items;
using Kingmaker.UI.Common;
using ObservableCollections;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class VendorReputationForItemWindowVM : ViewModel
{
	public readonly ObservableList<VendorReputationForItemVM> AcceptItems = new ObservableList<VendorReputationForItemVM>();

	private TradeLogic Vendor = VendorHelper.TradeLogic;

	public VendorReputationForItemWindowVM(List<ItemsItemOrigin> types)
	{
		foreach (KeyValuePair<ItemsItemOrigin, int> item in new Dictionary<ItemsItemOrigin, int>().OrderBy(delegate(KeyValuePair<ItemsItemOrigin, int> item)
		{
			KeyValuePair<ItemsItemOrigin, int> keyValuePair = item;
			return keyValuePair.Value;
		}).Reverse())
		{
			AcceptItems.Add(new VendorReputationForItemVM(item.Key, item.Value));
		}
	}

	protected override void OnDispose()
	{
		AcceptItems.Clear();
	}
}
