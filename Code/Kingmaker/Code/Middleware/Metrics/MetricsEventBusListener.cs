using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.Middleware.Metrics;

public class MetricsEventBusListener : IFullScreenUIHandler, ISubscriber, IDialogInteractionHandler, IUIEventHandler
{
	public static void Init()
	{
		EventBus.Subscribe(new MetricsEventBusListener());
	}

	public void HandleFullScreenUiChanged(bool state, FullScreenUIType fullScreenUIType)
	{
		if (fullScreenUIType != 0)
		{
			InterfaceMetricsEvent.InterfaceStates state2 = ((!state) ? InterfaceMetricsEvent.InterfaceStates.Close : InterfaceMetricsEvent.InterfaceStates.Open);
			Metrics.Interface.FullScreenType(fullScreenUIType).State(state2).Send();
		}
	}

	public void StartDialogInteraction(BlueprintDialog dialog)
	{
		Metrics.Dialog.Id(dialog.AssetGuid).State(DialogMetricsEvent.DialogState.Open).Send();
	}

	public void StopDialogInteraction(BlueprintDialog dialog)
	{
		Metrics.Dialog.Id(dialog.AssetGuid).State(DialogMetricsEvent.DialogState.Close).Send();
	}

	public void HandleUIEvent(UIEventType type)
	{
		if (type == UIEventType.FormationWindowOpen)
		{
			Metrics.Interface.Type(InterfaceMetricsEvent.InterfaceTypes.Formation).State(InterfaceMetricsEvent.InterfaceStates.Open).Send();
		}
		if (type == UIEventType.FormationWindowClose)
		{
			Metrics.Interface.Type(InterfaceMetricsEvent.InterfaceTypes.Formation).State(InterfaceMetricsEvent.InterfaceStates.Close).Send();
		}
	}
}
