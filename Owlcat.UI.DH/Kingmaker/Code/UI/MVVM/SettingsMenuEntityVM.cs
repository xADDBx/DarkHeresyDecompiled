using System;
using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class SettingsMenuEntityVM : SelectionGroupEntityVM, ILocalizationHandler, ISubscriber
{
	private readonly ReactiveProperty<string> m_Title = new ReactiveProperty<string>();

	private Action<UISettingsManager.SettingsScreen> m_ConfirmAction;

	private readonly LocalizedString m_TitleString;

	public ReadOnlyReactiveProperty<string> Title => m_Title;

	public UISettingsManager.SettingsScreen SettingsScreenType { get; }

	public SettingsMenuEntityVM(LocalizedString title, UISettingsManager.SettingsScreen screenType, Action<UISettingsManager.SettingsScreen> confirmAction)
		: base(allowSwitchOff: false)
	{
		m_TitleString = title;
		SettingsScreenType = screenType;
		m_ConfirmAction = confirmAction;
		UpdateTitle();
		AddDisposable(EventBus.Subscribe(this));
	}

	public void Confirm()
	{
		DoSelectMe();
	}

	protected override void DoSelectMe()
	{
		m_ConfirmAction?.Invoke(SettingsScreenType);
	}

	protected override void DisposeImplementation()
	{
		m_ConfirmAction = null;
	}

	public void UpdateTitle()
	{
		m_Title.Value = m_TitleString.Text;
	}

	public void HandleLanguageChanged()
	{
		UpdateTitle();
	}
}
