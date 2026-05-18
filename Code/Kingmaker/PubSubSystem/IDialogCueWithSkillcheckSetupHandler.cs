using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IDialogCueWithSkillcheckSetupHandler : ISubscriber
{
	void HandleDialogCueWithSkillcheckSetup();
}
