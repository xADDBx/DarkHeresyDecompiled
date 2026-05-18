using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IWarningNotificationUIHandler : ISubscriber
{
	void HandleWarning(WarningNotificationType type, string overrideLabel = null, bool addToLog = true, WarningNotificationFormat? format = null, bool withSound = true);

	void HandleWarning(string text, bool addToLog = true, WarningNotificationFormat format = WarningNotificationFormat.Common, bool withSound = true);
}
