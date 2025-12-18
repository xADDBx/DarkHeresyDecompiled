using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class VendorReputationLevelConsoleView : VendorReputationLevelView, IConsoleNavigationEntity, IConsoleEntity
{
	[SerializeField]
	private OwlcatMultiButton m_FocusButton;

	public void SetFocus(bool value)
	{
		m_FocusButton.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_Button.IsValid();
	}
}
