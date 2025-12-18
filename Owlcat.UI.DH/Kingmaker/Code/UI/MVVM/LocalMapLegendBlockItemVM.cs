using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class LocalMapLegendBlockItemVM : ViewModel
{
	public readonly Sprite ItemSprite;

	public readonly string ItemLabel;

	public LocalMapLegendBlockItemVM(Sprite itemSprite, string itemLabel)
	{
		ItemSprite = itemSprite;
		ItemLabel = itemLabel;
	}
}
