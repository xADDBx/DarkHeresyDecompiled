using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("e239b162c2df4ad1a9a82b744cd5c862")]
public class TutorialTriggerGlobalMapEnter : TutorialTrigger, ICloseLoadingScreenHandler, ISubscriber
{
	public void HandleCloseLoadingScreen()
	{
		if (Game.Instance.CurrentlyLoadedArea.AreaStatGameMode == GameModeType.GlobalMap)
		{
			TryToTrigger(null);
		}
	}
}
