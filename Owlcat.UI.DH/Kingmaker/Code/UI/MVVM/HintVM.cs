using Code.Framework.Utility.UnityExtensions;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class HintVM : ViewModel
{
	public readonly string Text;

	public readonly string BindingText;

	public readonly Color Color;

	public HintVM(HintData data)
	{
		Text = data.Text;
		if (!data.BindingName.IsNullOrEmpty())
		{
			BindingText = UIKeyboardTexts.Instance.GetStringByBinding(Game.Instance.Keyboard.GetBindingByName(data.BindingName));
		}
		Color = data.Color;
	}
}
