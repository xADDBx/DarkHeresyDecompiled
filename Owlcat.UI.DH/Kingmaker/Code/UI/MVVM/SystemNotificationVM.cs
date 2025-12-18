using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.Bridge.Enums;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class SystemNotificationVM : ViewModel
{
	public readonly WarningNotificationType Type;

	public readonly bool ShowWithSound;

	public readonly WarningNotificationFormat Format;

	public readonly string Label;

	public SystemNotificationVM(WarningNotificationType type, bool showWithSound, WarningNotificationFormat format)
	{
		Type = type;
		ShowWithSound = showWithSound;
		Format = format;
		Label = LocalizedTexts.Instance.WarningNotification.GetText(type);
	}

	public SystemNotificationVM(string label, bool showWithSound, WarningNotificationFormat format)
	{
		Type = WarningNotificationType.None;
		ShowWithSound = showWithSound;
		Format = format;
		Label = label;
	}
}
