using Kingmaker.Code.Framework.Settings.UISettings;

namespace Kingmaker.Code.UI.MVVM;

public class SettingsEntityAccessibilityImageVM : SettingsEntityVM
{
	private UISettingsEntityAccessiabilityImage m_AccessibilityImageEntity;

	public SettingsEntityAccessibilityImageVM(UISettingsEntityAccessiabilityImage uiSettingsEntity)
		: base(uiSettingsEntity)
	{
		m_AccessibilityImageEntity = uiSettingsEntity;
	}
}
