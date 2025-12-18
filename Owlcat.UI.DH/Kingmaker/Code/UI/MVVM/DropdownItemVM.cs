using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class DropdownItemVM : ViewModel
{
	public readonly string Text;

	public readonly Sprite Icon;

	public DropdownItemVM(string text, Sprite icon = null)
	{
		Text = text;
		Icon = icon;
	}
}
