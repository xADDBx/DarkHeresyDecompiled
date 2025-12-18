using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.Utility.UniRxExtensions;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class SettingsEntityBoolVM : SettingsEntityWithValueVM
{
	private readonly UISettingsEntityBool m_UISettingsEntity;

	public readonly ReadOnlyReactiveProperty<bool> TempValue;

	public SettingsEntityBoolVM(UISettingsEntityBool uiSettingsEntity, bool hideMarkImage = false)
		: base(uiSettingsEntity, hideMarkImage)
	{
		m_UISettingsEntity = uiSettingsEntity;
		AddDisposable(TempValue = m_UISettingsEntity.Setting.ObserveTempValue());
	}

	public bool GetTempValue()
	{
		return m_UISettingsEntity.Setting.GetTempValue();
	}

	public void SetTempValue(bool value)
	{
		if (base.ModificationAllowed.CurrentValue)
		{
			m_UISettingsEntity.Setting.SetTempValue(value);
		}
	}

	public void ChangeValue()
	{
		SetTempValue(!GetTempValue());
	}
}
