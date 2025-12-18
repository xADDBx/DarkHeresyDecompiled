using System;
using Kingmaker.RuleSystem.Rules.Damage;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ReasonBuffItemVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public Sprite Icon;

	public string Name;

	public ReasonBuffItemVM(BuffInformation buff)
	{
		Icon = buff.Icon;
		Name = buff.Name;
	}

	protected override void DisposeImplementation()
	{
	}
}
