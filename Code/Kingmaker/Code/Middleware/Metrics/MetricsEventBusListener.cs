using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.Middleware.Metrics;

public class MetricsEventBusListener : IFullScreenUIHandler, ISubscriber
{
	public static void Init()
	{
		EventBus.Subscribe(new MetricsEventBusListener());
	}

	public void HandleFullScreenUiChanged(bool state, FullScreenUIType fullScreenUIType)
	{
		InterfaceMetricsEvent.InterfaceStates state2 = ((!state) ? InterfaceMetricsEvent.InterfaceStates.Close : InterfaceMetricsEvent.InterfaceStates.Open);
		Metrics.Interface.FullScreenType(fullScreenUIType).State(state2).Send();
	}
}
