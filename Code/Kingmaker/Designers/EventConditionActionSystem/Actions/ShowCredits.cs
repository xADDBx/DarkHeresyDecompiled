using Kingmaker.ElementsSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("76367edfd4564f4d86bd2372754cb476")]
public class ShowCredits : GameAction
{
	public override string GetCaption()
	{
		return "Show Credits";
	}

	protected override void RunAction()
	{
		EventBus.RaiseEvent(delegate(ICreditsWindowUIHandler h)
		{
			h.HandleOpenCredits();
		});
	}
}
