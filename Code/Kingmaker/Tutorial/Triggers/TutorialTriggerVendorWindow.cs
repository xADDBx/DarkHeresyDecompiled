using Kingmaker.Designers;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("97639975aa885fa4381d47ba79089f50")]
public class TutorialTriggerVendorWindow : TutorialTrigger, IDialogAnswersAddedToPoolHandler, ISubscriber
{
	public void HandleDialogAnswersAddedToPool(BlueprintAnswer answer)
	{
		if (answer.OnSelect.Actions.HasItem((GameAction i) => i is StartTrade))
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.SourceUnit = GameHelper.GetPlayerCharacter();
			});
		}
	}
}
