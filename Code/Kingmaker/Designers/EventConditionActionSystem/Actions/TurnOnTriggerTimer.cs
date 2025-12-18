using Kingmaker.ElementsSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("f9708d74dd7b4303a9e15dbe5679bc37")]
public class TurnOnTriggerTimer : GameAction
{
	public override string GetCaption()
	{
		return "Turn on Tutorial Triggers Counter";
	}

	protected override void RunAction()
	{
		EventBus.RaiseEvent(delegate(ITutorialTriggerTimerHandler i)
		{
			i.HandleTimerStart();
		});
	}
}
