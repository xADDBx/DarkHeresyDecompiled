using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public sealed class StatusEffectConsoleView : StatusEffectBaseView, IConsoleNavigationEntity, IConsoleEntity, IHasTooltipTemplate
{
	[SerializeField]
	private OwlcatMultiSelectable m_Selectable;

	public void SetFocus(bool value)
	{
		m_Selectable.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_Selectable.IsValid();
	}

	public TooltipBaseTemplate TooltipTemplate()
	{
		return base.ViewModel.Tooltip.CurrentValue;
	}
}
