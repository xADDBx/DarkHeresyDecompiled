using JetBrains.Annotations;
using Kingmaker.Framework.DetectiveSystem;
using Owlcat.UI;

namespace Kingmaker.Code.View.UI.MVVM.HUDNotification.New;

public class NotificationCaseBodyVM : ViewModel
{
	[CanBeNull]
	public readonly BlueprintCase Case;

	public NotificationCaseBodyVM([CanBeNull] BlueprintCase @case)
	{
		Case = @case;
	}
}
