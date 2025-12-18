using Owlcat.UI;
using UnityEngine;

namespace Assets.Code.View.UI.MVVM;

public class CharInfoSpecializationItemVM : ViewModel
{
	public string Name { get; }

	public Sprite Icon { get; }

	public CharInfoSpecializationItemVM(string name, Sprite icon)
	{
		Name = name;
		Icon = icon;
	}
}
