using Kingmaker.Code.Framework.Settings.UISettings;

namespace Kingmaker.Code.UI.MVVM;

public class SettingsEntityDisplayImagesVM : SettingsEntityVM
{
	private UISettingsEntityDisplayImages m_DisplayImagesEntity;

	public SettingsEntityDisplayImagesVM(UISettingsEntityDisplayImages uiSettingsEntity)
		: base(uiSettingsEntity)
	{
		m_DisplayImagesEntity = uiSettingsEntity;
	}
}
