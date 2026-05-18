using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class SpecialWeaponTagVM : ViewModel
{
	public readonly SpecialWeaponDamageType Type;

	public readonly TextValueElement Value;

	public SpecialWeaponTagVM(SpecialWeaponDamageType type, string value)
	{
		Type = type;
		Value = new TextValueElement(UIStrings.Instance.Tooltips.GetWeaponTagLabel(type), value);
	}
}
