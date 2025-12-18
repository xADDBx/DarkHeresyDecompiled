using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ICursorNotificationUIHandler : ISubscriber
{
	void HandleNotification(string text, WarningNotificationFormat format);
}
