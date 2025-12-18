using System;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CreditsMenuEntityVM : SelectionGroupEntityVM
{
	public readonly Sprite Logo;

	public readonly string Label;

	private readonly Action m_ConfirmAction;

	public CreditsMenuEntityVM(Sprite logo, string label, Action onConfirm)
		: base(allowSwitchOff: false)
	{
		Logo = logo;
		Label = label;
		m_ConfirmAction = onConfirm;
	}

	protected override void DoSelectMe()
	{
		m_ConfirmAction?.Invoke();
	}
}
