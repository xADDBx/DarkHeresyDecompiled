using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class FirstLaunchAccessiabilityPageVM : FirstLaunchSettingsPageVM, ISettingsDescriptionUIHandler, ISubscriber
{
	public readonly SettingsEntitySliderVM FontSize;

	public readonly SettingsEntitySliderVM Protanopia;

	public readonly SettingsEntitySliderVM Deuteranopia;

	public readonly SettingsEntitySliderVM Tritanopia;

	public readonly InfoSectionVM InfoVM;

	private readonly ReactiveProperty<TooltipBaseTemplate> m_ReactiveTooltipTemplate = new ReactiveProperty<TooltipBaseTemplate>();

	public FirstLaunchAccessiabilityPageVM()
	{
		FontSize = new SettingsEntitySliderVM(UISettingsRoot.Instance.UIAccessiabilitySettings.FontSize, SettingsEntitySliderVM.EntitySliderType.FontSizeIndex);
		Protanopia = new SettingsEntitySliderVM(UISettingsRoot.Instance.UIAccessiabilitySettings.Protanopia);
		Deuteranopia = new SettingsEntitySliderVM(UISettingsRoot.Instance.UIAccessiabilitySettings.Deuteranopia);
		Tritanopia = new SettingsEntitySliderVM(UISettingsRoot.Instance.UIAccessiabilitySettings.Tritanopia);
		InfoVM = new InfoSectionVM().AddTo(this);
		m_ReactiveTooltipTemplate.Subscribe(InfoVM.SetTemplate).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
		HandleShowSettingsDescription(FontSize.UISettingsEntity);
	}

	protected override void OnDispose()
	{
		HandleHideSettingsDescription();
	}

	public void HandleShowSettingsDescription(IUISettingsEntityBase entity, string ownTitle = null, string ownDescription = null)
	{
		SetupTooltipTemplate(entity, ownTitle, ownDescription);
	}

	public void HandleHideSettingsDescription()
	{
		SetupTooltipTemplate(null);
	}

	private void SetupTooltipTemplate(IUISettingsEntityBase entity, string ownTitle = null, string ownDescription = null)
	{
		m_ReactiveTooltipTemplate.Value = ((entity != null || ownTitle != null || ownDescription != null) ? TooltipTemplate(entity, ownTitle, ownDescription) : null);
	}

	private TooltipBaseTemplate TooltipTemplate(IUISettingsEntityBase entity, string ownTitle = null, string ownDescription = null)
	{
		return new TooltipTemplateSettingsEntityDescription(entity, ownTitle, ownDescription);
	}
}
