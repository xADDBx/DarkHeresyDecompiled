using Kingmaker.RuleSystem.Rules.Damage;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ReasonBuffItemVM : ViewModel
{
	public Sprite Icon;

	public string Name;

	public ReasonBuffItemVM(BuffInformation buff)
	{
		Icon = buff.Icon;
		Name = buff.Name;
	}
}
