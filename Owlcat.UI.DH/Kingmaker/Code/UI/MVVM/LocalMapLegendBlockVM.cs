using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.UI.Common;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class LocalMapLegendBlockVM : ViewModel
{
	public readonly AutoDisposingList<LocalMapLegendBlockItemVM> LocalMapItemsVMs = new AutoDisposingList<LocalMapLegendBlockItemVM>();

	public LocalMapLegendBlockVM()
	{
		AddItems();
	}

	private void AddItems()
	{
		LocalMapItemsVMs.Clear();
		List<LocalMapLegendBlockItemInfo> localMapLegendBlockItemInfo = UIConfig.Instance.BlueprintUILocalMapLegend.LocalMapLegendBlockItemInfo;
		if (localMapLegendBlockItemInfo.Any())
		{
			localMapLegendBlockItemInfo.ForEach(delegate(LocalMapLegendBlockItemInfo block)
			{
				LocalMapItemsVMs.Add(new LocalMapLegendBlockItemVM(block.Sprite.Load(), block.Description));
			});
		}
	}
}
