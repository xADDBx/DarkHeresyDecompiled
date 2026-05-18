using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("25bf0d7f92484fb58470008713e8e7a4")]
public class TutorialTriggerSkillcheckInDialogCue : TutorialTrigger, IDialogCueWithSkillcheckSetupHandler, ISubscriber
{
	public void HandleDialogCueWithSkillcheckSetup()
	{
		TryToTrigger(null);
	}
}
