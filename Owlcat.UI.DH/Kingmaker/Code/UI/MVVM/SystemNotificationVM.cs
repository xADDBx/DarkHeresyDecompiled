using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UI.Sound;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class SystemNotificationVM : ViewModel
{
	public readonly WarningNotificationType Type;

	public readonly WarningNotificationFormat Format;

	public readonly string Label;

	private readonly bool m_ShowWithSound;

	public SystemNotificationVM(WarningNotificationType type, string label, bool showWithSound, WarningNotificationFormat format)
	{
		Type = type;
		m_ShowWithSound = showWithSound;
		Format = format;
		Label = label ?? LocalizedTexts.Instance.WarningNotification.GetText(type);
	}

	public SystemNotificationVM(string label, bool showWithSound, WarningNotificationFormat format)
	{
		Type = WarningNotificationType.None;
		m_ShowWithSound = showWithSound;
		Format = format;
		Label = label;
	}

	public UISound GetSound()
	{
		if (!m_ShowWithSound)
		{
			return UISounds.Instance.Sounds.DoNothingEvent;
		}
		if (Type != 0)
		{
			return NotificationsSounds.Instance.SystemNotifications.GetSound(Type);
		}
		return NotificationsSounds.Instance.SystemNotifications.GetSound(Format);
	}
}
