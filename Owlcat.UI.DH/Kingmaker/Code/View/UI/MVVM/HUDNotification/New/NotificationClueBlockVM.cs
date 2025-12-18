using Code.Framework.Utility.UnityExtensions;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Framework.DetectiveSystem;

namespace Kingmaker.Code.View.UI.MVVM.HUDNotification.New;

public class NotificationClueBlockVM : NotificationContentBlockVM
{
	public readonly BlueprintClue BlueprintClue;

	public NotificationClueBlockVM(BlueprintClue clue, QuestNotificationState state)
		: base(clue.GetUIData().Name, null, state)
	{
		BlueprintClue = clue;
	}

	public void AddAddendum(BlueprintClueAddendum addendum)
	{
		if (!base.Description.IsNullOrEmpty())
		{
			base.Description += "\n";
		}
		base.Description += addendum.Name.Text;
	}
}
