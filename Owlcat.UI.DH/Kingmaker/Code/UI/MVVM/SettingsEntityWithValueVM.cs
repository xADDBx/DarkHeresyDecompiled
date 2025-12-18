using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.Utility.UniRxExtensions;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public abstract class SettingsEntityWithValueVM : SettingsEntityVM
{
	public readonly ReadOnlyReactiveProperty<bool> IsChanged;

	private readonly IUISettingsEntityWithValueBase m_UISettingsEntity;

	private readonly ReactiveProperty<bool> m_ModificationAllowed = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<string> m_ModificationAllowedReason = new ReactiveProperty<string>();

	public readonly bool IsOdd;

	public readonly bool HideMarkImage;

	public ReadOnlyReactiveProperty<bool> ModificationAllowed => m_ModificationAllowed;

	public ReadOnlyReactiveProperty<string> ModificationAllowedReason => m_ModificationAllowedReason;

	public SettingsEntityWithValueVM(IUISettingsEntityWithValueBase uiSettingsEntity, bool hideMarkImage)
		: base(uiSettingsEntity)
	{
		m_UISettingsEntity = uiSettingsEntity;
		HideMarkImage = hideMarkImage;
		AddDisposable(IsChanged = m_UISettingsEntity.SettingsEntity.ObserveTempValueIsConfirmed().Not().ToReadOnlyReactiveProperty(initialValue: false));
		AddDisposable(ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(), delegate
		{
			m_ModificationAllowed.Value = m_UISettingsEntity.ModificationAllowed;
		}));
		m_ModificationAllowedReason.Value = m_UISettingsEntity.ModificationAllowedReason;
		IsOdd = false;
	}

	public void ResetToDefault()
	{
		m_UISettingsEntity.SettingsEntity.ResetToDefault(confirmChanges: false);
	}
}
