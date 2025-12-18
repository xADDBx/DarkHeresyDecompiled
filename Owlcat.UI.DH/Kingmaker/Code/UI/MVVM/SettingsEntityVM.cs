using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public abstract class SettingsEntityVM : VirtualListElementVMBase, ILocalizationHandler, ISubscriber
{
	public readonly LocalizedString Title;

	public readonly string Description;

	public readonly bool IsConnector;

	public readonly bool IsSet;

	public readonly IUISettingsEntityBase UISettingsEntity;

	private readonly ReactiveCommand<Unit> m_LanguageChanged = new ReactiveCommand<Unit>();

	public Observable<Unit> LanguageChanged => m_LanguageChanged;

	public SettingsEntityVM(IUISettingsEntityBase uiSettingsEntity)
	{
		UISettingsEntity = uiSettingsEntity;
		Title = uiSettingsEntity.Description;
		Description = ((!string.IsNullOrWhiteSpace(uiSettingsEntity.TooltipDescription)) ? ((string)uiSettingsEntity.TooltipDescription) : string.Empty);
		IsConnector = uiSettingsEntity.ShowVisualConnection && !uiSettingsEntity.IAmSetHandler;
		IsSet = uiSettingsEntity.ShowVisualConnection && uiSettingsEntity.IAmSetHandler;
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
	}

	public void HandleLanguageChanged()
	{
		m_LanguageChanged.Execute();
	}
}
