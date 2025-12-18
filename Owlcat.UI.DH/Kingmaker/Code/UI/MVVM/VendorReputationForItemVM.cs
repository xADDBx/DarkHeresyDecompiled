using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class VendorReputationForItemVM : VirtualListElementVMBase
{
	public ItemsItemOrigin Type;

	public Sprite TypeIcon;

	public string TypeLabel;

	public int ReputationCost;

	public VendorReputationForItemVM(ItemsItemOrigin type, int reputationCost)
	{
		ReputationCost = reputationCost;
		TypeLabel = UIStrings.Instance.lootTypeTexts.GetLabelByOrigin(type);
		TypeIcon = UIConfig.Instance.UIIcons.CargoIcons.GetIconByOrigin(type);
	}

	protected override void DisposeImplementation()
	{
	}
}
