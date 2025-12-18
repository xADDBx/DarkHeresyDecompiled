using Kingmaker.Code.UI.MVVM.View;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickCalculatedFormulaConsoleView : TooltipBrickCalculatedFormulaView, IConsoleTooltipBrick, IConsoleNavigationEntity, IConsoleEntity, IMonoBehaviour
{
	[Header("Console")]
	[SerializeField]
	protected OwlcatMultiButton m_MultiButton;

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
}
