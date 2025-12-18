using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class VeilThicknessConsoleView : VeilThicknessView, IConsoleNavigationEntity, IConsoleEntity, IHasTooltipTemplate
{
	[Header("Console")]
	[SerializeField]
	private OwlcatMultiButton m_Button;

	public void SetFocus(bool value)
	{
		m_Button.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_Button.IsValid();
	}

	public TooltipBaseTemplate TooltipTemplate()
	{
		return base.ViewModel.Tooltip;
	}
}
