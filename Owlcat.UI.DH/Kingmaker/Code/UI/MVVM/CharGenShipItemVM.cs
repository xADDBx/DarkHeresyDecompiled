using Kingmaker.Code.View.Bridge.Data;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenShipItemVM : SelectionGroupEntityVM
{
	public readonly ChargenUnit ChargenUnit;

	public readonly string Description;

	public readonly Sprite ShipBigPicture;

	public readonly string Title;

	public CharGenShipItemVM(ChargenUnit chargenUnit)
		: base(allowSwitchOff: false)
	{
		ChargenUnit = chargenUnit;
	}

	protected override void DoSelectMe()
	{
	}
}
