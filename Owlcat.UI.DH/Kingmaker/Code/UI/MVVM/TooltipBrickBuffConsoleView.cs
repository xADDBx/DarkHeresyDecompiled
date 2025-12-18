using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickBuffConsoleView : TooltipBrickBuffView, IConsoleTooltipBrick, IConsoleNavigationEntity, IConsoleEntity, IHasTooltipTemplate, IMonoBehaviour
{
	[SerializeField]
	protected OwlcatMultiButton m_MultiButton;

	public OwlcatMultiButton MultiButton => m_MultiButton;

	public bool IsBinded => base.ViewModel != null;

	public MonoBehaviour MonoBehaviour => this;

	public IConsoleEntity GetConsoleEntity()
	{
		return this;
	}

	public void SetFocus(bool value)
	{
		m_MultiButton.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_MultiButton.IsValid();
	}

	public TooltipBaseTemplate TooltipTemplate()
	{
		return base.ViewModel.Tooltip;
	}
}
