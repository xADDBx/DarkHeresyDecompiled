using Kingmaker.PubSubSystem.Core;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class ExperienceNotificationVM : NotificationVM
{
	private readonly ReactiveProperty<string> m_ShowExperienceAmount = new ReactiveProperty<string>(string.Empty);

	public ReadOnlyReactiveProperty<string> ShowExperienceAmount => m_ShowExperienceAmount;

	public ExperienceNotificationVM()
	{
		EventBus.Subscribe(this).AddTo(this);
	}

	public void HandleExperienceNotification(int amount)
	{
		m_ShowExperienceAmount.Value = amount.ToString();
		ShowNotification();
	}
}
