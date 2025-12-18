using System;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IBugReportDescriptionUIHandler : ISubscriber
{
	void HandleFullScreenUIItJustWorks(bool active, FullScreenUIType fullScreenUIType);

	void HandleException(Exception exception);

	void HandleErrorMessages(string[] errorMessages);
}
