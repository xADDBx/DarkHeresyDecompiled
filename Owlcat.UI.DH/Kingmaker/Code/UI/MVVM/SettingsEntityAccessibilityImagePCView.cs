using Owlcat.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class SettingsEntityAccessibilityImagePCView : SettingsEntityView<SettingsEntityAccessibilityImageVM>
{
	[SerializeField]
	private Image m_AccessibilityImage;

	[SerializeField]
	private VirtualListLayoutElementSettings m_LayoutSettings;

	public override VirtualListLayoutElementSettings LayoutSettings => m_LayoutSettings;
}
