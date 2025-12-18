using Owlcat.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class SettingsEntityAccessibilityImageConsoleView : VirtualListElementViewBase<SettingsEntityAccessibilityImageVM>, IConsoleNavigationEntity, IConsoleEntity
{
	[SerializeField]
	private Image m_AccessibilityImage;

	[SerializeField]
	private VirtualListLayoutElementSettings m_LayoutSettings;

	public override VirtualListLayoutElementSettings LayoutSettings => m_LayoutSettings;

	protected override void BindViewImplementation()
	{
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void SetFocus(bool value)
	{
	}

	public bool IsValid()
	{
		return false;
	}
}
